using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class scr_RaceCheckpoints : MonoBehaviour
{

    public List<GameObject> Checkpoints;
    public List<Transform> CheckpointTransforms;

    public List<GameObject> Racers;
    public List<GameObject> RacerCompletionOrder; // List to store the order of racers based on completion

    //private List<scr_Checkpoint> CheckpointScripts;
    private int nextCheckpointIndex;

    // determine the amount of laps the race has
    [SerializeField] public int numOfLaps;

    private void Awake()
    {
        //CheckpointScripts = new List<scr_Checkpoint>();

        // go over the children of this object and find all the checkpoints
        // and add them to the list of checkpoints objects
        foreach (Transform child in transform)
        {
            if (child.tag == "Checkpoint")
            {
                Checkpoints.Add(child.gameObject);
                CheckpointTransforms.Add(child.transform);
            }
        }

        foreach (Transform checkpointTransform in CheckpointTransforms)
        {
            // get the scr_Checkpoint component and set
            scr_Checkpoint checkpoint = checkpointTransform.GetComponent<scr_Checkpoint>();
            checkpoint.SetRaceCheckpoints(this);


        }

        nextCheckpointIndex = 0; // Initialize the next checkpoint index

        // determine amount of racers in the scene
        // Players

        foreach (GameObject racer in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (racer != null)
            {
                Racers.Add(racer);
            }

        }
        // AI cars
        foreach (GameObject ai in GameObject.FindGameObjectsWithTag("AI"))
        {
            if (ai != null)
            {
                Racers.Add(ai);
            }
        }


    }

    public void ObjectThroughCheckpoint(scr_Checkpoint checkpoint)
    {

        Checkpoints.IndexOf(checkpoint.gameObject);
        //Debug.Log(" Checkpoint: # " + Checkpoints.IndexOf(checkpoint.gameObject));

        if (Checkpoints.IndexOf(checkpoint.gameObject) == nextCheckpointIndex)
        {
            // correct checkpoint passed
            nextCheckpointIndex = (nextCheckpointIndex + 1) % Checkpoints.Count; // Move to the next checkpoint
        }
        else
        {
            // wrong checkpoint passed
            //Debug.Log("Checkpoint missed: " + nextCheckpointIndex);
        }


    }

    /*
    public void SortRacerList()
    {
        // Old Implementation

        //Debug.Log("Racer List Sorted... ");

        // first we sort the list by the number of checkpoints passed
        // sorts the list
        Racers.Sort((r1, r2) => r1.GetComponent<scr_My_Race_Progress>().numOfCheckpointsPassed.CompareTo(r2.GetComponent<scr_My_Race_Progress>().numOfCheckpointsPassed));

        // reverse the list so that the racer with the most checkpoints is first
        Racers.Reverse();

        foreach (var racer in Racers)
        {
            //Debug.Log(racer.name + ": Pos: " + Racers.IndexOf(racer) + ". Checkpoint Number " + racer.GetComponent<scr_My_Race_Progress>().numOfCheckpointsPassed);
            //Debug.Log(racer);
        }


    }
    */

    private void FixedUpdate()
    {
        SortRacerList();
    
    }

    public void SortRacerList()
    {
        // First sort by checkpoints passed (descending),
        // then by distance to next checkpoint (ascending)
        Racers = Racers
            .OrderByDescending(r => r.GetComponent<scr_My_Race_Progress>().numOfCheckpointsPassed)
            .ThenBy(r => GetDistanceToNextCheckpoint(r))
            .ToList();

        foreach (var racer in Racers)
        {
            var progress = racer.GetComponent<scr_My_Race_Progress>();
            //Debug.Log($"{racer.name} - Pos {Racers.IndexOf(racer) + 1} | Checkpoints: {progress.numOfCheckpointsPassed} | DistToNext: {GetDistanceToNextCheckpoint(racer):F2}");
        }
    }

    private float GetDistanceToNextCheckpoint(GameObject racer)
    {
        var progress = racer.GetComponent<scr_My_Race_Progress>();

        // Which checkpoint should this racer go to next?
        int nextCheckpointIndex = progress.numOfCheckpointsPassed % Checkpoints.Count;

        Transform nextCheckpoint = CheckpointTransforms[nextCheckpointIndex];

        return Vector3.Distance(racer.transform.position, nextCheckpoint.position);
    }

    public int GetRacerPosition(GameObject racer)
    {
        return Racers.IndexOf(racer); // returns 1 position
    }

    public int GetRacerCompletionPosition(GameObject racer)
    {
        return RacerCompletionOrder.IndexOf(racer); // returns position
    }

    // return gameboject of a racer given their position
    public GameObject GetRacerByPosition(int position)
    {
        if ((position <= Racers.Count))
        {
            if (position >= 0)
            {
                return Racers[position]; // returns position of the racer, 0 = 0 in the list. so with 12 racers, racer position 12 = index 11 in the list
            }
            else 
            {
                return Racers[Racers.Count() - 1]; // invalid position, just set value to the last racer in the list
            }
        }
        else
        {
            return Racers[Racers.Count() - 1]; // invalid position, just set value to the last racer in the list
        }
    }
}
