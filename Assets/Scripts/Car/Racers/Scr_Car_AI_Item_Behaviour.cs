using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.LowLevelPhysics;
using static UnityEngine.UI.Image;

public class Scr_Car_AI_Item_Behaviour : MonoBehaviour
{
    // reference item handler so we know what item we are holding
    private Scr_Item_Handler scr_ItemHandler;

    // what item are we holding
    private string itemHeld;

    // conditional checks for item use
    // Nitro
    private bool isInNitroTriggerZone = false;

    // my race progress to get next checkpoint position
    private scr_My_Race_Progress scr_MyRaceProgress;

    private Transform nextCheckpoint;

    // ai thresholds
    [SerializeField] private float aiHealThreshold = 0.75f; // health threshold to use healthpack

    // for health pack usage, we need to get out current health
    private Scr_Car_Health scr_CarHealth;

    // get car ai simple script
    private CarAISimple scr_CarAISimple;

    // item properties related to ai use
    // rocket properties for non-predictive firing
    private float firingAngleThreshold = 1.25f; // angle threshold in degrees to fire rocket

    // rocket properties for predictive firing
    private float initialRocketSpeed = 100f; // initial speed of rocket in units per second
    private float maxRocketSpeed = 375f; // max speed of rocket in units per second
    private float rocketAcceleration = 175f; // acceleration of rocket in units per second squared
    private float rocketMass = 1f; // mass of rocket in kg

    // debugging purposes for rocket firing
    // --- Debugging ---
    private Vector3 debugInterceptPoint;
    private List<Vector3> debugTargetArc = new List<Vector3>();
    private List<Vector3> debugRocketPath = new List<Vector3>();
    private bool debugHasSolution = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get the item handler component
        scr_ItemHandler = GetComponent<Scr_Item_Handler>();

        // get item held
        itemHeld = scr_ItemHandler.getItemHeld();

        // get race progress component
        scr_MyRaceProgress = GetComponent<scr_My_Race_Progress>();

        // set next checkpoint to the first checkpoint in the checkpoint list
        // by default
        nextCheckpoint = scr_MyRaceProgress.RaceCheckpointTransforms[scr_MyRaceProgress.nextCheckpointIndex];

        // get car health component
        scr_CarHealth = GetComponent<Scr_Car_Health>();

        // get car ai simple component
        scr_CarAISimple = GetComponent<CarAISimple>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // update item held
        itemHeld = scr_ItemHandler.getItemHeld();

        // if we have no items at all, exit
        if (itemHeld == "None") 
        {
            return;
        }

        // Nitro Use Case
        // Check if we are in a nitro trigger zone and have nitro to us
        if (itemHeld == "Nitro") 
        {
            if (isInNitroTriggerZone) 
            {
                // get the transform of the next checkpoint
                nextCheckpoint = scr_MyRaceProgress.RaceCheckpointTransforms[scr_MyRaceProgress.nextCheckpointIndex];

                // make sure our angle to next checkpoint is within acceptable range
                float alignment = Vector3.Dot((nextCheckpoint.position - transform.position).normalized, transform.forward);

                if (alignment > 0.9f) // adjust threshold as needed
                {
                    // use nitro
                    scr_ItemHandler.UseItemNitro();

                    // debug, log nitro use
                    // Debug.Log("Nitro used by " + gameObject.name + " at checkpoint " + scr_MyRaceProgress.nextCheckpointIndex + " angle alignment: " + alignment);

                }

            }


        }

        // healthpack use case
        // check if our health is below a set threshold and we have a healthpack to use
        if (itemHeld == "Health Pack") 
        { 
            if (scr_CarHealth.GetCurrentHealth() < aiHealThreshold * scr_CarHealth.GetMaxHealth()) 
            {
                // use health pack
                scr_ItemHandler.UseItemHealthPack();
                // debug, log health pack use
                // Debug.Log("Health Pack used by " + gameObject.name + " at health: " + scr_CarHealth.GetCurrentHealth());
            }

        }
        
