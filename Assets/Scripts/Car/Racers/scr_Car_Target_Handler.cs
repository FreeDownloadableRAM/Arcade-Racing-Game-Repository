
/* OLD code - kept for reference
 * 
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class scr_Car_Target_Handler : MonoBehaviour
{
    // The target transform to follow
    public Transform AIMovementTarget;

    // race track object
    private scr_My_Race_Progress MyRaceProgress;


    // AI state variable
    [SerializeField] private string AIState = "Race";

    

    private void Awake()
    {
        MyRaceProgress = GetComponent<scr_My_Race_Progress>();
    }

    private void Start()
    {
        if (AIMovementTarget == null)
        {
            // If no target is set, default to this cars transform
            AIMovementTarget = GetComponent<Transform>();
            

        }
    }

    private void Update()
    {
        // We are racing, not attacking
        if (AIState == "Race") 
        {
            // simply set target to the next checkpoint
            AIMovementTarget = MyRaceProgress.RaceCheckpointTransforms[MyRaceProgress.nextCheckpointIndex];

            // exit conditions to other AI states would go here
            // Get Item state condition
            // if there is an item box within a certain range, inbetween us and the next checkpoint
            // go for it
            

        }
        if (AIState == "Get Item")
        {
            // code to get item would go here




            // exit conditions to other AI states would go here
        }

    }
}

*/

