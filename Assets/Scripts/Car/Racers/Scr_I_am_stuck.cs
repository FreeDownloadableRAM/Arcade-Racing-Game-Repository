using Unity.VisualScripting;
using UnityEngine;

public class Scr_I_am_stuck : MonoBehaviour
{
    // get the last checkpoint passed
    private Transform lastCheckpointPassed;

    // define timer to determine if we are stuck
    [SerializeField] float stuckTimer;
    private float stuckTimerCounter;

    // track car speed
    private float carSpeed;
    [SerializeField] private Rigidbody rigidBody;

    // race progress script reference
    private scr_My_Race_Progress scrMyRaceProgress;

    // get car health script, do not reset movement if health is zero
    // the health script will handle respawn in that case
    private Scr_Car_Health scrCarHealth;

    // car controller to control wheels
    private CarControllerAI scrCarControllerAI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set stuck timer to 3 seconds if it is set to 0 in the inspector
        if (stuckTimer == 0f)
        {
            stuckTimer = 3f;
        }

        // get component reference of race progress script
        scrMyRaceProgress = GetComponent<scr_My_Race_Progress>();

        // get car health script reference
        scrCarHealth = GetComponent<Scr_Car_Health>();

        // get car controller reference
        scrCarControllerAI = GetComponent<CarControllerAI>();

    }

    // Update is called once per frame
    void Update()
    {
        // first check if we completed the race
        // if we did, we dont need to check if we are stuck anymore
        if (scrMyRaceProgress.completedRace)
        {
            return; // exit the function if we completed
        }

        if ((scrCarHealth.GetCurrentHealth() < 1)) 
        { 
            // reset stuck timer, do not attempt to re-place the car onto last checkpoint while it is dead
            stuckTimerCounter = stuckTimer;
            return;
        }

        // get car speed from Rigidbody component
        carSpeed = Vector3.Magnitude(rigidBody.linearVelocity);

        // if our car speed is lower than 5 (very slow) for the duration of the stuck timer, we are stuck
        if (carSpeed < 5f)
        {
            stuckTimerCounter -= Time.deltaTime;
            if (stuckTimerCounter <= 0f)
            {
                // we are stuck, so reset to last checkpoint passed
                if (lastCheckpointPassed != null && (scrCarHealth.GetCurrentHealth() > 0))
                {
                    transform.position = lastCheckpointPassed.position + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f)) + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision
                    transform.rotation = lastCheckpointPassed.rotation; // align car rotation with checkpoint rotation

                    // reset wheels
                    scrCarControllerAI.resetWheelsToDefaultPosition();

                    Debug.Log("Car was stuck! Resetting to last checkpoint.");

                    
                }
                else
                {
                    //Debug.Log("No checkpoint passed yet, cannot reset position!");
                }
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
        GameObject Checkpoint = other.gameObject;

        // check if the object is a checkpoint
        if (other.CompareTag("Checkpoint"))
        {

            // set the last checkpoint passed to this one
            //lastCheckpointPassed = scrMyRaceProgress.RaceCheckpointTransforms.IndexOf(Checkpoint.transform);

            // if we are at the first checkpoint, we need to handle that case
            if (scrMyRaceProgress.nextCheckpointIndex > 0)
            {
                // get the transform of the last checkpoint passed
                lastCheckpointPassed = scrMyRaceProgress.RaceCheckpointTransforms[scrMyRaceProgress.nextCheckpointIndex - 1]; // last checkpoint passed
            }
            else
            {
                lastCheckpointPassed = scrMyRaceProgress.RaceCheckpointTransforms[0]; // first checkpoint
            }



            //Debug.Log("Last checkpoint passed set to: " + lastCheckpointPassed.name);
        }

        // check if object is out of bounds geometry
        if (other.CompareTag("OutOfBounds"))
        {
            // we are stuck, so reset to last checkpoint passed
            if (lastCheckpointPassed != null)
            {
                transform.position = lastCheckpointPassed.position + Vector3.up * 2f; // move car slightly above the checkpoint to avoid collision
                transform.rotation = lastCheckpointPassed.rotation; // align car rotation with checkpoint rotation

                // reset wheels
                scrCarControllerAI.resetWheelsToDefaultPosition();

                //Debug.Log("Car was out of bounds! Resetting to last checkpoint.");
            }
            else
            {
                //Debug.Log("No checkpoint passed yet, cannot reset position!");
            }

            // reset the stuck timer
            stuckTimerCounter = stuckTimer;
        }
    }

    public Transform GetLastCheckpointPassed()
    {
        return lastCheckpointPassed;
    }

    // get car stuck internal timer value
    public float GetStuckTimerCounter()
    {
        return stuckTimerCounter;
    }

    // get car stuck timer value
    public float GetStuckTimer()
    {
        return stuckTimer;
    }

    // set stuck timer counter
    public void SetStuckTimerCounter(float newTimer)
    {
        stuckTimerCounter = newTimer;
    }
}