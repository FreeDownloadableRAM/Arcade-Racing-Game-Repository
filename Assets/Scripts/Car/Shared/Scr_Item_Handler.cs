using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Scr_Item_Handler : MonoBehaviour
{
    // item held
    [SerializeField] private string itemHeld;
    // 0 = none
    // 1 = nitro

    // item box script reference
    private Scr_Item_Box scrItemBox;

    // is item available
    private bool isItemAvailable;

    // for nitro use
    private float nitroDuration = 3.0f; // duration of nitro effect in seconds
    private float nitroTimer = 0.0f; // timer to track nitro duration

    private float originalAcceleration = 0f;
    private float originalMaxSpeed = 0f;

    private bool nitroActive = false;

    // AI car controller reference
    private CarControllerAI scr_CarControllerAI;

    // player car controller reference
    private CarController scr_CarController;

    // get our car rigidbody
    private Rigidbody carRigidbody;

    // car health reference
    private Scr_Car_Health scr_CarHealth;

    // get car particle handler reference
    private Scr_Particle_Handler scr_ParticleHandler;

    // car specific parameters
    // rocket spawn offset
    [SerializeField] private float rocketSpawnOffset = 3.75f;
    [SerializeField] private float rocketSpawnHeightOffset = 1.25f;

    // laser item object reference
    [SerializeField] private GameObject laserItemPrefab;

    // rocket item object reference
    [SerializeField] private GameObject rocketItemPrefab;

    // missile object reference
    [SerializeField] private GameObject missileItemPrefab;

    // flamethrower object reference
    [SerializeField] private GameObject flamethrowerItemPrefab;

    // shield object reference 
    [SerializeField] private GameObject shieldItemPrefab;

    // shield dispersion effect prefab
    [SerializeField] private GameObject shieldDispersionEffectPrefab;

    // Ghost balls object reference
    [SerializeField] private GameObject ghostItemPrefab;

    // beam item prefabs
    // charge
    [SerializeField] private GameObject shockBeamItemChargePrefab;
    // charge duration
    private float beamChargeTimer = 0f;
    private float beamChargeDuration = 1f;
    private float beamDestructionTimer = 3f;
    private bool isChargeBeamActive = false;

    // fire
    [SerializeField] private GameObject shockBeamItemFirePrefab;
    // private float beamFireTimer = 0f;
    private float beamFireDuration = 0.5f;
    // private bool isShockBeamActive = false;

    [SerializeField] private float beamSpawnHeightOffset = 0.4f;


    // helpers
    private bool isShieldActive = false;
    private float shieldDuration = 5.0f;
    private float shieldTimer = 0.0f;

    // get car ai simple script
    private CarAISimple scr_CarAISimple;

    // for position based items -----------------------------
    // track Racer position
    private int position;

    // get race manager script from race manager game object
    // race track object
    private GameObject RaceTrackObject;

    // race manager script reference
    private scr_RaceCheckpoints scr_raceCheckpointsScript;

    // object racer to keep track of
    private GameObject Racer;

    // for firing multiple lasers
    // create a short 3 round burst of lasers
    float timeBetweenShots = 0.0f; // time between each shot in seconds
    int numberOfShots = 3; // number of shots in the burst

    // car item usage behaviour script
    private Scr_Car_AI_Item_Behaviour scr_CarAIItemBehaviour;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // On start, set item held to none
        itemHeld = "None"; // "None"

        // debug
        // itemHeld = "Laser"; // "Laser"

        // get rigidbody component
        carRigidbody = GetComponent<Rigidbody>();

        // get car controller ai if ai
        scr_CarControllerAI = GetComponent<CarControllerAI>();

        // get car controller if player
        scr_CarController = GetComponent<CarController>();

        // get particle handler component
        scr_ParticleHandler = GetComponent<Scr_Particle_Handler>();

        // get these variables so that we can reset car acceleration and max speed after nitro use
        originalAcceleration = scr_CarControllerAI.getAcceleration();
        originalMaxSpeed = scr_CarControllerAI.getMaxSpeed();

        scr_CarHealth = GetComponent<Scr_Car_Health>();

        scr_CarAISimple = GetComponent<CarAISimple>();

        // so the item behaviour and item handler can communicate
        scr_CarAIItemBehaviour = GetComponent<Scr_Car_AI_Item_Behaviour>();

        // find the race track object in the scene
        // this will have the racers placement data that we need to home in on the correct target
        RaceTrackObject = GameObject.FindWithTag("Race");

        // get the race checkpoints script from the race track object
        scr_raceCheckpointsScript = RaceTrackObject.GetComponent<scr_RaceCheckpoints>();

        // set racer to this game object
        Racer = gameObject;
    }

    // fixed update
    private void FixedUpdate()
    {
        
        // check if nitro is even active or not
        if (nitroActive) 
        {
            // if nitro timer is above 0, count down timer
            if (nitroTimer > 0.0f)
            {
                // count down nitro timer
                nitroTimer -= Time.fixedDeltaTime;
            }
            else
            {
                // are we an ai or player car?
                if (gameObject.CompareTag("AI"))
                {
                    // reset car acceleration and max speed
                    scr_CarControllerAI.setAcceleration(originalAcceleration);
                    scr_CarControllerAI.setMaxSpeed(originalMaxSpeed);

                }
                else
                {
                    // reset car acceleration and max speed
                    scr_CarController.setAcceleration(originalAcceleration);
                    scr_CarController.setMaxSpeed(originalMaxSpeed);

                }

                // set nitro active to false
                nitroActive = false;

                // debug, log nitro end
                // Debug.Log("Nitro ended for " + gameObject.name);

            }
        }

        // count down shield timer if shield is active
        if (isShieldActive)
        {
            // shield is active, count up timer
            // once it equals or is higher than shield duration, deactivate shield
            shieldTimer += Time.fixedDeltaTime;

            if (shieldTimer >= shieldDuration)
            {
                // create shield dispersion effect
                GameObject shieldDispersionObject = Instantiate(shieldDispersionEffectPrefab, transform.position, transform.rotation);

                // get our car speed to give it to the shield dispersion effect
                float carSpeed = carRigidbody.linearVelocity.magnitude;

                shieldDispersionObject.GetComponentInChildren<Scr_Item_Shield_Dispersion>().SetInitialSpeed(carSpeed);

                // deactivate shield
                isShieldActive = false;
                shieldTimer = 0.0f; // reset timer

            }

        }

        // count down shield timer if shield is active
        if (isChargeBeamActive)
        {
            // shield is active, count up timer
            // once it equals or is higher than shield duration, deactivate shield
            beamChargeTimer += Time.fixedDeltaTime;

            if (beamChargeTimer >= beamChargeDuration)
            {
                // Create Chield object in the front of the car
                // get car position
                Vector3 carPosition = scr_CarAISimple.getCarOrigin();

                // get car forward direction
                Vector3 carForward = transform.forward;

                // get car rotation angle
                Quaternion carRotation = transform.rotation;

                // calculate spawn position for rocket (in front of car)
                //Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + scr_CarAISimple.getCarOrigin(); // adjust offsets as needed

                Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * beamSpawnHeightOffset; // adjust offsets as needed

                // set projectile rotation to match car rotation
                Quaternion spawnRotation = carRotation;

                // create actual firing beam
                GameObject shockBeamFireObject = Instantiate(shockBeamItemFirePrefab, spawnPosition, spawnRotation, transform);

                // timer to send to object so that it can self destruct after duration
                shockBeamFireObject.GetComponentInChildren<Scr_Item_Ion_Beam>().SetBeamFireDestructionTimer(beamFireDuration);

                // deactivate shield
                isChargeBeamActive = false;
                beamChargeTimer = 0.0f; // reset timer

            }

        }




        // update racer position data
        // if we are at item 0 in the racer list, we are in first place, but this will return 1.
        // So if we want to get the position ahead of us, from us, we subtract 2 from our position.
        // thats the value of the racer that the homing target should be heading towards.
        position = scr_raceCheckpointsScript.GetRacerPosition(Racer);


    }


    // on collision with item box, give item to player
    private void OnTriggerEnter(Collider other)
    {
        // if we collide with an item box
        if (other.gameObject.CompareTag("ItemBox"))
        {
            // check if the item box item is available
            scrItemBox = other.gameObject.GetComponent<Scr_Item_Box>();

            isItemAvailable = other.gameObject.GetComponent<Scr_Item_Box>().IsItemAvailable();

            if (isItemAvailable)
            {
                // check if we arent holding an item already
                if (itemHeld == "None")
                {
                    // we can collect item
                    // generate random number from 0 to 1
                    float randomItem = Random.Range(0f, 1f);

                    if (randomItem < 0.015f)
                    {
                        // give item to player
                        itemHeld = "Nitro"; // nitro

                    }
                    else if (randomItem < 0.025f)
                    {
                        // give item to player
                        itemHeld = "Rocket"; // Rocket

                    }
                    else if (randomItem < 0.035f)
                    {
                        // give item to player
                        itemHeld = "Missile"; // Missile

                    }
                    else if (randomItem < 0.045f)
                    {
                        // give item to player
                        itemHeld = "Laser"; // Laser

                    }
                    else if (randomItem < 0.055f)
                    {
                        // give item to player
                        itemHeld = "Flamethrower"; // Flamethrower

                    }
                    else if (randomItem < 0.065f)
                    {
                        // give item to player
                        itemHeld = "Shield"; // Shield

                    }
                    else if (randomItem < 0.075f)
                    {
                        // give item to player
                        itemHeld = "Ghosts"; // Ghost Balls

                    }
                    else if (randomItem < 0.85f)
                    {
                        // give item to player
                        itemHeld = "Ion Beam"; // Shock Beam

                    }
                    else
                    {
                        // give health pack to player
                        itemHeld = "Health Pack"; // health pack
                    }
                }
                
                // debug log if we hit an item box
                //Debug.Log("Item Box collected by " + gameObject.name);

                // set item box to unavailable
                scrItemBox.SetItemAvailable(false);

            }

            // debug log if we hit an item box
            //Debug.Log("Item Box collided with " + other.gameObject.name);
            //Debug.Log(" Item box: " + other.gameObject);
            //Debug.Log(" Available?: " + isItemAvailable);

        }
        
    }

    // This function returns the item held by the player
    public string getItemHeld() 
    { 
        return itemHeld;

    }

    // set item held to none after use
    public void clearItemHeld() 
    { 
        itemHeld = "None";
    }

    // get if nitro is active
    public bool isNitroActive() 
    { 
        return nitroActive;
    }

    // item use functions
    // nitro use function
    public void UseItemNitro() 
    {
        // debug, log nitro use
        // Debug.Log("Nitro used by " + gameObject.name);

        // set nitro active
        nitroActive = true;

        // set nitro timer
        nitroTimer = nitroDuration;

        // are we an ai or player car?
        if (gameObject.CompareTag("AI"))
        {
            // increase car acceleration and max speed
            scr_CarControllerAI.setAcceleration(originalAcceleration * 1.25f); // increase acceleration by 25%
            scr_CarControllerAI.setMaxSpeed(originalMaxSpeed * 1.15f); // increase max speed by 15%

        }
        else 
        {
            // increase car acceleration and max speed
            scr_CarController.setAcceleration(originalAcceleration * 1.25f); // increase acceleration by 25%
            scr_CarController.setMaxSpeed(originalMaxSpeed * 1.2f); // increase max speed by 20%

        }

        
        // once done, clear item
        clearItemHeld();
        
    }

    // health pack use function
    public void UseItemHealthPack()
    {
        // health pack heal amount
        // 50% of max health
        int healAmount = Mathf.RoundToInt(scr_CarHealth.GetMaxHealth() * 0.5f);

        // get our current health
        int currentHealth = scr_CarHealth.GetCurrentHealth();

        // calculate new health amount after heal
        int newHealth = currentHealth + healAmount;

        // clamp new health to max health
        if (newHealth > scr_CarHealth.GetMaxHealth())
        {
            newHealth = scr_CarHealth.GetMaxHealth();
        }

        // if our current health is below zero, we cant heal
        if (currentHealth <= 0)
        {
            // restore car health
            scr_CarHealth.SetCurrentHealth(0);

            // play heal particles
            scr_ParticleHandler.PlayHealParticles();

            // cant heal, car is destroyed
            // clear item held
            clearItemHeld();
            return;
        }

        // restore car health
        scr_CarHealth.SetCurrentHealth(newHealth);

        // play heal particles
        scr_ParticleHandler.PlayHealParticles();

        // clear item held
        clearItemHeld();

    }

    // Laser Use function
    public void UseItemLaser()
    {
        // Create laser projectile and launch it forward from the car
        // get car position
        Vector3 carPosition = transform.position;

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
        //Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + scr_CarAISimple.getCarOrigin(); // adjust offsets as needed

        Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * rocketSpawnHeightOffset; // adjust offsets as needed

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        if (timeBetweenShots > 0)
        {
            timeBetweenShots -= Time.fixedDeltaTime;

        }
        else 
        {
            // check if we have shots left to fire
            if (numberOfShots > 0)
            {
                // timer reached zero, fire laser
                // fire one laser slightly to the left and one slightly to the right of center
                Vector3 leftOffset = transform.right * -0.5f; // adjust offset as needed
                Vector3 rightOffset = transform.right * 0.5f; // adjust offset as needed

                GameObject laserProjectileLeft = Instantiate(laserItemPrefab, spawnPosition + leftOffset, spawnRotation);
                GameObject laserProjectileRight = Instantiate(laserItemPrefab, spawnPosition + rightOffset, spawnRotation);

                // count down number of shots left
                numberOfShots -= 1;

                // reset timer
                timeBetweenShots = 0.25f;
            }
            else 
            {
                scr_CarAIItemBehaviour.setFireLaserBurstToggle(false);

                // clear item held
                clearItemHeld();

                // reset number of shots for next time
                numberOfShots = 3;
                timeBetweenShots = 0.0f;

            }

        }


        
    }

    // Flamethrower  use function 
    public void UseItemFlamethrower()
    {
        // Create flamethrower object in front of the car
        // get car position
        Vector3 carPosition = transform.position;

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
        //Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + scr_CarAISimple.getCarOrigin(); // adjust offsets as needed

        Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * rocketSpawnHeightOffset; // adjust offsets as needed

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        // we want to create this object as a child of the car so that it moves with the car
        GameObject flamethrowerObject = Instantiate(flamethrowerItemPrefab, spawnPosition, spawnRotation, transform);
       
        // clear item held
        clearItemHeld();
    }

    // Shield use function 
    public void UseItemShield()
    {
        // Create Chield object in the CENTER of the car
        // get car position
        Vector3 carPosition = scr_CarAISimple.getCarOrigin();

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
        //Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + scr_CarAISimple.getCarOrigin(); // adjust offsets as needed

        Vector3 spawnPosition = carPosition;

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        // we want to create this object as a child of the car so that it moves with the car
        GameObject ShieldObject = Instantiate(shieldItemPrefab, spawnPosition, spawnRotation, transform);

        // set destruction timer for shield
        ShieldObject.GetComponentInChildren<Scr_Item_Shield>().SetShieldDestructionTimer(shieldDuration); // shield lasts for 5 seconds

        // set shield active to true
        isShieldActive = true;

        // clear item held
        clearItemHeld();
    }

    // Shock Beam Use function
    public void UseItemIonBeam()
    {
        // Create Chield object in the front of the car
        // get car position
        Vector3 carPosition = scr_CarAISimple.getCarOrigin();

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
        //Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + scr_CarAISimple.getCarOrigin(); // adjust offsets as needed

        Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * beamSpawnHeightOffset; // adjust offsets as needed

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        // we want to create this object as a child of the car so that it moves with the car
        GameObject ShockBeamChargeObject = Instantiate(shockBeamItemChargePrefab, spawnPosition, spawnRotation, transform);

        // set destruction timer for shield
        ShockBeamChargeObject.GetComponentInChildren<Scr_Item_Ion_Beam_Charge>().SetBeamChargeDestructionTimer(beamDestructionTimer); // shield lasts for 5 seconds

        // set shock beam charge active to true
        isChargeBeamActive = true;

        // clear item held
        clearItemHeld();
    }

    

    // return if shield is active for car health script
    public bool IsShieldActive()
    {
        return isShieldActive;
    }

    // Rocket use function
    public void UseItemRocket()
    {
        // Create Rocket projectile and launch it forward from the car
        // get car position
        Vector3 carPosition = transform.position;

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
        //Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + scr_CarAISimple.getCarOrigin(); // adjust offsets as needed

        Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * rocketSpawnHeightOffset; // adjust offsets as needed

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        // create the rocket projectile from prefab and set its initial rocket speed to 110
        GameObject rocketProjectile = Instantiate(rocketItemPrefab, spawnPosition, spawnRotation);
        
        // get our own car's linear velocity to set the initial rocket speed accordingly
        float carSpeed = carRigidbody.linearVelocity.magnitude;

        rocketProjectile.GetComponentInChildren<Scr_Item_Rocket>().SetInitialRocketSpeed(1.1f * carSpeed); // set initial rocket speed to 110% of car speed

        // clear item held
        clearItemHeld();
    }

    // Rocket use function
    public void UseItemMissile()
    {
        // Create Missile projectile and launch it forward from the car
        // get car position
        Vector3 carPosition = transform.position;

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
     
        Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * rocketSpawnHeightOffset; // adjust offsets as needed

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        // set the homing target
        int targetPositionIndex = position - 1; // get the position index of the racer ahead of us

        // make sure target position index is within bounds
        if (targetPositionIndex < 0)
        {
            // set target position index to last racer if out of range
            targetPositionIndex = scr_raceCheckpointsScript.Racers.Count() - 1;

        }

        // this gets the actual racer game object that we need to home in on based on their position
        GameObject homingTarget = scr_raceCheckpointsScript.GetRacerByPosition(targetPositionIndex);

        // get the car height off ground ray cast offset from the target car to aim for
        // so that we aim at the center of the car rather than the ground
        Vector3 targetCarHeightOffset = homingTarget.GetComponent<CarAISimple>().getCarHeightOffGroundRaycastOffset();

        // create the missile projectile from prefab and set its initial rocket speed 
        GameObject missileProjectile = Instantiate(missileItemPrefab, spawnPosition, spawnRotation);

        // get our own car's linear velocity to set the initial rocket speed accordingly
        float carSpeed = carRigidbody.linearVelocity.magnitude;

        missileProjectile.GetComponentInChildren<Scr_Item_Missile>().SetInitialMissileSpeed(1.2f * carSpeed); // set initial rocket speed to 110% of current car speed

        // set homing target for missile
        missileProjectile.GetComponentInChildren<Scr_Item_Missile>().SetHomingTarget(homingTarget.transform);

        // set target height offset to target center of target
        missileProjectile.GetComponentInChildren<Scr_Item_Missile>().SetHomingTargetHeightOffset(targetCarHeightOffset);

        // clear item held
        clearItemHeld();
    }

    // Rocket use function
    public void UseItemGhosts()
    {
        // Create Ghost projectiles and launch it forward from the car
        // get car position
        Vector3 carPosition = transform.position;

        // get car forward direction
        Vector3 carForward = transform.forward;

        // get car rotation angle
        Quaternion carRotation = transform.rotation;

        // calculate spawn position for rocket (in front of car)
        Vector3 spawnPosition = carPosition + (carForward * rocketSpawnOffset) + transform.up * rocketSpawnHeightOffset; // adjust offsets as needed

        // set projectile rotation to match car rotation
        Quaternion spawnRotation = carRotation;

        // we will create 3 ghosts that home in on the next 3 racers ahead of us
        // set the homing target
        int targetPositionIndex = position - 1; // get the position index of the racer ahead of us
        int targetPositionIndexTwo = position - 2; // second target
        int targetPositionIndexThree = position - 3; // third target

        // make sure target position index is within bounds
        if (targetPositionIndex < 0)
        {
            // set target position index to last racer if out of range
            targetPositionIndex = scr_raceCheckpointsScript.Racers.Count() - 1;

        }
        if (targetPositionIndexTwo < 0)
        {
            // set target position index to last racer if out of range
            targetPositionIndexTwo = scr_raceCheckpointsScript.Racers.Count() - 2;
        }
        if (targetPositionIndexThree < 0)
        {
            // set target position index to last racer if out of range
            targetPositionIndexThree = scr_raceCheckpointsScript.Racers.Count() - 3;
        }

        // this gets the actual racer game object that we need to home in on based on their position
        GameObject homingTarget = scr_raceCheckpointsScript.GetRacerByPosition(targetPositionIndex);

        // target two
        GameObject homingTargetTwo = scr_raceCheckpointsScript.GetRacerByPosition(targetPositionIndexTwo);

        // target three
        GameObject homingTargetThree = scr_raceCheckpointsScript.GetRacerByPosition(targetPositionIndexThree);


        // get the car height off ground ray cast offset from the target car to aim for
        // so that we aim at the center of the car rather than the ground
        Vector3 targetCarHeightOffset = homingTarget.GetComponent<CarAISimple>().getCarHeightOffGroundRaycastOffset();

        // target two
        Vector3 targetCarHeightOffsetTwo = homingTargetTwo.GetComponent<CarAISimple>().getCarHeightOffGroundRaycastOffset();

        // target three
        Vector3 targetCarHeightOffsetThree = homingTargetThree.GetComponent<CarAISimple>().getCarHeightOffGroundRaycastOffset();

        // Target one
        // create the Ghost projectile from prefab and set its initial ghost speed 
        GameObject GhostProjectile = Instantiate(ghostItemPrefab, spawnPosition, spawnRotation);

        // get our own car's linear velocity to set the initial rocket speed accordingly
        float carSpeed = carRigidbody.linearVelocity.magnitude;

        GhostProjectile.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetInitialGhostSpeed(1.2f * carSpeed); // set initial rocket speed to 110% of current car speed

        // set homing target for Ghost
        GhostProjectile.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetHomingTarget(homingTarget.transform);

        // set target height offset to target center of target
        GhostProjectile.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetHomingTargetHeightOffset(targetCarHeightOffset);

        // Target two
        // create the Ghost projectile from prefab and set its initial ghost speed 
        GameObject GhostProjectileTwo = Instantiate(ghostItemPrefab, spawnPosition, spawnRotation);

        GhostProjectileTwo.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetInitialGhostSpeed(1.2f * carSpeed); // set initial rocket speed to 110% of current car speed

        // set homing target for Ghost
        GhostProjectileTwo.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetHomingTarget(homingTargetTwo.transform);

        // set target height offset to target center of target
        GhostProjectileTwo.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetHomingTargetHeightOffset(targetCarHeightOffsetTwo);

        // Target three
        // create the Ghost projectile from prefab and set its initial ghost speed 
        GameObject GhostProjectileThree = Instantiate(ghostItemPrefab, spawnPosition, spawnRotation);

        GhostProjectileThree.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetInitialGhostSpeed(1.2f * carSpeed); // set initial rocket speed to 110% of current car speed

        // set homing target for Ghost
        GhostProjectileThree.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetHomingTarget(homingTargetThree.transform);

        // set target height offset to target center of target
        GhostProjectileThree.GetComponentInChildren<Scr_Item_Ghost_Ball>().SetHomingTargetHeightOffset(targetCarHeightOffsetThree);


        // clear item held
        clearItemHeld();
    }


    // get rocket spawn offset
    public float getRocketSpawnOffset()
    {
        return rocketSpawnOffset;
    }

    // get rocket spawn height offset
    public float getRocketSpawnHeightOffset()
    {
        return rocketSpawnHeightOffset;
    }

}
