using UnityEngine;

public class Scr_Item_Pierce_Laser : MonoBehaviour
{
    // components
    // rigidbody
    private Rigidbody rb;

    // speed of the laser
    // does not accelerate, just constant speed
    [SerializeField] private float speed;

    // self destruction timer
    [SerializeField] private float destructionTimer = 5f; // in seconds

    // damage amount of the laser
    // this is per projectile
    [SerializeField] private int damageAmount;

    // destruction particle effect prefab
    [SerializeField] private GameObject laserDestructionEffectPrefab;

    [SerializeField] private float groundRayDistance = 5f;
    [SerializeField] private LayerMask groundLayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get rocket rigidbody and set initial velocity
        rb = GetComponent<Rigidbody>();

        // set initial forward velocity
        //rb.linearVelocity = transform.forward * speed;

        // Raycast straight down
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundRayDistance, groundLayer))
        {
            Vector3 groundNormal = hit.normal;

            // Project forward direction onto ground plane
            Vector3 groundParallelDirection = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            // Apply new rotation
            transform.rotation = Quaternion.LookRotation(groundParallelDirection, groundNormal);
        }

        // Set velocity ONCE after rotation adjustment
        rb.linearVelocity = transform.forward * speed;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // update rocket position based on its velocity
        //transform.position += rb.linearVelocity * Time.fixedDeltaTime;

        // count down destruction timer
        destructionTimer -= Time.fixedDeltaTime;

        // destroy rocket when timer hits zero
        if (destructionTimer <= 0f)
        {
            LaserDestructionEffect();
        }
        
    }

    // return the damage amount the rocket does on impact
    public int GetLaserDamageAmount()
    {
        return damageAmount;
    }

    // destroy this rocket object when impacting with another object
    private void OnCollisionEnter(Collision collision)
    {
        LaserDestructionEffect();
    }

    // set laser speed
    public void SetLaserSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // rocket explosion effect function
    private void LaserDestructionEffect()
    {
        // add an offset to the explosion to be a bit in front of the rocket
        Transform LaserDestructionSpawn = transform;

        LaserDestructionSpawn.position = transform.position + (transform.forward * 0.25f);

        // set the rotation of the explosion to match the laser's rotation
        Quaternion LaserDestructionSpawnRotation = transform.rotation;

        // instantiate explosion effect at laser position
        Instantiate(laserDestructionEffectPrefab, LaserDestructionSpawn.position, LaserDestructionSpawnRotation);

        // destroy our laser object
        Destroy(transform.root.gameObject);
    }
}
