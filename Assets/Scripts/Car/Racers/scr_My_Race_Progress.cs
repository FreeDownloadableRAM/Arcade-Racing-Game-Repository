using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class scr_My_Race_Progress : MonoBehaviour
{

    // race track checkpoint list
    public List<Transform> RaceCheckpointTransforms;

    // race track object
    private GameObject RaceTrackObject;

    // the next checkpoint index
    public int nextCheckpointIndex;

    // track what lap we are on
    private int currentLap = 0;

    // relay progress to RaceCheckpoints script
    public float raceProgress = 0f;
    public int numOfCheckpointsPassed = 0;
    private int totalLaps;
    public bool completedRace = false;

    // timer variable
    private float timeToCompleteRace = 0.0f;


    private void Awake()
    {
        // find the race track object in the scene
        RaceTrackObject = GameObject.FindWithTag("Race");

        // it exists
        if (RaceTrackObject != null)
        {
            // set this list equal to the list determined by RaceCheckpoints game object
            RaceCheckpointTransforms = RaceTrackObject.GetComponent<scr_RaceCheckpoints>().CheckpointTransforms;
            totalLaps = RaceTrackObject.GetComponent<scr_RaceCheckpoints>().GetNumOfLaps(); // Get the total number of

        }

        nextCheckpointIndex = 0; // Initialize the next checkpoint index

    }

    private void Update()
    {
        // increment timer
        // acount of delta time
        // if we are not completed with the race, increment the timer
        if (completedRace == false) 
        {
            timeToCompleteRace += Time.deltaTime;
        }
            
    }

    // handle checkpoint collision
    private void OnTriggerEnter(Collider other)
    {
        GameObject Checkpoint = other.gameObject;

        // check if the object is a checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            //Debug.Log("Checkpoint " + RaceCheckpointTransforms.IndexOf(Checkpoint.transform) + " reached by Car!");

            // determine if we are on the correct checkpoint
            if (RaceCheckpointTransforms.IndexOf(Checkpoint.transform) == nextCheckpointIndex && currentLap <= totalLaps)
            {
                // correct checkpoint passed
                //Debug.Log("Correct checkpoint passed!");
                nextCheckpointIndex = (nextCheckpointIndex + 1) % RaceCheckpointTransforms.Count; // increment the next checkpoint index
                numOfCheckpointsPassed++; // increment the number of checkpoints passed

                // increment the lap if we have reached the last checkpoint
                if (nextCheckpointIndex == 1)
                {
                    currentLap++; // Increment the lap count
                    //Debug.Log("Lap " + currentLap + " completed!");
                }

                // RaceTrackObject.GetComponent<scr_RaceCheckpoints>().SortRacerList();

            }
            else if (RaceCheckpointTransforms.IndexOf(Checkpoint.transform) == nextCheckpointIndex && currentLap > totalLaps)
            {
                if (!completedRace) 
                { 
                    completedRace = true; // toggle this on
                    RaceTrackObject.GetComponent<scr_RaceCheckpoints>().RacerCompletionOrder.Add(gameObject); // add this racer to the completion order list

                    // convert time to complete race to minutes and seconds
                    // xx:xx.xx format
                    string timeToCompleteRaceString = string.Format("{0:00}:{1:00.00}", Mathf.Floor(timeToCompleteRace / 60), timeToCompleteRace % 60);

                    // pass on the time to complete the race to the RaceCheckpoints script
                    RaceTrackObject.GetComponent<scr_RaceCheckpoints>().racerCompletionTimes.Add(timeToCompleteRaceString);
                }

            }
            else
            {
                // incorrect checkpoint passed
                //Debug.Log("Incorrect checkpoint passed! Missed checkpoint # " + nextCheckpointIndex);

            }

            // if we are on a lap higher than total laps, we have completed the race
            if (currentLap > totalLaps)
            {

                // check if race complete flag is toggled to false, if it is, toggle it to true and add this racer to the completion order list
                if (!completedRace) 
                {
                    completedRace = true; // toggle this on
                    RaceTrackObject.GetComponent<scr_RaceCheckpoints>().RacerCompletionOrder.Add(gameObject); // add this racer to the completion order list

                    // convert time to complete race to minutes and seconds
                    // xx:xx.xx format
                    string timeToCompleteRaceString = string.Format("{0:00}:{1:00.00}", Mathf.Floor(timeToCompleteRace / 60), timeToCompleteRace % 60);

                    // pass on the time to complete the race to the RaceCheckpoints script
                    RaceTrackObject.GetComponent<scr_RaceCheckpoints>().racerCompletionTimes.Add(timeToCompleteRaceString);
                }
            }
        }

    }

    // get race completion status
    public bool GetCompletedRaceStatus()
    {
        return completedRace;
    }

    // get the current lap number
    public int GetCurrentLap()
    {
        return currentLap;
    }

}
