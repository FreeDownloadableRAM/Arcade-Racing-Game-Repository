using UnityEngine;

public class Scr_I_am_stuck_cop : MonoBehaviour
{
    
    // define timer to determine if we are stuck
    [SerializeField] float stuckTimer;
    private float stuckTimerCounter;

    // track car speed
    private float carSpeed;
    [SerializeField] private Rigidbody rigidBody;

    // initial position of the car
    private Vector3 initialPosition;

    // intial rotation of the car
    private Quaternion initialRotation;

    // reference the car target handler script to get the AI state
    private scr_Car_Cop_Target_Handler copTargetHandler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set stuck timer to 3 seconds if it is set to 0 in the inspector
        if (stuckTimer == 0f)
        {
            stuckTimer = 3f;
        }

        initialPosition = transform.position;

        initialRotation = transform.rotation;

        // Get the cop target handler component
        copTargetHandler = GetComponent<scr_Car_Cop_Target_Handler>();

    }

    // Update is called once per frame
    void Update()
    {
        // check if we are in chase mode, if so, we dont have to worry about being stuck
        if (copTargetHandler.AIState == "Chase")
        {
            return;
        }

        // first check if we are close to initial start position, if so, do nothing
        if (Vector3.Distance(transform.position, initialPosition) < 7.5f)
        {

            return;
        }

        // get car speed from Rigidbody component
        carSpeed = Vector3.Magnitude(rigidBody.linearVelocity);

        // if our car speed is lower than 2 (very slow) for the duration of the stuck timer, we are stuck
        if (carSpeed < 5f)
        {
            stuckTimerCounter -= Time.deltaTime;
            if (stuckTimerCounter <= 0f)
            {
                
                
                transform.position = initialPosition + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision
                transform.rotation = initialRotation; // align car rotation with initial spawn rotation
                Debug.Log("Car was stuck! Resetting to last checkpoint.");
                
               
                // reset the stuck timer
                stuckTimerCounter = stuckTimer;
            }
        }
        else
        {
            // we are not stuck, so reset the stuck timer
            stuckTimerCounter = stuckTimer;
        }

    }

    // handle checkpoint collision
    private void OnTriggerEnter(Collider other)
    {
        
        // check if object is out of bounds geometry
        if (other.CompareTag("OutOfBounds"))
        {
            
            transform.position = initialPosition + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision
            transform.rotation = initialRotation; // align car rotation with initial spawn rotation
            Debug.Log("Car was out of bounds! Resetting to last checkpoint.");
            

            // reset the stuck timer
            stuckTimerCounter = stuckTimer;
        }
    }

    
}