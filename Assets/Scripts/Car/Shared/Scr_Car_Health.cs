using UnityEngine;

public class Scr_Car_Health : MonoBehaviour
{
    // car health
    [SerializeField] private int carHealth = 200;

    // internal car health
    [SerializeField] private int internalCarHealth;

    // collision switch
    private bool calculateCollisionDamage;

    // reference i am stuck script so that we can reference its last checkpoint passed
    private Scr_I_am_stuck scr_IAmStuck;

    // car rigidbody reference
    private Rigidbody carRigidbody;

    // reference particle handler
    private Scr_Particle_Handler Scr_ParticleHandler;

    // get car controller for the ai
    private CarControllerAI scr_CarControllerAI;

    // get car controller for the player
    private CarController scr_CarController;

    // original max speed and acceleration to restore after reset
    float originalMaxSpeed = 0f;
    float originalAcceleration = 0f;

    // original car mass
    float originalCarMass = 0f;

    // race progress script reference
    private scr_My_Race_Progress scr_MyRaceProgress;

    // car death timer
    private float carDeathTimer = 5f;

    // get rocket script component
    private Scr_Item_Rocket scr_ItemRocket;

    // get missile script component
    private Scr_Item_Missile scr_ItemMissile;

    // get laser script component
    private Scr_Item_Pierce_Laser scr_PierceLaser;

    // get ghost ball script component
    private Scr_Item_Ghost_Ball scr_ItemGhost;

    // get flamethrower script component
    [SerializeField] private Scr_Item_Flamethrower scr_Flamethrower;

    // get Ion Beam script component
    private Scr_Item_Ion_Beam scr_ItemIonBeam;

    // get orbital ray script component
    private Scr_OrbitalRayFire scr_ItemOrbitalRayFire;

    // get item handler script component
    private Scr_Item_Handler scr_itemHandler;

    // car ai script reference to get car origin position
    private CarAISimple scr_CarAISimple;

    // Layer mask to look for collisions when trying to place a car back on a checkpoint after death.
    // check for cops, cars, and players.
    private LayerMask spawnBlockingLayers;

    // spawn blocking box cast dimensions, based on car collider size, to check for blocking objects when respawning after death
    private Vector3 spawnBlockingBoxCastDimensions;

    // flame damage trigger flag
    private bool inFlameArea = false;

    // Ion Beam damage trigger flag
    private bool inIonBeam = false;

    // Orbital Ray Damage trigger flag
    private bool inOrbitalRay = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set internal car health to car health at start
        internalCarHealth = carHealth;

        calculateCollisionDamage = true;

        // reference i am stuck script
        scr_IAmStuck = GetComponent<Scr_I_am_stuck>();

        // reference car rigidbody
        carRigidbody = GetComponent<Rigidbody>();

        // reference to car ai script
        scr_CarAISimple = GetComponent<CarAISimple>();

        // reference particle handler
        Scr_ParticleHandler = GetComponent<Scr_Particle_Handler>();

        // reference item handler script
        scr_itemHandler = GetComponent<Scr_Item_Handler>();

        // get race progress script reference 
        scr_MyRaceProgress = GetComponent<scr_My_Race_Progress>();

        // if this is an ai car, get its controller
        if (gameObject.CompareTag("AI"))
        {
            scr_CarControllerAI = GetComponent<CarControllerAI>();

            // get original speed and acceleration values
            originalMaxSpeed = scr_CarControllerAI.getMaxSpeed();
            originalAcceleration = scr_CarControllerAI.getAcceleration();

        }
        if (gameObject.CompareTag("Player"))
        {
            scr_CarController = GetComponent<CarController>();

            // get original speed and acceleration values
            originalMaxSpeed = scr_CarController.getMaxSpeed();
            originalAcceleration = scr_CarController.getAcceleration();

        }

        // get original car mass
        originalCarMass = carRigidbody.mass;

