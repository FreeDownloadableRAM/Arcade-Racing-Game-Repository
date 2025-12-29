using JetBrains.Annotations;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Scr_Car_Pathfinder : MonoBehaviour
{

    public NavMeshAgent agent; // Reference to the NavMeshAgent component
    [SerializeField] public Transform endtarget; // The target to move towards
    [SerializeField] private float arrivedAtCornerThres; // The target to move towards

    private scr_Car_Target_Handler carTargetHandler; // Reference to the car target handler script

    public LineRenderer lineRenderer; // Reference to the LineRenderer component
    
    // our target to send to car ai
    public Vector3 carTarget; // The target to send to the car AI

    // pathfinding calculation frequency
    private float pathFindRecalculateTimer;

    private void Awake()
    {
        carTargetHandler = GetComponent<scr_Car_Target_Handler>();

        
    }

    void Start() 
    {
        // set random colour for the line renderer
        lineRenderer.startColor = new Color(Random.value, Random.value, Random.value, 0.5f); // Set a random color for the start of the line
        lineRenderer.endColor = new Color(Random.value, Random.value, Random.value, 0.5f); // Set a random color for the end of the line

        // set default target for the endtarget
        if (endtarget == null) 
        {

            // set target to this object's transform
            endtarget = GetComponent<Transform>();
        }

        pathFindRecalculateTimer = 6f;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        // run this only once every second
        if (pathFindRecalculateTimer > 0)
        {
            pathFindRecalculateTimer--;

        }
        else 
        {
            SetTarget();

            pathFindRecalculateTimer = 6f;
        }

            
        
    }

    void SetTarget() 
    {
        endtarget = carTargetHandler.AIMovementTarget; // Get the target from the car target handler

        // check if agent is assigned
        if (agent != null) 
        {
            // check if agent is on nav mesh
            if (agent.isOnNavMesh) 
            {
                // set the destination
                agent.SetDestination(endtarget.position); // Set the destination of the NavMeshAgent to the target's position

                // calculate the path

                agent.CalculatePath(endtarget.position, agent.path);


                if (agent.hasPath)
                {
                    NavMeshPath path = agent.path;



                    if (path.corners.Length > 0)
                    {
                        // Visualize the path using a LineRenderer
                        lineRenderer.positionCount = path.corners.Length;
                        for (int i = 0; i < path.corners.Length; i++)
                        {
                            lineRenderer.SetPosition(i, path.corners[i]);
                        }

                        if (path.corners.Length == 1)
                        {
                            // If there's only one corner, set the target to that corner
                            // if we are close enough to corner, set target to the end target
                            if (Vector3.Distance(transform.position, path.corners[0]) < arrivedAtCornerThres)
                            {

                                carTarget = endtarget.position;
                                //Debug.Log("Car is close enough to the corner, setting target to end target: " + carTarget);
                            }
                            else
                            {
                                carTarget = path.corners[0];
                                //Debug.Log("Car is NOT clost enough to corner, target is to the only corner: " + carTarget);
                            }

                        }
                        else
                        {
                            // If there are multiple corners, set the target to the second corner
                            // first check if we are close enough to the second corner, if we are
                            // set the target to the final corner
                            if (Vector3.Distance(transform.position, path.corners[1]) < arrivedAtCornerThres)
                            {

                                carTarget = path.corners[path.corners.Length - 1];
                                //Debug.Log("Car is close enough to the second corner, setting target to the final corner: " + carTarget);
                            }
                            else
                            {
                                carTarget = path.corners[1];
                                //Debug.Log("Car is NOT close enough to the second corner, target is to the second corner: " + carTarget);
                            }

                        }


                    }
                    else
                    {
                        lineRenderer.positionCount = 0;
                        carTarget = endtarget.position;
                    }


                }
                else
                {
                    lineRenderer.positionCount = 0;
                    carTarget = endtarget.position;
                }


            }

        
        }

        

    }

    // return the 2nd corner in the path
    public Vector3 GetNextPathVertexPosition() 
    {
        if (agent != null && agent.hasPath) 
        {
            NavMeshPath path = agent.path;
            if (path.corners.Length > 1) 
            {
                return path.corners[1];
            }
            else if (path.corners.Length == 1) 
            {
                return path.corners[0];
            }
            else 
            {
                return endtarget.position;
            }
        }
        else 
        {
            return endtarget.position;
        }
    }

    // return first corner in the parth
    public Vector3 GetFirstVertexPosition() 
    {
        if (agent != null && agent.hasPath) 
        {
            NavMeshPath path = agent.path;
            if (path.corners.Length > 0) 
            {
                return path.corners[0];
            }
            else 
            {
                return endtarget.position;
            }
        }
        else 
        {
            return endtarget.position;
        }
    }

}
