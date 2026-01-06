using UnityEngine;

public class Scr_Item_Missile : MonoBehaviour
{
    // missile properties
    [SerializeField] private float initialSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private int damageAmount;

    [SerializeField] private float destructionTimer;
    [SerializeField] private float destructionStartValue;
    [SerializeField] private GameObject explosionRocketEffectPrefab;

    // lock on properties
    [Header("Homing")]
    [SerializeField] private float turnRate = 5f;
    [SerializeField] private float homingStrength = 1f;

    // collision avoidance properties
    [Header("Collision Avoidance")]
    [SerializeField] private float avoidanceDistance = 6f;
    [SerializeField] private float avoidanceStrength = 2.5f;
    [SerializeField] private float avoidanceRadius = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    // avoid hitting the ground
    [Header("Ground Safety")]
    [SerializeField] private float groundCheckDistance = 4f;
    [SerializeField] private float minGroundClearance = 1.2f;
    [SerializeField] private float groundAvoidStrength = 3f;
    [SerializeField] private LayerMask groundLayer;

    // components
    private Rigidbody rb;

    // target for homing
    private Transform homingTarget;

    // height offset to correct for target's center
    private Vector3 targetHeightOffset;

    void Start()
    {
        // get components
        rb = GetComponent<Rigidbody>();

        // set initial forward velocity
        rb.linearVelocity = transform.forward * initialSpeed;

        // set destruction timer start value
        destructionStartValue = destructionTimer;
    }

    void FixedUpdate()
    {
        // handle missile movement
        // do not run homing logic for the first moments of missile flight to allow for initial stabilization
        if (destructionTimer < (destructionStartValue - 0.25f)) { 
            HandleHoming();

        }
        // handle missile speed
        HandleSpeed();
        
        // destroy object after set time
        HandleLifetime();
    }

    // ------------------ Collision Avoidance ------------------

    // ground safety to avoid crashing into the ground
    private Vector3 ApplyGroundSafety(Vector3 desiredDirection)
    {
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer))
        {
            // Too close to ground → prevent downward steering
            if (hit.distance < minGroundClearance)
            {
                // Remove downward component
                desiredDirection = Vector3.ProjectOnPlane(desiredDirection, hit.normal);

                // Bias upward along the terrain normal
                desiredDirection += hit.normal * groundAvoidStrength;
            }
        }

        return desiredDirection.normalized;
    }


    // avoid collisions with obstacles and terrain
    private Vector3 AvoidCollision()
    {
        Vector3 avoidanceDirection = Vector3.zero;

        // Ray origins slightly offset to cover missile width
        Vector3 origin = transform.position + transform.forward * 0.5f;

        Vector3[] rayDirections =
        {
        transform.forward,
        Quaternion.AngleAxis(20f, transform.up) * transform.forward,
        Quaternion.AngleAxis(-20f, transform.up) * transform.forward,
        Quaternion.AngleAxis(20f, transform.right) * transform.forward,
        Quaternion.AngleAxis(-20f, transform.right) * transform.forward
        };

        foreach (Vector3 dir in rayDirections)
        {
            if (Physics.SphereCast(
                origin,
                avoidanceRadius,
                dir,
                out RaycastHit hit,
                avoidanceDistance,
                obstacleLayer))
            {
                // Steer away from surface normal
                float weight = 1f - (hit.distance / avoidanceDistance);
                avoidanceDirection += hit.normal * weight;
            }
        }

        return avoidanceDirection.normalized;
    }



    // ------------------ HOMING ------------------

    private void HandleHoming()
    {
        Vector3 desiredDirection = transform.forward;

        if (homingTarget != null)
        {
            desiredDirection = ((homingTarget.position + targetHeightOffset) - transform.position).normalized;
        }

        // get distance to target for debugging
        // float distanceToTarget = Vector3.Distance(transform.position, (homingTarget.position + targetHeightOffset));

        // debugging, draw line towards target
        // Debug.DrawLine(transform.position, transform.position + desiredDirection * distanceToTarget, Color.red);

        // Obstacle avoidance
        Vector3 avoidance = AvoidCollision();
        if (avoidance != Vector3.zero)
        {
            desiredDirection = Vector3.Slerp(
                desiredDirection,
                avoidance,
                avoidanceStrength * Time.fixedDeltaTime
            );
        }

        // Ground safety pass 
        desiredDirection = ApplyGroundSafety(desiredDirection);

        Vector3 newForward = Vector3.RotateTowards(
            transform.forward,
            desiredDirection,
            turnRate * Time.fixedDeltaTime,
            0f
        );

        transform.rotation = Quaternion.LookRotation(newForward);

        // Redirect velocity to match new forward direction
        rb.linearVelocity = newForward * rb.linearVelocity.magnitude * homingStrength + rb.linearVelocity * (1f - homingStrength);
    }

    // ------------------ SPEED ------------------

    private void HandleSpeed()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed < maxSpeed)
        {
            rb.linearVelocity += transform.forward * acceleration * Time.fixedDeltaTime;
        }
    }

    // ------------------ LIFETIME ------------------

    private void HandleLifetime()
    {
        destructionTimer -= Time.fixedDeltaTime;

        if (destructionTimer <= 0f)
        {
            RocketExplosionEffect();
        }
    }

    // ------------------ COLLISION ------------------

    private void OnCollisionEnter(Collision collision)
    {
        // check if we collided with a car first
        if ((collision.gameObject.CompareTag("AI") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Cop")))
        {
            RocketExplosionEffect();
        }
        // if we didnt, that means we collided with terrain
        else 
        {
            // do not explode within a moment of object creation, so when we get spawned it, and hit the ground instantly, it wont cause an explosion, and delete this object
            if (destructionTimer < (destructionStartValue - 0.25f))
            {
                RocketExplosionEffect();
            }
            
        }
        
    }

    private void RocketExplosionEffect()
    {
        Vector3 spawnPos = transform.position + transform.forward * 0.25f;
        Instantiate(explosionRocketEffectPrefab, spawnPos, transform.rotation);
        Destroy(transform.root.gameObject);
    }

    // ------------------ PUBLIC API ------------------

    public int GetMissileDamageAmount()
    {
        return damageAmount;
    }

    public float SetInitialMissileSpeed(float speed)
    {
        initialSpeed = speed;
        return initialSpeed;
    }

    public Transform SetHomingTarget(Transform targetTransform)
    {
        homingTarget = targetTransform;
        return homingTarget;
    }

    public Vector3 SetHomingTargetHeightOffset(Vector3 targetCarHeightOffset) 
    {
        targetHeightOffset = targetCarHeightOffset;
        return targetHeightOffset;

    }
}
