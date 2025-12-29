using UnityEngine;

public class Scr_Item_Rocket : MonoBehaviour
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

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get rocket rigidbody and set initial velocity
        rb = GetComponent<Rigidbody>();

        // set initial forward velocity
        rb.linearVelocity = transform.forward * initialSpeed;

        
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


}