        // Rocket Use Case
        if (itemHeld == "Rocket")
        {
            RaycastHit hitInfo;
            float castRange = 175f;   // distance forward
            Vector3 boxHalfExtents = new Vector3(10f, 2f, 5f); // adjust box size as needed

            // Origin of cast
            Vector3 origin = scr_CarAISimple.getCarOrigin();

            // Forward direction
            Vector3 direction = transform.forward;

            // Perform BoxCast 
            // orientation of the box aligns with the car's rotation 
            bool hit = Physics.BoxCast(origin, boxHalfExtents, direction, out hitInfo, transform.rotation, castRange);
            
            if (hit)
            {
                
                // if we hit a cop, dont use item
                if (hitInfo.collider.CompareTag("Cop"))
                {
                    Debug.DrawRay(origin, direction * castRange, Color.red);

                    // draw box cast for debugging
                    DrawBoxCast(origin, boxHalfExtents, transform.rotation, transform.forward, castRange, Color.red);
                }
                // check if we hit a car
                else if (hitInfo.collider.CompareTag("Player") || hitInfo.collider.CompareTag("Cars"))
                {
                    // set rocket initial speed based on our linear velocity
                    initialRocketSpeed = GetComponent<Rigidbody>().linearVelocity.magnitude * 1.1f; // add some extra speed

                    // first get the distance, if we are closer than a certain threshold, fire rocket
                    if (hitInfo.distance < 15f) // adjust close range threshold as needed
                    {
                        // get direction vector to the target inside our box collider
                        Vector3 directionToTarget = (hitInfo.collider.transform.position - transform.position).normalized;

                        fireRocketNonPredictive(directionToTarget, 1.5f, hitInfo, boxHalfExtents, origin, direction, castRange);

                    }
                    else 
                    {
                        // get direction vector to the target inside our box collider
                        Vector3 directionToTarget = (hitInfo.collider.transform.position - transform.position).normalized;

                        // get the root game object of the target car
                        GameObject targetRoot =
                        hitInfo.collider.GetComponentInParent<CarControllerAI>()?.gameObject ??
                        hitInfo.collider.GetComponentInParent<CarController>()?.gameObject ??
                        hitInfo.collider.gameObject;

                        fireRocketPredictive(directionToTarget, targetRoot, hitInfo, 
                            initialRocketSpeed, maxRocketSpeed, rocketAcceleration, rocketMass, 
                                firingAngleThreshold);

                    }

                }
                else
                {
                    Debug.DrawRay(origin, direction * castRange, Color.yellow);

                    // draw box cast for debugging
                    DrawBoxCast(origin, boxHalfExtents, transform.rotation, transform.forward, castRange, Color.orange);
                }

                
            }
        }

