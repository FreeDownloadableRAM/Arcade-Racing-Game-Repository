using UnityEngine;

public class Scr_Item_Shield_Dispersion : MonoBehaviour
{

    // self destruction timer in seconds
    [SerializeField] private float selfDestructTimer;

    // components
    private Rigidbody rb;

    // object speed
    private float objectSpeed;

    // on start
    private void Start()
    {
        // get components
        rb = GetComponent<Rigidbody>();

        // set initial forward velocity
        rb.linearVelocity = transform.forward * objectSpeed;

    }


    // Count down timer
    void FixedUpdate()
    {
        
        selfDestructTimer -= Time.fixedDeltaTime;

        // when timer hits zero, destroy this object
        if (selfDestructTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // set dispersion initial speed
    public void SetInitialSpeed(float speedValue)
    {
        // set initial speed of this game object
        objectSpeed = speedValue;
    }
}