        // define spawn blocking layers
        spawnBlockingLayers = LayerMask.GetMask("Cars", "Cops", "PlayerCars");

        // set spawn blocking box cast dimensions based on car collider size, we will use this to check for blocking objects when respawning after death
        if (TryGetComponent<BoxCollider>(out BoxCollider carBoxCollider))
        {
            spawnBlockingBoxCastDimensions = carBoxCollider.size;
        }
        else
        {
            // if we cannot find a box collider, use default dimensions for the box cast
            spawnBlockingBoxCastDimensions = new Vector3(3f, 3f, 4f); // adjust as needed based on average car size
        }

    }

    void FixedUpdate()
    {
        // if health is 0, place car back to last checkpoint passed
        if (internalCarHealth <= 0)
        {
            
            ResetToLastCheckpoint();

        }

        // if flame toggle is true, apply damage over time
        if (inFlameArea)
        {
            // check if flamethrower script is assigned
            if (scr_Flamethrower == null)
            {
                return;
            }

            // check if shield is active on car
            // reference it from item handler script
            if (scr_itemHandler != null)
            {
                if (scr_itemHandler.IsShieldActive())
                {
                    // if shield is active, do not apply collision damage
                    return;
                }
            }

            // first check if particles are still being spawned
            if (scr_Flamethrower.GetParticleSpawnDurationValue() <= 0)
            {
                // exit if particles are not being spawned anymore
                return;

            }

            // debug log flamethrower hit
            // Debug.Log("Laser hit detected on " + gameObject.name);

            // get damage amount from flamethrower script
            int flamethrowerDamage = scr_Flamethrower.GetFlamethrowerDamagePerSecondAmount();

            // calculate damage per fixed update frame
            flamethrowerDamage = Mathf.RoundToInt(flamethrowerDamage * Time.fixedDeltaTime);

            // subtract rocket damage from car health
            internalCarHealth -= Mathf.RoundToInt(flamethrowerDamage);

            // calculateCollisionDamage = false;

            // if health is below 1, play car destruction particles
            if (internalCarHealth < 1)
            {
                Scr_ParticleHandler.PlayCarDestructionParticles();
            }

        }

        // if Ion Beam toggle is true, apply damage over time
        if (inIonBeam)
        {
            // check if Ion Beam script is assigned
            if (scr_ItemIonBeam == null)
            {
                //Debug.Log("Ion Beam script is null on " + gameObject.name);
                return;
            }

            // check if shield is active on car
            // reference it from item handler script
            if (scr_itemHandler != null)
            {
                if (scr_itemHandler.IsShieldActive())
                {
                    //Debug.Log("Shield is active on " + gameObject.name);
                    // if shield is active, do not apply collision damage
                    return;
                }
            }

            Vector3 carPosition = new Vector3(0f,0f,0f);

            // draw a ray cast from the car origin to the ion beam origin to check line of sight
            // if this object is a player use car controller to get car origin
            if (gameObject.CompareTag("Player"))
            {
                carPosition = scr_CarController.getCarOrigin();
            }
            else 
            {
                carPosition = scr_CarAISimple.getCarOrigin();
            }

            Vector3 directionToIonBeamOrigin = scr_ItemIonBeam.transform.position - carPosition;

            // get distance to car
            float distanceToCar = directionToIonBeamOrigin.magnitude;

            Ray rayFromCar = new Ray(scr_ItemIonBeam.transform.position, directionToIonBeamOrigin.normalized);

            if (Physics.Raycast(rayFromCar, out RaycastHit hitInfo, directionToIonBeamOrigin.magnitude))
            {
                // if we hit terrain obstacles or outofbounds, return
                if (hitInfo.collider.gameObject.CompareTag("Obstacle") || hitInfo.collider.gameObject.CompareTag("Terrain")
                    || hitInfo.collider.gameObject.CompareTag("OutOfBounds") || hitInfo.collider.gameObject.CompareTag("Road"))
                {
                    // draw red debug ray
                    Debug.DrawRay(rayFromCar.origin, rayFromCar.direction * hitInfo.distance, Color.red, 0.2f);

                    //Debug.Log("Ion Beam line of sight blocked on " + gameObject.name + " by " + hitInfo.collider.gameObject.name);

                    return;
                }
                
            }

            Debug.DrawRay(rayFromCar.origin, rayFromCar.direction * hitInfo.distance, Color.green, 0.2f);

            // debug log flamethrower hit
            // Debug.Log("Laser hit detected on " + gameObject.name);

            // get damage amount from Ion Beam script
            int ionBeamDamage = scr_ItemIonBeam.GetIonBeamDPS();

            // calculate damage per fixed update frame
            ionBeamDamage = Mathf.RoundToInt(ionBeamDamage * Time.fixedDeltaTime);

            // subtract rocket damage from car health
            internalCarHealth -= Mathf.RoundToInt(ionBeamDamage);

            // show us how much damage we are taking from ion beam
            //Debug.Log("Car " + gameObject.name + " taking " + ionBeamDamage + " damage from Ion Beam.");

            // calculateCollisionDamage = false;

            // if health is below 1, play car destruction particles
            if (internalCarHealth < 1)
            {
                Scr_ParticleHandler.PlayCarDestructionParticles();
            }

        }


        // if Orbital Ray damage toggle is true, apply damage over time
        if (inOrbitalRay)
        {
            // check if Ion Beam script is assigned
            if (scr_ItemOrbitalRayFire == null)
            {
                //Debug.Log("Orbital Ray Fire script is null on " + gameObject.name);
                return;
            }

            // check if shield is active on car
            // reference it from item handler script
            if (scr_itemHandler != null)
            {
                if (scr_itemHandler.IsShieldActive())
                {
                    //Debug.Log("Shield is active on " + gameObject.name);
                    // if shield is active, do not apply collision damage
                    return;
                }
            }

            Vector3 carPosition = new Vector3(0f, 0f, 0f);

            // draw a ray cast from the car origin to the ion beam origin to check line of sight
            // if this object is a player use car controller to get car origin
            if (gameObject.CompareTag("Player"))
            {
                carPosition = scr_CarController.getCarOrigin();
            }
            else
            {
                carPosition = scr_CarAISimple.getCarOrigin();
            }

            Vector3 directionToOrbitalRayOrigin = scr_ItemOrbitalRayFire.transform.position - carPosition;

            // get distance to car
            float distanceToCar = directionToOrbitalRayOrigin.magnitude;

            Ray rayFromCar = new Ray(scr_ItemOrbitalRayFire.transform.position, directionToOrbitalRayOrigin.normalized);

            if (Physics.Raycast(rayFromCar, out RaycastHit hitInfo, directionToOrbitalRayOrigin.magnitude))
            {
                // if we hit terrain obstacles or outofbounds, return
                if (hitInfo.collider.gameObject.CompareTag("Obstacle") || hitInfo.collider.gameObject.CompareTag("Terrain")
                    || hitInfo.collider.gameObject.CompareTag("OutOfBounds") || hitInfo.collider.gameObject.CompareTag("Road"))
                {
                    // draw red debug ray
                    Debug.DrawRay(rayFromCar.origin, rayFromCar.direction * hitInfo.distance, Color.red, 0.2f);

                    //Debug.Log("Orbital Ray line of sight blocked on " + gameObject.name + " by " + hitInfo.collider.gameObject.name);

                    return;
                }

            }

            Debug.DrawRay(rayFromCar.origin, rayFromCar.direction * hitInfo.distance, Color.green, 0.2f);

            // debug log flamethrower hit
            // Debug.Log("Laser hit detected on " + gameObject.name);

            // get damage amount from Orbital Ray script
            int orbitalRayDamage = scr_ItemOrbitalRayFire.GetOrbitalRayDPS();

            // calculate damage per fixed update frame
            orbitalRayDamage = Mathf.RoundToInt(orbitalRayDamage * Time.fixedDeltaTime);

            // subtract rocket damage from car health
            internalCarHealth -= Mathf.RoundToInt(orbitalRayDamage);

            // show us how much damage we are taking from Orbital Ray
            //Debug.Log("Car " + gameObject.name + " taking " + orbitalRayDamage + " damage from Orbital Ray.");

            // calculateCollisionDamage = false;

            // if health is below 1, play car destruction particles
            if (internalCarHealth < 1)
            {
                Scr_ParticleHandler.PlayCarDestructionParticles();
            }

        }
    }

    // get current health
    public int GetCurrentHealth()
    {
        return internalCarHealth;
    }

    public int GetMaxHealth()
    {
        return carHealth;
    }

    // set current health
    public void SetCurrentHealth(int healthAmount)
    {
        internalCarHealth = healthAmount;
    }

    // return if we are on fire
    public bool IsInFlameArea()
    {
        return inFlameArea;
    }

    // return if we are in ion beam
    public bool IsInIonBeam()
    {
        return inIonBeam;
    }

    // return if we are in orbital Ray
    public bool IsInOrbitalRay()
    {
        return inOrbitalRay;
    }

    // check if we collided with other object
    private void OnCollisionEnter(Collision collision)
    {
        
        // if we are not calculating collision damage, return
        if (!calculateCollisionDamage)
        {
            return;
        }

        // check if shield is active on car
        // reference it from item handler script
        if (scr_itemHandler != null)
        {
            if (scr_itemHandler.IsShieldActive())
            {
                // if shield is active, do not apply collision damage
                return;
            }
        }

        // check if we collided with an obstacle
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Props")
                || collision.gameObject.CompareTag("AI") || collision.gameObject.CompareTag("Player")
                    || collision.gameObject.CompareTag("Cop"))
        {
            // calculate amount of health to lose based off collision impact force
            float impactForce = collision.relativeVelocity.magnitude;

            // if impact force is lower than a threshold, set it to zero
            if (impactForce < 10f) // adjust threshold as needed
            {
                impactForce = 0f;
            }

            int healthLoss = Mathf.RoundToInt(impactForce); // adjust multiplier as needed
            internalCarHealth -= healthLoss;

            // clamp health to minimum of 0
            // internalCarHealth = Mathf.Max(internalCarHealth, 0);

            // calculateCollisionDamage = false;

            // if health is below 1, play car destruction particles
            if (internalCarHealth < 1)
            {
                Scr_ParticleHandler.PlayCarDestructionParticles();
            }

            // debug
            // what did we collide with and how much damage did it do?
            // Debug.Log("Detected collision with: " + collision.gameObject.name + " on object: " + gameObject.name + " dealing " + healthLoss + " worth of damage.");


        }

        // check if we collided with a rocket
        if (collision.gameObject.CompareTag("Rocket"))
        {
            // get rocket script component from rocket object if it exists
            if (collision.gameObject.TryGetComponent<Scr_Item_Rocket>(out scr_ItemRocket))
            {
                // debug log rocket hit
                // Debug.Log("Rocket hit detected on " + gameObject.name);

                // get damage amount from rocket script
                int rocketDamage = scr_ItemRocket.GetRocketDamageAmount();

                // subtract rocket damage from car health
                internalCarHealth -= Mathf.RoundToInt(rocketDamage);

                // calculateCollisionDamage = false;

                // if health is below 1, play car destruction particles
                if (internalCarHealth < 1)
                {
                    Scr_ParticleHandler.PlayCarDestructionParticles();
                }

                // debug
                // what did we collide with and how much damage did it do?
                // Debug.Log("Detected collision with: " + collision.gameObject.name + " on object: " + gameObject.name + " dealing " + rocketDamage + " worth of damage.");

            }
            else
            {
                // if we cannot get rocket script, return
                return;
            }

        }

        // check if we collided with a missile
        if (collision.gameObject.CompareTag("Missile"))
        {
            // get missile script component from missile object if it exists
            if (collision.gameObject.TryGetComponent<Scr_Item_Missile>(out scr_ItemMissile))
            {
                // debug log missile hit
                // Debug.Log("Missile hit detected on " + gameObject.name);

                // get damage amount from missile script
                int missileDamage = scr_ItemMissile.GetMissileDamageAmount();

                // subtract rocket damage from car health
                internalCarHealth -= Mathf.RoundToInt(missileDamage);

                // calculateCollisionDamage = false;

                // if health is below 1, play car destruction particles
                if (internalCarHealth < 1)
                {
                    Scr_ParticleHandler.PlayCarDestructionParticles();
                }

                // debug
                // what did we collide with and how much damage did it do?
                // Debug.Log("Detected collision with: " + collision.gameObject.name + " on object: " + gameObject.name + " dealing " + missileDamage + " worth of damage.");

            }
            else
            {
                // if we cannot get rocket script, return
                return;
            }

        }

        // check if we collided with a laser
        if (collision.gameObject.CompareTag("Laser"))
        {
            // get laser script component from missile object if it exists
            if (collision.gameObject.TryGetComponent<Scr_Item_Pierce_Laser>(out scr_PierceLaser))
            {
                // debug log laser hit
                // Debug.Log("Laser hit detected on " + gameObject.name);

                // get damage amount from laser script
                int laserDamage = scr_PierceLaser.GetLaserDamageAmount();

                // subtract rocket damage from car health
                internalCarHealth -= Mathf.RoundToInt(laserDamage);

                // calculateCollisionDamage = false;

                // if health is below 1, play car destruction particles
                if (internalCarHealth < 1)
                {
                    Scr_ParticleHandler.PlayCarDestructionParticles();
                }

                // debug
                // what did we collide with and how much damage did it do?
                // Debug.Log("Detected collision with: " + collision.gameObject.name + " on object: " + gameObject.name + " dealing " + laserDamage + " worth of damage.");

            }
            else
            {
                // if we cannot get rocket script, return
                return;
            }

        }

        // check if we collided with a rocket
        if (collision.gameObject.CompareTag("Ghost Ball"))
        {
            // get rocket script component from rocket object if it exists
            if (collision.gameObject.TryGetComponent<Scr_Item_Ghost_Ball>(out scr_ItemGhost))
            {
                // debug log rocket hit
                // Debug.Log("Rocket hit detected on " + gameObject.name);

                // get damage amount from rocket script
                int ghostDamage = scr_ItemGhost.GetGhostDamageAmount();

                // subtract rocket damage from car health
                internalCarHealth -= Mathf.RoundToInt(ghostDamage);

                // calculateCollisionDamage = false;

                // if health is below 1, play car destruction particles
                if (internalCarHealth < 1)
                {
                    Scr_ParticleHandler.PlayCarDestructionParticles();
                }

                // debug
                // what did we collide with and how much damage did it do?
                // Debug.Log("Detected collision with: " + collision.gameObject.name + " on object: " + gameObject.name + " dealing " + rocketDamage + " worth of damage.");

            }
            else
            {
                // if we cannot get rocket script, return
                return;
            }

        }


    }

    // this if for trigger volumes
    // ex. for damage for flamethrower
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Flamethrower"))
        {
            // get flamethrower script component from 
            if (collision.gameObject.TryGetComponent<Scr_Item_Flamethrower>(out scr_Flamethrower))
            {
                // get componenet
                scr_Flamethrower = collision.gameObject.GetComponent<Scr_Item_Flamethrower>();

            }

            // toggle flag that we are in flame area
            inFlameArea = true;
        }
        if (collision.gameObject.CompareTag("Ion Beam"))
        {
            // get flamethrower script component from 
            if (collision.gameObject.TryGetComponent<Scr_Item_Ion_Beam>(out scr_ItemIonBeam))
            {
                // get componenet
                scr_ItemIonBeam = collision.gameObject.GetComponent<Scr_Item_Ion_Beam>();

            }

            // toggle flag that we are in flame area
            inIonBeam = true;
        }
        if (collision.gameObject.CompareTag("Orbital Ray"))
        {
            // get flamethrower script component from 
            if (collision.gameObject.TryGetComponent<Scr_OrbitalRayFire>(out scr_ItemOrbitalRayFire))
            {
                // get componenet
                scr_ItemOrbitalRayFire = collision.gameObject.GetComponent<Scr_OrbitalRayFire>();

            }

            // toggle flag that we are in flame area
            inOrbitalRay = true;
        }

    }

    private void OnTriggerExit(Collider collision)
    {
        // toggle flag that we are in flame area
        if (collision.gameObject.CompareTag("Flamethrower")) 
        { 
            inFlameArea = false;
        }
        if (collision.gameObject.CompareTag("Ion Beam"))
        {
            inIonBeam = false;
        }
        if (collision.gameObject.CompareTag("Orbital Ray"))
        {
            inOrbitalRay = false;
        }
    }

    // while we are in trigger volume
    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Flamethrower"))
        {
            // get flamethrower script component from 
            if (collision.gameObject.TryGetComponent<Scr_Item_Flamethrower>(out scr_Flamethrower))
            {
                // get componenet
                scr_Flamethrower = collision.gameObject.GetComponent<Scr_Item_Flamethrower>();
            }
            // toggle flag that we are in flame area
            inFlameArea = true;
        }
        if (collision.gameObject.CompareTag("Ion Beam"))
        {
            // get flamethrower script component from 
            if (collision.gameObject.TryGetComponent<Scr_Item_Ion_Beam>(out scr_ItemIonBeam))
            {
                // get componenet
                scr_ItemIonBeam = collision.gameObject.GetComponent<Scr_Item_Ion_Beam>();

            }

            // toggle flag that we are in flame area
            inIonBeam = true;
        }
        if (collision.gameObject.CompareTag("Orbital Ray"))
        {
            // get flamethrower script component from 
            if (collision.gameObject.TryGetComponent<Scr_OrbitalRayFire>(out scr_ItemOrbitalRayFire))
            {
                // get componenet
                scr_ItemOrbitalRayFire = collision.gameObject.GetComponent<Scr_OrbitalRayFire>();

            }

            // toggle flag that we are in flame area
            inOrbitalRay = true;
        }

    }

    // when we exit collision, re-enable collision damage calculation
    private void OnCollisionExit(Collision collision)
    {
        /*
        // check if we collided with an obstacle
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Props")
                || collision.gameObject.CompareTag("AI") || collision.gameObject.CompareTag("Player")
                    || collision.gameObject.CompareTag("Cop") || collision.gameObject.CompareTag("Rocket")
                        || collision.gameObject.CompareTag("Missile"))
        {
            calculateCollisionDamage = true;


        }
        */

        // calculateCollisionDamage = true;

    }

    private void ResetToLastCheckpoint()
    {
        // get last checkpoint passed from i am stuck script
        Transform lastCheckpoint = scr_IAmStuck.GetLastCheckpointPassed();
        if (lastCheckpoint != null)
        {
            carDeathTimer -= Time.deltaTime;

            if (carDeathTimer <= 0f)
            {
                // set rigidbody velocity to zero before teleporting to avoid sliding into walls after reset
                carRigidbody.linearVelocity = Vector3.zero;

                // stop death particles
                Scr_ParticleHandler.StopCarDestructionParticles();

                // if we are finished the race, set car position to the first checkpoint instead of last checkpoint we passed
                if (scr_MyRaceProgress.completedRace)
                {
                    // set box cast to where we will attempt to place the car back, if there is something there, generate a new random position
                    // box cast will be the dimensions of the car collider. We will keep generating a new random position until we find one that is not colliding with anything.
                    // This is to prevent the car from being placed inside another and start an extreme physics interaction

                    // generate a random position around top of the checkpoint to spawn the car back on
                    Vector3 RespawnPosition = scr_MyRaceProgress.RaceCheckpointTransforms[0].position + new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 4f), Random.Range(-8f, 8f)) + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision

                    int numberOfAttempts = 10; // counter to track number of attempts to find a non-colliding position

                    // check if that respawn position is colliding with anything using a box cast with the dimensions of the car collider, if it is, generate a new random position until we find one that is not colliding with anything.
                    // This is to prevent the car from being placed inside another and start an extreme physics interaction
                    while (Physics.CheckBox(RespawnPosition, spawnBlockingBoxCastDimensions * 1.5f, Quaternion.identity, spawnBlockingLayers))
                    {
                        // if we are colliding with something, generate a new random position and check again
                        RespawnPosition = scr_MyRaceProgress.RaceCheckpointTransforms[0].position + new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 4f), Random.Range(-8f, 8f)) + Vector3.up * 2f;

                        // debug, draw the box cast in the scene view to see where we are checking for collisions when respawning after death
                        Debug.DrawLine(RespawnPosition - spawnBlockingBoxCastDimensions, RespawnPosition + spawnBlockingBoxCastDimensions, Color.red, 1f);
                        Debug.Log(transform.name + " Respawn position " + RespawnPosition + " is colliding with something, generating new position.");

                        // if not colliding, break out of the loop and use that position for respawn
                        if (!Physics.CheckBox(RespawnPosition, spawnBlockingBoxCastDimensions * 1.5f, Quaternion.identity, spawnBlockingLayers))
                        {
                            Debug.DrawLine(RespawnPosition - spawnBlockingBoxCastDimensions, RespawnPosition + spawnBlockingBoxCastDimensions, Color.green, 1f);
                            Debug.Log(transform.name + " Respawn position " + RespawnPosition + " is not colliding with anything, placed.");

                            break;
                        }

                        // after x amount of attempts to find a non-colliding position, just place the car at the checkpoint position even if it is colliding with something to avoid infinite loop, this is a rare edge case and we want to make sure the car respawns even if we cannot find a perfect position for it
                        numberOfAttempts--;

                        if (numberOfAttempts <= 0)
                        {
                            Debug.LogWarning(transform.name + " Could not find a non-colliding respawn position after 10 attempts, placing at checkpoint position anyway.");
                            break;
                        }
                    }

                    transform.position = RespawnPosition;
                    transform.rotation = scr_MyRaceProgress.RaceCheckpointTransforms[0].rotation; // align car rotation with checkpoint rotation


                    // freeze rotation 
                    // transform.rotation = Quaternion.Euler(scr_MyRaceProgress.RaceCheckpointTransforms[0].rotation.x, scr_MyRaceProgress.RaceCheckpointTransforms[0].rotation.y, scr_MyRaceProgress.RaceCheckpointTransforms[0].rotation.z);
                }
                else 
                {
                    // generate a random position around top of the checkpoint to spawn the car back on
                    Vector3 RespawnPosition = scr_IAmStuck.GetLastCheckpointPassed().position + new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 4f), Random.Range(-8f, 8f)) + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision

                    int numberOfAttempts = 20; // counter to track number of attempts to find a non-colliding position


                    // check if that respawn position is colliding with anything using a box cast with the dimensions of the car collider, if it is, generate a new random position until we find one that is not colliding with anything.
                    // This is to prevent the car from being placed inside another and start an extreme physics interaction
                    while (Physics.CheckBox(RespawnPosition, spawnBlockingBoxCastDimensions * 1.5f, Quaternion.identity, spawnBlockingLayers))
                    {
                        // if we are colliding with something, generate a new random position and check again
                        RespawnPosition = scr_MyRaceProgress.RaceCheckpointTransforms[0].position + new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 4f), Random.Range(-8f, 8f)) + Vector3.up * 2f;

                        // debug, draw the box cast in the scene view to see where we are checking for collisions when respawning after death
                        Debug.DrawLine(RespawnPosition - spawnBlockingBoxCastDimensions, RespawnPosition + spawnBlockingBoxCastDimensions, Color.red, 1f);
                        Debug.Log(transform.name + " Respawn position " + RespawnPosition + " is colliding with something, generating new position.");

                        // if not colliding, break out of the loop and use that position for respawn
                        if (!Physics.CheckBox(RespawnPosition, spawnBlockingBoxCastDimensions * 1.5f, Quaternion.identity, spawnBlockingLayers))
                        {
                            Debug.DrawLine(RespawnPosition - spawnBlockingBoxCastDimensions, RespawnPosition + spawnBlockingBoxCastDimensions, Color.green, 1f);
                            Debug.Log(transform.name + " Respawn position " + RespawnPosition + " is not colliding with anything, placed.");

                            break;
                        }

                        // after x amount of attempts to find a non-colliding position, just place the car at the checkpoint position even if it is colliding with something to avoid infinite loop, this is a rare edge case and we want to make sure the car respawns even if we cannot find a perfect position for it
                        numberOfAttempts--;

                        if (numberOfAttempts <= 0)
                        {
                            Debug.LogWarning(transform.name + " Could not find a non-colliding respawn position after 20 attempts, placing at checkpoint position anyway.");
                            break;
                        }

                    }

                    transform.position = RespawnPosition;
                    transform.rotation = scr_IAmStuck.GetLastCheckpointPassed().rotation; // align car rotation with checkpoint rotation
                    
                    // transform.rotation = Quaternion.Euler(scr_IAmStuck.GetLastCheckpointPassed().rotation.x, scr_IAmStuck.GetLastCheckpointPassed().rotation.y, scr_IAmStuck.GetLastCheckpointPassed().rotation.z);

                }

                // set car rigidbody velocity to zero to avoid car sliding after reset
                carRigidbody.linearVelocity = Vector3.zero;
                carRigidbody.angularVelocity = Vector3.zero;

                // reset health back to max
                internalCarHealth = carHealth;

                // set car max speed and acceleration back to original values after reset
                if (gameObject.CompareTag("AI"))
                {
                    scr_CarControllerAI.setAcceleration(originalAcceleration);
                    scr_CarControllerAI.setMaxSpeed(originalMaxSpeed);

                    // reset wheels - AI
                    scr_CarControllerAI.resetWheelsToDefaultPosition();

                }
                if (gameObject.CompareTag("Player"))
                {
                    scr_CarController.setAcceleration(originalAcceleration);
                    scr_CarController.setMaxSpeed(originalMaxSpeed);

                    // reset wheels - Player
                    scr_CarController.resetWheelsToDefaultPosition();
                }

                // reset car mass back to original value
                carRigidbody.mass = originalCarMass;

                // reset the stuck timer
                carDeathTimer = 5f;

                // set velocity to zero again after reset just to be safe
                carRigidbody.linearVelocity = Vector3.zero;

                // set angular velocity to zero again after reset just to be safe
                carRigidbody.angularVelocity = Vector3.zero;
            }
            else 
            {
                
                // make car a lot lighter
                carRigidbody.mass = originalCarMass * 0.5f;

                // Set car max speed and acceleration to zero
                if (gameObject.CompareTag("AI"))
                {

                    scr_CarControllerAI.setAcceleration(0f);
                    //scr_CarControllerAI.setMaxSpeed(0f);
                }
                if (gameObject.CompareTag("Player"))
                {

                    scr_CarController.setAcceleration(0f);
                    //scr_CarController.setMaxSpeed(0f);
                }

            }
            
        }
    }


}
