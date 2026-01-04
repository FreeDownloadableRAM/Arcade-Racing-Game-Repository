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

    [SerializeField] private float speed = 30f;

    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private float heightAdjustSpeed = 10f;

    [SerializeField] private float downhillPullSpeed = 12f;

    [SerializeField] private float forwardRayDistance = 2.5f;
    [SerializeField] private float anticipationStrength = 1.5f;
    [SerializeField] private float normalBlend = 0.6f;


    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float rotationSpeed = 8f;


    private Vector3 lastGroundNormal = Vector3.up;
    private float lastGroundHeight;


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
        /*
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
        */

        
        HandleMovement();
        HandleTerrainHugging();
        HandleLifetime();
        

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

    // movement handle function
    void HandleMovement()
    {
        speed = Mathf.Min(speed + acceleration * Time.deltaTime, maxSpeed);
        transform.position += transform.forward * speed * Time.deltaTime;
    }


    // make sure we hug the ground
    void HandleTerrainHugging()
    {
        RaycastHit downHit;
        RaycastHit forwardHit;

        Vector3 pos = transform.position;

        bool hasDownHit = Physics.Raycast(
            pos,
            Vector3.down,
            out downHit,
            groundCheckDistance,
            groundLayer
        );

        bool hasForwardHit = Physics.Raycast(
            pos + transform.forward * forwardRayDistance,
            Vector3.down,
            out forwardHit,
            groundCheckDistance,
            groundLayer
        );

        // Store last known ground info
        if (hasDownHit)
        {
            lastGroundNormal = downHit.normal;
            lastGroundHeight = downHit.point.y;
        }
        else if (hasForwardHit)
        {
            lastGroundNormal = forwardHit.normal;
            lastGroundHeight = forwardHit.point.y;
        }

        // ---------- HEIGHT CONTROL ----------

        float desiredHeight = hoverHeight;

        // Anticipate upcoming terrain
        if (hasForwardHit)
        {
            float delta = forwardHit.point.y - lastGroundHeight;
            desiredHeight += delta * anticipationStrength;
        }

        float targetY = lastGroundHeight + desiredHeight;

        // Pull downward faster when descending
        float lerpSpeed = heightAdjustSpeed;
        if (!hasDownHit && pos.y > targetY)
            lerpSpeed = downhillPullSpeed;

        float newY = Mathf.MoveTowards(
            pos.y,
            targetY,
            lerpSpeed * Time.deltaTime
        );

        transform.position = new Vector3(pos.x, newY, pos.z);

        // ---------- ROTATION CONTROL ----------

        Vector3 baseNormal = lastGroundNormal;

        if (hasDownHit && hasForwardHit)
        {
            baseNormal = Vector3.Slerp(
                downHit.normal,
                forwardHit.normal,
                normalBlend
            );
        }

        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, baseNormal).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(projectedForward, baseNormal);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }


    // handle lifetime, delete rocket when timer hits zero
    void HandleLifetime()
    {
        destructionTimer -= Time.fixedDeltaTime;
        if (destructionTimer <= 0f)
        {
            RocketExplosionEffect();
        }
    }

}
