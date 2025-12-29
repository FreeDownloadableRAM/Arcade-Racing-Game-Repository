using UnityEngine;

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
    // rocket item object reference
    [SerializeField] private GameObject rocketItemPrefab;

    // get car ai simple script
    private CarAISimple scr_CarAISimple;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // On start, set item held to none
        itemHeld = "None"; // "None"

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
                Debug.Log("Nitro ended for " + gameObject.name);

            }
        }

        // check if we have health pack item
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

                    if (randomItem < 0.25f)
                    {
                        // give item to player
                        itemHeld = "Nitro"; // nitro

                    }
                    else if (randomItem < 0.75f)
                    {
                        // give item to player
                        itemHeld = "Rocket"; // Rocket

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

        // restore car health
        scr_CarHealth.SetCurrentHealth(newHealth);

        // play heal particles
        scr_ParticleHandler.PlayHealParticles();

        // clear item held
        clearItemHeld();

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

        rocketProjectile.GetComponentInChildren<Scr_Item_Rocket>().SetInitialRocketSpeed(1.1f * carSpeed); // set initial rocket speed to 110 + car speed

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
