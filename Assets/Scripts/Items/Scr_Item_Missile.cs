using UnityEngine;

public class Scr_Item_Missile : MonoBehaviour
{
    // initial speed of the rocket
    [SerializeField] private float initialSpeed;

    // max speed of the rocket
    [SerializeField] private float maxSpeed;

    // acceleration of the rocket
    [SerializeField] private float acceleration;

    // amount of damage the rocket does on impact
    [SerializeField] private int damageAmount;

    // get rigidbody component
    private Rigidbody rb;

    // destruction timer
    [SerializeField] private float destructionTimer; // in seconds

    // destruction particle effect prefab
    [SerializeField] private GameObject explosionRocketEffectPrefab;

    // homing target
    private Transform homingTarget;

    // get race manager script from race manager game object
    // race track object
    private GameObject RaceTrackObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get rocket rigidbody and set initial velocity
        rb = GetComponent<Rigidbody>();

        // set initial forward velocity
        rb.linearVelocity = transform.forward * initialSpeed;

        // find the race track object in the scene
        // this will have the racers placement data that we need to home in on the correct target
        RaceTrackObject = GameObject.FindWithTag("Race");

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // check the rocket's current speed and apply acceleration if below max speed
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.linearVelocity += transform.forward * acceleration * Time.fixedDeltaTime;
        }

        // update rocket position based on its velocity
        //transform.position += rb.linearVelocity * Time.fixedDeltaTime;

        // count down destruction timer
        destructionTimer -= Time.fixedDeltaTime;

        // destroy rocket when timer hits zero
        if (destructionTimer <= 0f)
        {
            RocketExplosionEffect();
        }

        // we want to adjust direction to point towards the target if we have one
        // get target
    }

    // return the damage amount the rocket does on impact
    public int GetRocketDamageAmount()
    {
        return damageAmount;
    }

    // destroy this rocket object when impacting with another object
    private void OnCollisionEnter(Collision collision)
    {
        RocketExplosionEffect();
    }

    // rocket explosion effect function
    private void RocketExplosionEffect()
    {
        // add an offset to the explosion to be a bit in front of the rocket
        Transform RocketExplosionSpawn = transform;

        RocketExplosionSpawn.position = transform.position + (transform.forward * 0.25f);

        // set the rotation of the explosion to match the rocket's rotation
        Quaternion RocketExplosionSpawnRotation = transform.rotation;

        // instantiate explosion effect at rocket position
        Instantiate(explosionRocketEffectPrefab, RocketExplosionSpawn.position, RocketExplosionSpawnRotation);

        // destroy our rocket object
        Destroy(transform.root.gameObject);
    }


    // set initial rocket speed
    public void SetInitialRocketSpeed(float speed)
    {
        initialSpeed = speed;
    }

    // get target to home in on
    private Transform GetHomingTarget()
    {
        // get our racer position (out of 24), get the racer ahead of us, and set their transform as our homing target
        // if we are first place, set homing target to the racer in last place




        // return homing target
        return homingTarget;
    }
}