        // just for debugging, use missile the moment we get it
        if (itemHeld == "Missile")
        {
            scr_ItemHandler.UseItemMissile();
        }

    }


    // Nitro Use case Hint
    // toggle sInNitroTriggerZone when we enter a nitro trigger zone to true
    private void OnTriggerEnter(Collider other)
    {
        // if we collide with Nitro Trigger volume
        if (other.gameObject.CompareTag("NitroZone"))
        {
            isInNitroTriggerZone = true;
        }
    }

    // switch isInNitroTriggerZone to false when we exit a nitro trigger zone
    private void OnTriggerExit(Collider other)
    {
        // if we exit a Nitro Trigger volume
        if (other.gameObject.CompareTag("NitroZone"))
        {
            isInNitroTriggerZone = false;
        }
    }

    public static void DrawBoxCast(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
    {
        // Calculate the 8 corners of the box at the start and end of the cast
        Vector3[] startPoints = new Vector3[8];
        Vector3[] endPoints = new Vector3[8];

        // Local corner positions
        Vector3[] corners = new Vector3[8]
        {
        new Vector3( halfExtents.x,  halfExtents.y,  halfExtents.z),
        new Vector3( halfExtents.x,  halfExtents.y, -halfExtents.z),
        new Vector3( halfExtents.x, -halfExtents.y,  halfExtents.z),
        new Vector3( halfExtents.x, -halfExtents.y, -halfExtents.z),
        new Vector3(-halfExtents.x,  halfExtents.y,  halfExtents.z),
        new Vector3(-halfExtents.x,  halfExtents.y, -halfExtents.z),
        new Vector3(-halfExtents.x, -halfExtents.y,  halfExtents.z),
        new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z)
        };

        // Compute world positions for start and end boxes
        for (int i = 0; i < 8; i++)
        {
            startPoints[i] = origin + orientation * corners[i];
            endPoints[i] = startPoints[i] + direction.normalized * distance;
        }

        // Draw edges for a box (4 top edges, 4 bottom edges, 4 vertical edges)
        void DrawBox(Vector3[] pts)
        {
            // Top square
            Debug.DrawLine(pts[0], pts[1], color);
            Debug.DrawLine(pts[1], pts[3], color);
            Debug.DrawLine(pts[3], pts[2], color);
            Debug.DrawLine(pts[2], pts[0], color);

            // Bottom square
            Debug.DrawLine(pts[4], pts[5], color);
            Debug.DrawLine(pts[5], pts[7], color);
            Debug.DrawLine(pts[7], pts[6], color);
            Debug.DrawLine(pts[6], pts[4], color);

            // Vertical edges
            Debug.DrawLine(pts[0], pts[4], color);
            Debug.DrawLine(pts[1], pts[5], color);
            Debug.DrawLine(pts[2], pts[6], color);
            Debug.DrawLine(pts[3], pts[7], color);
        }

        // Draw start box
        DrawBox(startPoints);

        // Draw end box
        DrawBox(endPoints);

        // Connect corresponding corners between start and end
        for (int i = 0; i < 8; i++)
        {
            Debug.DrawLine(startPoints[i], endPoints[i], color);
        }
    }

    // predictive rocket firing function
    // get direction to target as input
    // get target velocity and our rocket velocity to predict where and when to fire
    private void fireRocketPredictive(Vector3 directionToTarget, GameObject targetObject, RaycastHit hitInfo,
        float rocketInitialSpeed, float maxRocketSpeed, float rocketAcceleration, float rocketMass,
            float firingAngleThreshold)
    {
        // ----------------------------
        // 1. Extract target state
        // ----------------------------
        Rigidbody rb = targetObject.GetComponent<Rigidbody>();
        Vector3 targetVel = rb != null ? rb.linearVelocity : Vector3.zero;

        float targetAcc;
        if (hitInfo.collider.CompareTag("Cars"))
            targetAcc = targetObject.GetComponentInParent<CarControllerAI>().getAcceleration();
        else if (hitInfo.collider.CompareTag("Player"))
            targetAcc = targetObject.GetComponentInParent<CarController>().getAcceleration();
        else
            targetAcc = 0;

        Vector3 targetAccVec = targetAcc * targetObject.transform.forward;
        Vector3 targetPos = targetObject.transform.position;

        // ----------------------------
        // 2. Rocket launch geometry
        // ----------------------------
        Vector3 rocketPos =
            transform.position
            + transform.forward * scr_ItemHandler.getRocketSpawnOffset()
            + Vector3.up * scr_ItemHandler.getRocketSpawnHeightOffset();

        Vector3 fwd = transform.forward;

        // ----------------------------
        // 3. Try to find future intercept along forward direction only
        // ----------------------------
        float tHit = FindStraightLineInterceptTime(
            rocketPos, fwd,
            targetPos, targetVel, targetAccVec,
            rocketInitialSpeed, rocketAcceleration, maxRocketSpeed
        );

        if (tHit < 0f)
            return; // no intercept possible

        // Predicted intercept position
        Vector3 predictedTargetPos =
            targetPos
            + targetVel * tHit
            + 0.5f * targetAccVec * (tHit * tHit);

        // --------- DEBUG DATA ------------------------------------
        debugHasSolution = true;
        debugInterceptPoint = predictedTargetPos;

        // Build target arc for visualization
        debugTargetArc.Clear();
        float debugArcDuration = Mathf.Min(tHit, 5f);
        for (float t = 0; t <= debugArcDuration; t += 0.1f)
        {
            Vector3 pos =
                targetPos +
                targetVel * t +
                0.5f * targetAccVec * (t * t);

            debugTargetArc.Add(pos);
        }

        // Build rocket path visualization
        debugRocketPath.Clear();
        Vector3 rocketStart = rocketPos;

        float dt = 0.1f;
        float currentSpeed = rocketInitialSpeed;
        Vector3 currentPos = rocketStart;

        for (float t = 0; t <= debugArcDuration; t += dt)
        {
            // accelerate rocket
            currentSpeed = Mathf.Min(currentSpeed + rocketAcceleration * dt, maxRocketSpeed);
            currentPos += transform.forward * currentSpeed * dt;

            debugRocketPath.Add(currentPos);
        }


        // Direction to intercept point
        Vector3 aimDir = (predictedTargetPos - rocketPos).normalized;

        // ----------------------------
        // 4. Check angular threshold
        // ----------------------------
        float angle = Vector3.Angle(fwd, aimDir);
        if (angle > firingAngleThreshold)
            return; // shooter isn't pointing close enough

        // ----------------------------
        // 5. FIRE
        // ----------------------------
        scr_ItemHandler.UseItemRocket(); // rocket fired straight
    }


    // non-predictive rocket firing function
    // get direction to target as input
    // check if angle towards target is within angle threshold to fire rocket, then fire
    private void fireRocketNonPredictive(Vector3 directionToTarget, float angleThreshold, RaycastHit hitInfo, Vector3 boxHalfExtents, 
            Vector3 origin, Vector3 direction, float castRange) 
    {
        // get the angle between the cars forward vector and the vector towards our target
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // do a physics ray cast from the cars forward vector
        Ray rayToTarget = new Ray(origin, directionToTarget);
        // layer mask to ignore terrain and obstacles
        LayerMask layerMask = LayerMask.GetMask("PlayerCars", "Cars");
        if (Physics.Raycast(rayToTarget, out RaycastHit forwardHitInfo, castRange, layerMask))
        {
            
            if (Mathf.Abs(angleToTarget) < (angleThreshold)) // adjust angle threshold as needed
            {
                // angle is small enough to fire rocket
                // use rocket
                scr_ItemHandler.UseItemRocket();

                Debug.Log("Rocket used by " + gameObject.name + " on target " + hitInfo.collider.name + " at close range.");

                Debug.DrawRay(origin, direction * castRange, Color.green);

                // draw box cast for debugging
                DrawBoxCast(origin, boxHalfExtents, transform.rotation, transform.forward, castRange, Color.green);

            }
            else 
            {
                Debug.DrawRay(origin, directionToTarget * castRange, Color.cyan);

                DrawBoxCast(origin, boxHalfExtents, transform.rotation, transform.forward, castRange, Color.cyan);

            }

            
        }
        else
        {
            // angle too large, dont fire
            Debug.DrawRay(origin, direction * castRange, Color.magenta);

            // draw box cast for debugging
            DrawBoxCast(origin, boxHalfExtents, transform.rotation, transform.forward, castRange, Color.magenta);


        }


    }

    // calculate rocket intercept time function
    private float FindStraightLineInterceptTime(Vector3 rocketPos, Vector3 forward,
        Vector3 targetPos, Vector3 targetVel, Vector3 targetAcc,
            float v0, float accel, float vmax)
    {
        const int steps = 250;     // search resolution
        const float dt = 0.25f;    // search step
        const float lateralTolerance = 2f; // how close target must be to forward line

        for (int i = 1; i < steps; i++)
        {
            float t = i * dt;

            // Rocket forward distance traveled at time t (accelerating)
            float rocketSpeed = Mathf.Min(v0 + accel * t, vmax);
            float dist;
            float tToMax = (vmax - v0) / accel;

            if (t <= tToMax)
            {
                dist = v0 * t + 0.5f * accel * t * t;
            }
            else
            {
                float accelDist = v0 * tToMax + 0.5f * accel * tToMax * tToMax;
                dist = accelDist + vmax * (t - tToMax);
            }

            Vector3 rocketPoint = rocketPos + forward * dist;

            // Target at time t
            Vector3 targetFuture =
                targetPos +
                targetVel * t +
                0.5f * targetAcc * t * t;

            // Compute lateral distance from rocket's forward line
            Vector3 toTarget = targetFuture - rocketPos;
            float lateral = Vector3.Magnitude(toTarget - Vector3.Project(toTarget, forward));

            // If lateral distance small enough → hit possible
            if (lateral < lateralTolerance)
                return t;
        }

        return -1f; // no intercept solution
    }

    // debug gizmos to visualize rocket firing solution
    private void OnDrawGizmos()
    {
        if (!debugHasSolution)
            return;

        // --------------------------
        // 1. Draw predicted intercept point
        // --------------------------
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(debugInterceptPoint, 0.5f);

        // Cross marker
        float c = 1f;
        Gizmos.DrawLine(debugInterceptPoint + Vector3.right * c, debugInterceptPoint - Vector3.right * c);
        Gizmos.DrawLine(debugInterceptPoint + Vector3.up * c, debugInterceptPoint - Vector3.up * c);
        Gizmos.DrawLine(debugInterceptPoint + Vector3.forward * c, debugInterceptPoint - Vector3.forward * c);


        // --------------------------
        // 2. Draw target predicted motion arc
        // --------------------------
        Gizmos.color = Color.green;
        for (int i = 0; i < debugTargetArc.Count - 1; i++)
        {
            Gizmos.DrawLine(debugTargetArc[i], debugTargetArc[i + 1]);
        }


        // --------------------------
        // 3. Draw rocket predicted forward path
        // --------------------------
        Gizmos.color = Color.red;
        for (int i = 0; i < debugRocketPath.Count - 1; i++)
        {
            Gizmos.DrawLine(debugRocketPath[i], debugRocketPath[i + 1]);
        }
    }

}
