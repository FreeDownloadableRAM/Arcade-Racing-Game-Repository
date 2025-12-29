using UnityEngine;

public class Scr_Car_Forces : MonoBehaviour
{
    Rigidbody rb; // Reference to the Rigidbody component

    // bounce force
    [SerializeField] private float bounceForce; // Force applied when bouncing off objects
    [SerializeField] private float countDownToFlip;
    private float countDownToFlipTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        countDownToFlipTimer = countDownToFlip * Time.deltaTime;


    }

    private void FixedUpdate()
    {
        // prevent staying upside down
        if (Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            countDownToFlipTimer = (countDownToFlipTimer - 1);

            if (countDownToFlipTimer < 0) 
            {
                transform.rotation *= Quaternion.Euler(0, 0, 180);
                countDownToFlipTimer = countDownToFlip;

                Debug.Log("Car Flipped");
            }

        }
        else 
        {
            countDownToFlipTimer = countDownToFlip;

        }

        
        //Debug.Log("Car Flip Timer: " + countDownToFlipTimer);

    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the car collides with an object tagged as "Cars"
        if (collision.gameObject.CompareTag("Cars"))
        {
            Debug.Log("Car collided with another car!");

            // Calculate the bounce direction based on the collision normal
            Vector3 bounceDirection = ((transform.position) + (collision.transform.position)).normalized;
            // Apply a force in the opposite direction of the collision normal
            rb.AddForce((bounceDirection) * bounceForce, ForceMode.Acceleration);

        }

    }

    



}