// new code
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class scr_Car_Target_Handler : MonoBehaviour
{
    public Transform AIMovementTarget;

    private scr_My_Race_Progress MyRaceProgress;

    [SerializeField] private string AIState = "Race";

    // get car ai simple script
    private CarAISimple scr_CarAISimple;

    [Header("Item Box Settings")]
    public float itemDetectRange = 75f;           // How far ahead to look for item boxes
    public LayerMask itemBoxLayer;                // Layer for item boxes
    public LayerMask obstacleMask;                // Layer for obstacles (walls, track boundaries, etc.)
    public float alignmentThreshold = 0.65f;       // Forward direction alignment (0–1)
    public float itemReachedDistance = 1.25f;        // Distance at which we consider the item "collected"

    private Transform nearestItemBox;

    // Cache for all detected item boxes this frame (for debug drawing)
    private List<Collider> detectedBoxes = new List<Collider>();

    // item handler script reference
    private Scr_Item_Handler itemHandler;

    // get race manager script from race manager game object
    // race track object
    private GameObject RaceTrackObject;

    // race manager script reference
    private scr_RaceCheckpoints scr_raceCheckpointsScript;

    // object racer to keep track of
    private GameObject Racer;

    // track Racer position
    private int position;

    // chase timer
    // once this reachers zero, reset back to race state.
    private float chaseTimer = 7f; // seconds to chase before giving up and going back to racing

    // bounds for chase timer
    [SerializeField] private float chaseTimerLowerBound = 7f;
    [SerializeField] private float chaseTimerUpperBound = 12f;

    // chase cooldown timer, prevents us from immediately re-entering chase state after exiting it.
    private float chaseCooldownTimer = 5f; // in seconds

    // bounds for chase cooldown timer
    [SerializeField] private float chaseCooldownTimerLowerBound = 5f;
    [SerializeField] private float chaseCooldownTimerUpperBound = 15f;

    // chase distance threshold
    private float chaseDistanceThreshold = 50f; // if the target is farther than this, we won't chase

    // chase target game object.
    private GameObject chaseTarget;

    private void Awake()
    {
        // get your race progress script
        MyRaceProgress = GetComponent<scr_My_Race_Progress>();

        // set up layer masks if not set in inspector
        if (itemBoxLayer == 0)
        {
            itemBoxLayer = LayerMask.GetMask("ItemBoxes");

        }
        if (obstacleMask == 0)
        {
            obstacleMask = LayerMask.GetMask("Obstacles", "Terrain");
        }

        // get item handler script
        itemHandler = GetComponent<Scr_Item_Handler>();

        // find the race track object in the scene
        // this will have the racers placement data that we need to home in on the correct target
        RaceTrackObject = GameObject.FindWithTag("Race");

        // get the race checkpoints script from the race track object
        scr_raceCheckpointsScript = RaceTrackObject.GetComponent<scr_RaceCheckpoints>();

        // set racer to this game object
        Racer = gameObject;
    }

    private void Start()
    {
        if(AIMovementTarget == null)
        {
            // If no target is set, default to this cars transform
            AIMovementTarget = GetComponent<Transform>();

        }

        // get car ai simple component
        // this is for our box cast on items
        scr_CarAISimple = GetComponent<CarAISimple>();

    }

    // AI State Machine 
    private void Update()
    {
        // thats the value of the racer that the homing target should be heading towards.
        position = scr_raceCheckpointsScript.GetRacerPosition(Racer);

        switch (AIState)
        {
            case "Race":
                HandleRaceState();
                break;

            case "Chase":
                HandleChaseState();
                break;

            case "Get Item":
                HandleGetItemState();
                break;
        }
    }

    private void HandleChaseState()
    {
        // count down chase timer
        chaseTimer -= Time.deltaTime;

        // if we have an OFFENSIVE item, set AI movement target to the racer in the position ahead of us, if there is one. If there isnt any, default to race state.
        // only chase with items that require us to aim. Dont chase with flamethrower as its good for zoning.
        // anything else, default to race state.

        // set the homing target
        int targetPositionIndex = position - 1; // get the position index of the racer ahead of us

        // make sure target position index is within bounds
        if (targetPositionIndex < 0)
        {
            // set chase timer cooldown so we dont immediately re-enter chase state after exiting it
            chaseCooldownTimer = UnityEngine.Random.Range(chaseCooldownTimerLowerBound, chaseCooldownTimerUpperBound);

            AIState = "Race";
        }
        else 
        {
            // get the game object we are going to chase
            chaseTarget = scr_raceCheckpointsScript.GetRacerByPosition(targetPositionIndex);

            // check if that car game object has health, if its dead, do not chase it.
            if (chaseTarget == null || chaseTarget.GetComponent<Scr_Car_Health>().GetCurrentHealth() <= 0)
            {
                Transform nextCheckpoint = MyRaceProgress.RaceCheckpointTransforms[MyRaceProgress.nextCheckpointIndex];
                AIMovementTarget = nextCheckpoint;

            }
            else 
            {
                // set our ai movement target to the transform of the target we are chasing
                AIMovementTarget = chaseTarget.transform;
            }

        }

        // if chase timer reachers zero, set to race ai state
        if (chaseTimer <= 0f)
        {
            // set chase timer cooldown so we dont immediately re-enter chase state after exiting it
            chaseCooldownTimer = UnityEngine.Random.Range(chaseCooldownTimerLowerBound, chaseCooldownTimerUpperBound);

            AIState = "Race";
        }

    }

    private void HandleRaceState()
    {
        Transform nextCheckpoint = MyRaceProgress.RaceCheckpointTransforms[MyRaceProgress.nextCheckpointIndex];
        AIMovementTarget = nextCheckpoint;

        // increment chase cooldown timer, when its zero, we can chase again if we get another item
        if (chaseCooldownTimer > 0f) 
        {
            chaseCooldownTimer -= Time.deltaTime;
        }

        // only look for item box if we don't already have an item
        if (itemHandler.getItemHeld() == "None")
        {
            // get nearest item box between us and next checkpoint
            nearestItemBox = FindItemBoxBetween(transform.position, nextCheckpoint.position);

            // if there is one, switch to Get Item state
            if (nearestItemBox != null)
            {
                Scr_Item_Box boxScript = nearestItemBox.GetComponent<Scr_Item_Box>();

                // check if item box is permitted to be targeted
                // only chase after box if you are in a certain checkpoint progress.
                // ex. item set 1 can only be targeted by ai racers who are going towards checkpoint 1
                if (MyRaceProgress.nextCheckpointIndex == boxScript.GetAIItemBoxTargetCheckpointRequirment())
                {
                    AIState = "Get Item";

                }
            }

        }
        else 
        {
            // we have an item, check if its an offensive item that we have to aim with
            // if so, enter chase state
            if (itemHandler.getItemHeld() == "Rocket" || itemHandler.getItemHeld() == "Laser" || itemHandler.getItemHeld() == "Ion Beam")
            {
                // set the homing target
                int targetPositionIndex = position - 1; // get the position index of the racer ahead of us

                if (targetPositionIndex >= 0 && chaseCooldownTimer <= 0) 
                {
                    // check if distance between this object and the target chase object is within a certain range. If its close enough, we can chase.
                    // get the game object we are going to chase
                    chaseTarget = scr_raceCheckpointsScript.GetRacerByPosition(targetPositionIndex);

                    // calculate distance to target
                    if (chaseTarget != null) 
                    {
                        float distanceToTarget = Vector3.Distance(transform.position, chaseTarget.transform.position);

                        if (distanceToTarget <= chaseDistanceThreshold) 
                        {
                            // target is close enough, enter chase state
                            // set chase timer
                            chaseTimer = UnityEngine.Random.Range(chaseTimerLowerBound, chaseTimerUpperBound);

                            AIState = "Chase";
                        }
                    }
                }
            }
        }
    }

    private void HandleGetItemState()
    {
        // exit condition, if theres no nearest item box, go back to racing
        if (nearestItemBox == null)
        {
            AIState = "Race";
            return;
        }

        RaycastHit hitInfo;
        float castRange = 75f;   // distance forward

        LayerMask ItemLayerMask = LayerMask.GetMask("ItemBoxes"); // Layer mask to filter for only Items

        // Origin of cast
        Vector3 origin = scr_CarAISimple.getCarOrigin();

        // cast a box collider, if there is any item box DIRECTLY IN FRONT OF US
        Vector3 boxHalfExtentsItem = new Vector3(15f, 4f, 5f); // adjust box size as needed

        // Forward direction
        Vector3 direction = transform.forward;

        // Perform BoxCast 
        // orientation of the box aligns with the car's rotation 
        bool hit = Physics.BoxCast(origin, boxHalfExtentsItem, direction, out hitInfo, transform.rotation, castRange, ItemLayerMask);
        if (hit)
        {
            // check if we collided with game object tagged as item
            if (hitInfo.collider.CompareTag("ItemBox"))
            {
                // Scr_Car_AI_Item_Behaviour.DrawBoxCast(origin, boxHalfExtentsItem, transform.rotation, transform.forward, castRange, Color.green);

                // if there is, set target to it
                AIMovementTarget = nearestItemBox;

                // Exit conditions

                // Check box availability
                // if there is no item available, go back to racing
                Scr_Item_Box boxScript = nearestItemBox.GetComponent<Scr_Item_Box>();
                if (boxScript == null || !boxScript.IsItemAvailable())
                {
                    nearestItemBox = null;
                    AIState = "Race";
                    return;
                }

                if (!nearestItemBox.gameObject.activeInHierarchy)
                {
                    nearestItemBox = null;
                    AIState = "Race";
                    return;
                }

                // if we reached item box, go back to racing
                float dist = Vector3.Distance(transform.position, nearestItemBox.position);
                if (dist < itemReachedDistance)
                {
                    nearestItemBox = null;
                    AIState = "Race";
                }

            }
            else 
            {
                // Scr_Car_AI_Item_Behaviour.DrawBoxCast(origin, boxHalfExtentsItem, transform.rotation, transform.forward, castRange, Color.orange);
                AIState = "Race";
                return;

            }

        }
        else 
        {
            // Scr_Car_AI_Item_Behaviour.DrawBoxCast(origin, boxHalfExtentsItem, transform.rotation, transform.forward, castRange, Color.red);
            AIState = "Race";
            return;

        }

    }

    // Find the best item box between the car and the checkpoint
    private Transform FindItemBoxBetween(Vector3 carPos, Vector3 checkpointPos)
    {
        detectedBoxes.Clear(); // Reset list for debug drawing
        Collider[] hits = Physics.OverlapSphere(carPos, itemDetectRange, itemBoxLayer);

        Transform bestItem = null;
        float bestScore = float.MaxValue;

        foreach (Collider c in hits)
        {
            detectedBoxes.Add(c);

            Scr_Item_Box boxScript = c.GetComponent<Scr_Item_Box>();
            if (boxScript == null || !boxScript.IsItemAvailable())
                continue; // Skip unavailable boxes

            Vector3 toItem = c.transform.position - carPos;
            Vector3 toCheckpoint = checkpointPos - carPos;

            // Flatten vectors to ignore vertical differences (recommended for racers)
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 flatToItem = Vector3.ProjectOnPlane(toItem, Vector3.up);

            // HARD reject: item is behind the car
            if (Vector3.Dot(flatForward, flatToItem.normalized) <= 0f)
                continue;

            // Soft alignment check (cone in front of the car)
            float alignment = Vector3.Dot(flatForward, flatToItem.normalized);
            if (alignment < alignmentThreshold)
                continue;


            float proj = Vector3.Dot(toItem.normalized, toCheckpoint.normalized);
            if (proj < 0.5f)
                continue;

            if (!HasLineOfSight(carPos, c.transform.position))
                continue;

            float dist = toItem.magnitude;
            if (dist < bestScore)
            {
                bestScore = dist;
                bestItem = c.transform;
            }
        }

        

        return bestItem;
    }

    private bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        if (Physics.Raycast(from + Vector3.up * 0.5f, direction.normalized, out RaycastHit hit, distance, obstacleMask))
            return false;

        return true;
    }

    // debugging gizmos
    private void OnDrawGizmosSelected()
    {
        // Draw item detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, itemDetectRange);

        // Draw available/unavailable item box debug lines
        if (detectedBoxes != null)
        {
            foreach (Collider c in detectedBoxes)
            {
                if (c == null) continue;

                Scr_Item_Box boxScript = c.GetComponent<Scr_Item_Box>();
                bool hasLOS = HasLineOfSight(transform.position, c.transform.position);

                if (boxScript == null)
                    continue;

                // Choose color
                if (!boxScript.IsItemAvailable())
                    Gizmos.color = Color.red; // Unavailable
                else if (nearestItemBox == c.transform)
                    Gizmos.color = Color.green; // Currently targeted
                else if (hasLOS)
                    Gizmos.color = Color.blue; // Available & visible
                else
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f); // Grey for no LOS

                Gizmos.DrawLine(transform.position, c.transform.position);
            }
        }
    }
}



