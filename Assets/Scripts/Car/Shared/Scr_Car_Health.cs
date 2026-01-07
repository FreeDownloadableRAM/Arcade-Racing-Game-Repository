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

    // car death timer
    private float carDeathTimer = 5f;

    // get rocket script component
    private Scr_Item_Rocket scr_ItemRocket;

    // get missile script component
    private Scr_Item_Missile scr_ItemMissile;

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

        // reference particle handler
        Scr_ParticleHandler = GetComponent<Scr_Particle_Handler>();

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

        

    }

    void Update()
    {
        // if health is 0, place car back to last checkpoint passed
        if (internalCarHealth <= 0)
        {
            
            ResetToLastCheckpoint();

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


    // check if we collided with other object
    private void OnCollisionEnter(Collision collision)
    {
        
        // if we are not calculating collision damage, return
        if (!calculateCollisionDamage)
        {
            return;
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

            calculateCollisionDamage = false;

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

                calculateCollisionDamage = false;

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
            // get rocket script component from missile object if it exists
            if (collision.gameObject.TryGetComponent<Scr_Item_Missile>(out scr_ItemMissile))
            {
                // debug log rocket hit
                // Debug.Log("Missile hit detected on " + gameObject.name);

                // get damage amount from missile script
                int missileDamage = scr_ItemMissile.GetMissileDamageAmount();

                // subtract rocket damage from car health
                internalCarHealth -= Mathf.RoundToInt(missileDamage);

                calculateCollisionDamage = false;

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

        calculateCollisionDamage = true;

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
                // stop death particles
                Scr_ParticleHandler.StopCarDestructionParticles();

                transform.position = scr_IAmStuck.GetLastCheckpointPassed().position + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f)) + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision
                transform.rotation = scr_IAmStuck.GetLastCheckpointPassed().rotation; // align car rotation with checkpoint rotation

                
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
