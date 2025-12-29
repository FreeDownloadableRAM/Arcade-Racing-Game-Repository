using UnityEngine;

public class scr_Checkpoint : MonoBehaviour
{
    private scr_RaceCheckpoints raceCheckpoints;

    // trigger on collision
    private void OnTriggerEnter(Collider other)
    {
        // check if the object is a player
        if (other.CompareTag("PlayerCar"))
        {
            //Debug.Log("Checkpoint reached by Player!");
            raceCheckpoints.ObjectThroughCheckpoint(this);
        }

        // check if the object is a Ai Car
        if (other.CompareTag("Cars"))
        {
            //Debug.Log("Checkpoint reached by AI Car!");
            raceCheckpoints.ObjectThroughCheckpoint(this);
        }
    }

    public void SetRaceCheckpoints(scr_RaceCheckpoints raceCheckpoints)
    {
        this.raceCheckpoints = raceCheckpoints;
    }
}
