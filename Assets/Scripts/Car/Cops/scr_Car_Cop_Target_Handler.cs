using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class scr_Car_Cop_Target_Handler : MonoBehaviour
{
    

    // the transform target for the ai to move towards
    public Vector3 AICopMovementTarget;

    // the actual transform of the car object we are targeting
    private GameObject ChaseTarget;

    // get the trigger volume from game world object
    // the first racer to enter the trigger volume will become the AI's target
    [SerializeField] public GameObject AICopTargetTriggerVolume;

    // AI state variable
    public string AIState = "Idle";

    // AI give up distance
    [SerializeField] private float AIGiveUpDistance;

    // ai give up timer
    [SerializeField] private float AIGiveUpChaseTime;
    private float AIGiveUpTimer;

    // switch side to approach the target car from
    private bool approachFromLeftSide = false;
    private bool approachFromRightSide = false;

    // switch side to approach the target car from time range
    [SerializeField] private float switchSideTimerMin;
    [SerializeField] private float switchSideTimerMax;

    // timer to switch side to approach from
    private float switchSideTimer;

    // distance to the side of the target car to aim for
    [SerializeField] private float sideApproachDistance;

    // starting location to return to when not chasing
    public Vector3 initialStartLocation;

    private void Awake()
    {
        // if the Ai give up distance is not set, set it to a default value
        if (AIGiveUpDistance <= 0f)
        {
            AIGiveUpDistance = 100f;
        }

        // if the Ai give up timer is not set, set it to a default value
        if (AIGiveUpChaseTime <= 0f)
        {
            AIGiveUpChaseTime = 60f;
        }

        // randomly choose to approach from left or right side
        if (UnityEngine.Random.value > 0.5f)
        {
            approachFromLeftSide = true;
            approachFromRightSide = false;
        }
        else
        {
            approachFromLeftSide = false;
            approachFromRightSide = true;
        }

        initialStartLocation = transform.position;
    }

    private void Start()
    {
        
        if (AICopMovementTarget == null) 
        { 
            AICopMovementTarget = initialStartLocation; 

        }

        if (AICopMovementTarget == new Vector3(0f, 0f, 0f)) 
        { 
            AICopMovementTarget = initialStartLocation;
        }
    }

    private void Update()
    {
        // if we are in idle state, run the idle state logic
        if (AIState == "Idle") 
        {
            IdleStateLogic();
        }

        // if we are in chase state, run the chase state logic
        if (AIState == "Chase")
        {
            ChaseStateLogic();
        }


    }

    private void IdleStateLogic()
    {
        // set target to the first racer that entered the trigger volume
        if (AICopTargetTriggerVolume.GetComponent<scr_Car_Cop_Target_Trigger_Volume>().targetedGameObject != null)
        {
            // get the transform of the targeted game object from the trigger volume script
            ChaseTarget = AICopTargetTriggerVolume.GetComponent<scr_Car_Cop_Target_Trigger_Volume>().targetedGameObject;



            // set ai state to chase
            AIState = "Chase";

            // reset the give up timer
            AIGiveUpTimer = AIGiveUpChaseTime;

        }

        // go back to the initial start location
        AICopMovementTarget = initialStartLocation;


    }

    private void ChaseStateLogic()
    {

        // exit condition for chase state
        // if the target is too far away, give up the chase and return to idle state
        if (Vector3.Distance(transform.position, ChaseTarget.transform.position) > AIGiveUpDistance)
        {
            // set ai state to chase
            AIState = "Idle";

            // go back to the initial start location
            AICopMovementTarget = initialStartLocation;

            return;

        }

        // if we chased for too long, give up the chase and return to idle state
        if (AIGiveUpTimer <= 0f)
        {
            // set ai state to chase
            AIState = "Idle";

            // go back to the initial start location
            AICopMovementTarget = initialStartLocation;

            return;
        }
        else
        {
            // decrement the timer
            // only if we are too far away from the target
            if (Vector3.Distance(transform.position, ChaseTarget.transform.position) > AIGiveUpDistance)
            {
                AIGiveUpTimer -= Time.deltaTime;

            }
            else 
            {
                // reset the timer if we are close enough to the target
                AIGiveUpTimer = AIGiveUpChaseTime;
            }
                
        }

        // switch side to approach from timer logic
        if (switchSideTimer <= 0f)
        {
            // randomly choose to approach from left or right side
            if (UnityEngine.Random.value > 0.5f)
            {
                approachFromLeftSide = true;
                approachFromRightSide = false;
            }
            else
            {
                approachFromLeftSide = false;
                approachFromRightSide = true;
            }

            // reset the timer to a random value between the min and max
            switchSideTimer = UnityEngine.Random.Range(switchSideTimerMin, switchSideTimerMax);

        }
        else
        {
            // decrement the timer
            switchSideTimer -= Time.deltaTime;
        }

        // okay we are in chase state, so set the initial target location to be to the side of the target car
        // which side are we trying to approach from?
        if (approachFromLeftSide)
        {
            // approach from left side
            // set initial target location to be some units to the left of the target car's position
            //initialTargetLocation.transform.position = ChaseTarget.transform.position - (ChaseTarget.transform.right * sideApproachDistance);

            // set this as the ai movement target
            //AICopMovementTarget.transform.position = ChaseTarget.transform.position - (ChaseTarget.transform.right * sideApproachDistance);

            // once we are close enough to the target location, switch to approaching the target car directly
            if (Vector3.Distance(transform.position, ChaseTarget.transform.position) < 4.5f)
            {
                // set ai movement target to be the target car's position
                AICopMovementTarget = ChaseTarget.transform.position;
            }
            else 
            {

                // keep aiming to the left of the target car
                // and account for the target car's orientation
                AICopMovementTarget = ChaseTarget.transform.position - (transform.rotation * (ChaseTarget.transform.right * sideApproachDistance));
                
            }



        }
        else if (approachFromRightSide)
        {
            // approach from Right side
            // set initial target location to be some units to the left of the target car's position
            //initialTargetLocation.transform.position = ChaseTarget.transform.position + (ChaseTarget.transform.right * sideApproachDistance);

            // set this as the ai movement target
            //AICopMovementTarget = initialTargetLocation;

            // once we are close enough to the target location, switch to approaching the target car directly
            if (Vector3.Distance(transform.position, ChaseTarget.transform.position) < 4.5f)
            {
                // set ai movement target to be the target car's position
                AICopMovementTarget = ChaseTarget.transform.position;
            }
            else
            {

                // keep aiming for the initial target location
                AICopMovementTarget = ChaseTarget.transform.position + (transform.rotation * (ChaseTarget.transform.right * sideApproachDistance));
            }



        }



        // throw new NotImplementedException();
    }

}
