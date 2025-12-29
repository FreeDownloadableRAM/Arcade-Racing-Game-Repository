using UnityEngine;

public class Scr_Pathfind_Object_Follow : MonoBehaviour
{

    // target to warp nav mesh agent to
    [SerializeField] private Transform target; // The target to follow

    // nave mesh agent component
    private UnityEngine.AI.NavMeshAgent navMeshAgent; // The NavMeshAgent component

    private void Start()
    {
        // get the nav mesh agent component
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // is it null?
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name);
        }
    } 



    // Update is called once per frame
    void Update()
    {
        // if agent exists
        if (navMeshAgent != null)
        {
            // check if the nav mesh agent is a certain distance away from the target
            if (Vector3.Distance(navMeshAgent.transform.position, target.position) > 5f)
            {
                // warp the nav mesh agent to the target's position
                navMeshAgent.Warp(target.position);
                
            }
            else 
            {
                // set the nav mesh agent's position to the target's position
                navMeshAgent.transform.position = target.position;

                // and its rotation as well
                navMeshAgent.transform.rotation = target.rotation;
            }

        }

        
    }
}
