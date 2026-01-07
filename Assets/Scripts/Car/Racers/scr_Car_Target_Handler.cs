
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
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class scr_Car_Target_Handler : MonoBehaviour
{
    public Transform AIMovementTarget;

    private scr_My_Race_Progress MyRaceProgress;

    [SerializeField] private string AIState = "Race";

    [Header("Item Box Settings")]
    public float itemDetectRange = 75f;           // How far ahead to look for item boxes
    public LayerMask itemBoxLayer;                // Layer for item boxes
    public LayerMask obstacleMask;                // Layer for obstacles (walls, track boundaries, etc.)
    public float alignmentThreshold = 0.65f;       // Forward direction alignment (0–1)
    public float itemReachedDistance = 1.5f;        // Distance at which we consider the item "collected"

    private Transform nearestItemBox;

    // Cache for all detected item boxes this frame (for debug drawing)
    private List<Collider> detectedBoxes = new List<Collider>();

    // item handler script reference
    private Scr_Item_Handler itemHandler;

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
    }

    private void Start()
    {
        if(AIMovementTarget == null)
        {
            // If no target is set, default to this cars transform
            AIMovementTarget = GetComponent<Transform>();

        }

    }

    // AI State Machine 
    private void Update()
    {
        switch (AIState)
        {
            case "Race":
                HandleRaceState();
                break;

            case "Get Item":
                HandleGetItemState();
                break;
        }
    }

    private void HandleRaceState()
    {
        Transform nextCheckpoint = MyRaceProgress.RaceCheckpointTransforms[MyRaceProgress.nextCheckpointIndex];
        AIMovementTarget = nextCheckpoint;

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
                else 
                {
                    AIState = "Race";


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

            float angle = Vector3.Angle(flatForward, flatToItem);
            if (angle > 60f) // degrees
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



