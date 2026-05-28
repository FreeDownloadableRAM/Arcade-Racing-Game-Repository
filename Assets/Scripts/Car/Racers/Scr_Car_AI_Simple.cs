
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class CarAISimple : MonoBehaviour
{


    // move onto next vertex on path distance threshold
    [SerializeField] private float NextVertexDistanceThreshold; // distance to move to next vertex on path

    [SerializeField] private Vector3 raycastOffsetFromGround; // Offset for the raycast to come from origin of the car
    [SerializeField] private Rigidbody rigidBody; // Rigidbody of the car, to get velocity    

    // movement target

    //[SerializeField] private Transform targetPositionTransform;
    private Scr_Car_Pathfinder scrCarPathfinder;

    // reference braking trigger box script
    private Scr_BrakeZone_Hint_Properties brakeZoneHintProperties; // Reference to the brake zone hint properties script

    // car information

    private Vector3 carOrigin; // Origin of the car for raycasting
    [SerializeField] private Vector3 raycastCarWidth; // car width

    private CarControllerAI carControllerAI;
    [SerializeField] private Vector3 targetPosition; // Target position for AI to follow, can be set externally

    // track health, if zero, do not make this ai do anything
    private Scr_Car_Health scrCarHealth; // Reference to the car health script

    // Raycast to not get stuck on a wall
    Ray rayForward; // Rays for avoiding obstacles
    Ray rayForward_r;
    Ray rayForward_l;
    Ray rayForward_r_angled; // Ray for forward right (15 degrees)
    Ray rayForward_l_angled; // Ray for forward left (15 degrees)
    Ray rayForward_rf_angled; // Ray for forward right (30 degrees)
    Ray rayForward_lf_angled; // Ray for forward left (30 degrees)

    Ray rayForward_frm_angled; // Ray for forward right (7.5 degrees)
    Ray rayForward_flm_angled; // Ray for forward left (7.5 degrees)
    Ray rayForward_rfm_angled; // Ray for forward right (22.5 degrees)
    Ray rayForward_lfm_angled; // Ray for forward left (22.5 degrees)

    Ray rayBackward; // prevent reversing into a wall
    Ray rayBackward_r;
    Ray rayBackward_l;

    /*
    Ray raySteerAround; // Ray for steering around other Objects
    Ray raySteerAround_r; // right ray
    Ray raySteerAround_l; // left ray
    */

    // Ray Steer Around angle increments

    [SerializeField] private float raySteerAroundAngleIncrement = 2.5f; // Angle increment for the steer around rays, to check multiple angles for better obstacle avoidance when steering around objects

    // number of angled steering rays on each side, for better obstacle avoidance when steering around objects. Total rays will be this number times 2 (for left and right) plus the straight ray.
    
    [SerializeField] private int raySteerAroundNumberOfAngleIncrements = 24; // Number of angled rays on each side for steering around objects

    // create an array of angled steering rays        
    // we will create these rays in the update function based on the number of angle increments and the angle increment value,
    // to allow for more flexible configuration of the steer around obstacle avoidance system.
    // The rays will be created in a fan shape in front of the car, with the number of rays on each side determined by the raySteerAroundNumberOfAngleIncrements variable,
    // and the angle between each ray determined by the raySteerAroundAngleIncrement variable.
    // For example, if we have 3 angle increments and an angle increment value of 10 degrees,
    // we will have rays at -30, -20, -10, 0, 10, 20, and 30 degrees for steering around obstacles.

    private List<Ray> steerAroundRays = new List<Ray>(); // Create an ArrayList to hold the rays, since we don't know the exact number of rays yet

    private float currentAvoidanceSteer = 0f;

    // ray cast pointing towards the ground
    Ray rayDown; // if this collides with terrain layer objects, we are on offroad, not the road.

    // layer mask for offroad terrain detection
    private LayerMask offroadTerrainLayerMask; // Layer mask for offroad terrain detection

    // boolean for if we are offroad or not
    private bool isOffroad = false; // Are we currently offroad?

    // Dont re adjust steering when target angle is within this value
    [SerializeField] private float steeringAngleThreshold; // Angle threshold for steering adjustment

    // wall vision distance
    [SerializeField] private float obstacleVisionDistance; // Distance to check for obstacles in front of the car
    [SerializeField] private float obstacleBaseVisionDistance; // distance to check for obstacles
    [SerializeField] private float obstacleMinVisionDistance; // minimum distance to check for obstacles
    [SerializeField] private float obstacleMaxVisionDistance; // maximum distance to check for obstacles

    [SerializeField] private float obstacleReverseVisionDistance; // Distance to check for obstacles in front of the car
    [SerializeField] private float obstacleReverseBaseVisionDistance; // distance to check for obstacles
    [SerializeField] private float obstacleReverseMinVisionDistance; // minimum distance to check for obstacles
    [SerializeField] private float obstacleReverseMaxVisionDistance; // maximum distance to check for obstacles

    [SerializeField] private float steerVisionDistance; // Distance to check for obstacles in front of the car
    [SerializeField] private float steerBaseVisionDistance; // distance to check for obstacles
    [SerializeField] private float steerMinVisionDistance; // minimum distance to check for obstacles
    [SerializeField] private float steerMaxVisionDistance; // maximum distance to check for obstacles

    [SerializeField] private float steerAroundObjectSpeed; // Speed to steer around objects
    [SerializeField] private float sideSteerRayMultiplier; // Time to reverse when stuck
    //[SerializeField] private float angledRayLengthMultipliers; // shorten or lengthen the angled rays

    // angled rays - vectors
    private Vector3 carFrontSide_fr;
    private Vector3 carFrontSide_fl;
    private Vector3 carFrontSide_rf;
    private Vector3 carFrontSide_lf;

    // in between angled rays - vectors
    private Vector3 carFrontSide_frm; // 7.5 degrees
    private Vector3 carFrontSide_flm; // -7.5 degrees
    private Vector3 carFrontSide_rfm; // 22.5 degrees
    private Vector3 carFrontSide_lfm; // -22.5 degrees

    private Vector3 carBack;
    //private Vector3 carBack_r;
    //private Vector3 carBack_l;

    // angle
    [SerializeField] private float rayAngleForwardR; // Angle for forward right angled ray (15 degrees)
    [SerializeField] private float rayAngleForwardL; // Angle for forward left angled ray (-15 degrees)
    [SerializeField] private float rayAngleForwardRF; // Angle for forward right angled ray (30 degrees)
    [SerializeField] private float rayAngleForwardLF; // Angle for forward left angled ray (-30 degrees)

    // in between angles for more precise obstacle detection
    [SerializeField] private float rayAngleForwardFRM; // Angle for forward right angled ray (7.5 degrees)
    [SerializeField] private float rayAngleForwardFLM; // Angle for forward left angled ray (-7.5 degrees)
    [SerializeField] private float rayAngleForwardRFM; // Angle for forward right angled ray (22.5 degrees)
    [SerializeField] private float rayAngleForwardLFM; // Angle for forward left angled ray (-22.5 degrees)


    //[SerializeField] private float rayAngleBackward; // Angle for backward 
    //[SerializeField] private float rayAngleBackwardR; // Angle for backward right angled ray
    //[SerializeField] private float rayAngleBackwardL; // Angle for backward left angled ray

    float forwardAmount = 0f;
    float turnAmount = 0f;

    bool brakeInput = false; // No braking by default
    bool areWeInBrakeZone = false; // Are we in a brake zone?

    // random race finish steering variable
    float raceFinishedRandomSteer = 0f;

    // brake speed target modifier
    // > 1 means we are going to go faster than the set brake zone target speed
    // < 1 means we are going to go slower than the set brake zone target speed
    [SerializeField] private float brakeZoneTargetSpeedModifier = 1f; // Modifier for the brake zone target speed

    // AI Turn wheel speed
    //[SerializeField] float steerSmoothSpeed = 25f;

    // the actual steer input to the ai car controller 
    //private float steerAmount = 0f;

    // get race progress script component
    private scr_My_Race_Progress scrMyRaceProgress;

    private void Awake()
    {
        carControllerAI = GetComponent<CarControllerAI>();

        //aiStuckTimerReset = aiIsStuckCounter; // set these up once
        //aiRevTimeReset = aiHardReverseTime;

        raceFinishedRandomSteer = Random.Range(-1f, 1f); // generate random variable between -1 and 1 for steering when we finished the race

        scrCarPathfinder = GetComponent<Scr_Car_Pathfinder>(); // Get the target position from the Scr_Car_Pathfinder script

        scrMyRaceProgress = GetComponent<scr_My_Race_Progress>();

        scrCarHealth = GetComponent<Scr_Car_Health>(); // Reference to the car health script

        // get target for the car AI to follow
        targetPosition = scrCarPathfinder.carTarget; // Get the target position from the Scr_Car_Pathfinder script

        offroadTerrainLayerMask = LayerMask.GetMask("Terrain"); // Layer mask for offroad terrain detection

        
        // straight ray is already handled with raySteerAround, so we only need to create the angled rays here
        // create rays for the right side
        for (int i = 1; i <= raySteerAroundNumberOfAngleIncrements; i++)
        {
            float angle = raySteerAroundAngleIncrement * i; // Calculate the angle for this ray
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up); // Create a rotation for this angle
            Vector3 direction = rotation * transform.forward; // Rotate the forward vector by this angle to get the direction of the ray
            Ray ray = new Ray(carOrigin, direction); // Create a new ray with the car's origin and this direction
            steerAroundRays.Add(ray); // Add this ray to the ArrayList
            // also create rays for the left side by negating the angle
            float leftAngle = -angle; // Calculate the angle for the left ray by negating the right angle
            Quaternion leftRotation = Quaternion.AngleAxis(leftAngle, Vector3.up); // Create a rotation for this angle
            Vector3 leftDirection = leftRotation * transform.forward; // Rotate the forward vector by this angle to get the direction of the left ray
            Ray leftRay = new Ray(carOrigin, leftDirection); // Create a new ray with the car's origin and this direction
            steerAroundRays.Add(leftRay); // Add this ray to the ArrayList

            // debug draw the rays in the scene view
            Debug.DrawRay(carOrigin, direction * steerVisionDistance, Color.blue); // Debug draw the right ray in blue
        }


    }


    private void Update()
    {

        // Debug.Log("Target Position: " + targetPosition);
        // get target for the car AI to follow
        targetPosition = scrCarPathfinder.carTarget; // Get the target position from the Scr_Car_Pathfinder script

        //SetTargetPosition(targetPositionTransform.position);
        SetTargetPosition(targetPosition);

        forwardAmount = 0f;
        turnAmount = 0f;

        brakeInput = false; // No braking by default

        // set offroad flag to false by default, will be set to true if we detect offroad terrain below us
        isOffroad = false;

        // get our speed to scale detection rays
        float speed = Vector3.Magnitude(rigidBody.linearVelocity);

        // Did we reach the target position?
        float reachedTargetDistance = 7.5f; // Distance at which we consider the target reached

        // avoid obstacles
        carOrigin = transform.position + raycastOffsetFromGround; // Set the origin of the raycast

        obstacleVisionDistance = Mathf.Clamp(speed * obstacleBaseVisionDistance, obstacleMinVisionDistance, obstacleMaxVisionDistance); // Set the distance based on speed, between 10 and 50 units

        obstacleReverseVisionDistance = Mathf.Clamp(speed * obstacleReverseBaseVisionDistance, obstacleReverseMinVisionDistance, obstacleReverseMaxVisionDistance); // Set the distance based on speed, between 10 and 50 units

        steerVisionDistance = Mathf.Clamp(speed * steerBaseVisionDistance, steerMinVisionDistance, steerMaxVisionDistance); // Set the distance based on speed, between 10 and 50 units


        // determine distance to target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        float distanceToEndTarget = Vector3.Distance(transform.position, scrCarPathfinder.endtarget.position); // Distance to the end target

        // determine if target is in front or behind
        Vector3 dirToMovePosition = (targetPosition - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, dirToMovePosition); // if result is above zero, target is infront, if below, behind.


        // angled rays
        // Front angled rays
        // right
        Quaternion rotation_frontfrm = Quaternion.AngleAxis(rayAngleForwardFRM, Vector3.up);
        carFrontSide_frm = (rotation_frontfrm * transform.forward).normalized; // Front right direction 7.5

        Quaternion rotation_frontr = Quaternion.AngleAxis(rayAngleForwardR, Vector3.up);
        carFrontSide_fr = (rotation_frontr * transform.forward).normalized; // Front right direction 15

        Quaternion rotation_rightrfm = Quaternion.AngleAxis(rayAngleForwardRFM, Vector3.up);
        carFrontSide_rfm = (rotation_rightrfm * transform.forward).normalized; // Front right direction 22.5

        Quaternion rotation_rightf = Quaternion.AngleAxis(rayAngleForwardRF, Vector3.up);
        carFrontSide_rf = (rotation_rightf * transform.forward).normalized; // Front right direction 30

        // left
        Quaternion rotation_frontflm = Quaternion.AngleAxis(rayAngleForwardFLM, Vector3.up);
        carFrontSide_flm = (rotation_frontflm * transform.forward).normalized; // Front left direction 7.5

        Quaternion rotation_frontl = Quaternion.AngleAxis(rayAngleForwardL, Vector3.up);
        carFrontSide_fl = (rotation_frontl * transform.forward).normalized; // Front left direction 15

        Quaternion rotation_leftfml = Quaternion.AngleAxis(rayAngleForwardLFM, Vector3.up);
        carFrontSide_lfm = (rotation_leftfml * transform.forward).normalized; // Front left direction 22.5

        Quaternion rotation_leftf = Quaternion.AngleAxis(rayAngleForwardLF, Vector3.up);
        carFrontSide_lf = (rotation_leftf * transform.forward).normalized; // Front left direction 30

        // Backwards angled rays
        // directly behind
        //Quaternion rotation_backwards = Quaternion.AngleAxis(rayAngleBackward, Vector3.up);
        //carBack = (rotation_backwards * transform.forward).normalized; // Back straight direction

        //Quaternion rotation_backr = Quaternion.AngleAxis(rayAngleBackwardR, Vector3.up);
        //carBack_r = (rotation_backr * transform.forward).normalized; // Back right direction

        //Quaternion rotation_backl = Quaternion.AngleAxis(rayAngleBackwardL, Vector3.up);
        //carBack_l = (rotation_backl * transform.forward).normalized; // Back left direction

        Vector3 rotatedOffset = transform.rotation * raycastCarWidth; // Rotate the raycast offset to match the car's rotation

        /*
        // obstacle avoidance raycast
        rayForward = new Ray(carOrigin, transform.forward);
        rayForward_r = new Ray(carOrigin + raycastCarWidth, transform.forward);
        rayForward_l = new Ray(carOrigin - raycastCarWidth, transform.forward);
        rayForward_r_angled = new Ray(carOrigin, carFrontSide_fr);
        rayForward_l_angled = new Ray(carOrigin, carFrontSide_fl);
        rayForward_rf_angled = new Ray(carOrigin, carFrontSide_rf);
        rayForward_lf_angled = new Ray(carOrigin, carFrontSide_lf);

        // in between angled rays for better obstacle detection
        rayForward_frm_angled = new Ray(carOrigin, carFrontSide_frm); // Ray for forward right (7.5 degrees)
        rayForward_flm_angled = new Ray(carOrigin, carFrontSide_flm); // Ray for forward left (7.5 degrees)
        rayForward_rfm_angled = new Ray(carOrigin, carFrontSide_rfm); // Ray for forward right (22.5 degrees)
        rayForward_lfm_angled = new Ray(carOrigin, carFrontSide_lfm); // Ray for forward left (22.5 degrees)

        rayBackward = new Ray(carOrigin, -transform.forward);
        rayBackward_r = new Ray(carOrigin + rotatedOffset, -transform.forward);
        rayBackward_l = new Ray(carOrigin - rotatedOffset, -transform.forward);

        // steer around other cars raycast
        raySteerAround = new Ray(carOrigin, transform.forward);
        raySteerAround_r = new Ray(carOrigin + raycastCarWidth, transform.forward);
        raySteerAround_l = new Ray(carOrigin - raycastCarWidth, transform.forward);

        // update the steer around rays in the arraylist based on the current car origin and rotation,
        // since these rays are created in the awake function and need to be updated each frame to match
        // the car's current position and rotation for accurate obstacle detection when steering around objects.
        */

        // obstacle avoidance raycast
        rayForward = new Ray(carOrigin, transform.forward);

        rayForward_r = new Ray(
            carOrigin + raycastCarWidth,
            transform.forward
        );

        rayForward_l = new Ray(
            carOrigin - raycastCarWidth,
            transform.forward
        );

        rayForward_r_angled = new Ray(
            carOrigin,
            carFrontSide_fr
        );

        rayForward_l_angled = new Ray(
            carOrigin,
            carFrontSide_fl
        );

        rayForward_rf_angled = new Ray(
            carOrigin,
            carFrontSide_rf
        );

        rayForward_lf_angled = new Ray(
            carOrigin,
            carFrontSide_lf
        );

        // in-between angled rays
        rayForward_frm_angled = new Ray(
            carOrigin,
            carFrontSide_frm
        );

        rayForward_flm_angled = new Ray(
            carOrigin,
            carFrontSide_flm
        );

        rayForward_rfm_angled = new Ray(
            carOrigin,
            carFrontSide_rfm
        );

        rayForward_lfm_angled = new Ray(
            carOrigin,
            carFrontSide_lfm
        );

        // backward rays
        rayBackward = new Ray(
            carOrigin,
            -transform.forward
        );

        rayBackward_r = new Ray(
            carOrigin + rotatedOffset,
            -transform.forward
        );

        rayBackward_l = new Ray(
            carOrigin - rotatedOffset,
            -transform.forward
        );


        for (int i = 0; i < steerAroundRays.Count; i++)
        {
            float angle = raySteerAroundAngleIncrement * ((i / 2) + 1); // Calculate the angle for this ray based on its index in the list
            if (i % 2 == 0) // Even index, right side
            {
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up); // Create a rotation for this angle
                Vector3 direction = rotation * transform.forward; // Rotate the forward vector by this angle to get the direction of the ray
                steerAroundRays[i] = new Ray(carOrigin, direction); // Update the ray with the car's origin and this direction
            }
            else // Odd index, left side
            {
                Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.up); // Create a rotation for this angle (negate for left side)
                Vector3 direction = rotation * transform.forward; // Rotate the forward vector by this angle to get the direction of the ray
                steerAroundRays[i] = new Ray(carOrigin, direction); // Update the ray with the car's origin and this direction
            }

            // draw all the steer around rays in the scene view in a thin cyan line to visualize the area that the car is checking for obstacles to steer around
            //Debug.DrawRay(carOrigin, ((Ray)steerAroundRays[i]).direction * steerVisionDistance, Color.cyan); // Debug draw the ray in cyan
        }

        // offroad terrain detection raycast
        rayDown = new Ray(carOrigin, -transform.up);

        // if we finished race, do not run any logic
        if (scrMyRaceProgress.completedRace == true)
        {
            // stop the car
            carControllerAI.SetInputs(0f, raceFinishedRandomSteer, true); // full brake
            return; // exit the update function
        }

        // if we have no health, do not run any logic
        if (scrCarHealth.GetCurrentHealth() <= 0)
        {
            // stop the car
            carControllerAI.SetInputs(0f, 0f, true); // full brake
            return; // exit the update function
        }

        // if we are close enough to current target, swap to end target
        if (distanceToTarget < NextVertexDistanceThreshold)
        {
            // set target position to the next vetex on the path
            targetPosition = scrCarPathfinder.GetNextPathVertexPosition();

            //targetPosition = scrCarPathfinder.endtarget.position; // Swap to the end target

        }
        

        // if target is too far, go to it
        if (distanceToEndTarget > reachedTargetDistance)
        {

            // avoid only these layers
            LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacles"); // Layer mask to filter obstacles
            LayerMask steerAroundLayerMask = LayerMask.GetMask("Cars", "Cops"); // Layer mask to filter steering around objects

            // just release by default
            brakeInput = false; // release brake

            // go through each ray in the angled steering ray arraylist and check for obstacles,
            // starting with the rays with the smallest angle increment for better obstacle avoidance when steering around objects,
            // then move to the straight ray if no angled rays detect anything, to allow the car to steer around objects more smoothly instead
            // of always trying to steer around at the last second with the straight ray when it detects an object, which can cause jittery steering behavior.

            // ======================================================
            // SMART STEER-AROUND OBSTACLE DETECTION
            // ======================================================

            RaycastHit bestHit = new RaycastHit();

            bool foundObstacle = false;

            float bestScore = Mathf.Infinity;

            Ray bestRay = new Ray();

            for (int i = 0; i < steerAroundRays.Count; i++)
            {
                Ray ray = steerAroundRays[i];

                if (Physics.Raycast(ray, out RaycastHit hit, steerVisionDistance, steerAroundLayerMask))
                {
                    // -----------------------------
                    // DISTANCE SCORE
                    // -----------------------------
                    float distanceScore = hit.distance;

                    // -----------------------------
                    // ANGLE SCORE
                    // Smaller angles = more important
                    // -----------------------------
                    float angleFromForward =
                        Vector3.Angle(transform.forward, ray.direction);

                    // Weight angle importance
                    // Tune this value
                    float angleWeight = 0.35f;

                    float angleScore = angleFromForward * angleWeight;

                    // -----------------------------
                    // FINAL SCORE
                    // Lower = more dangerous
                    // -----------------------------
                    float totalScore = distanceScore + angleScore;

                    // Debug rays
                    //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.darkOrange);

                    // -----------------------------
                    // BEST OBSTACLE?
                    // -----------------------------
                    if (totalScore < bestScore)
                    {
                        bestScore = totalScore;

                        bestHit = hit;

                        bestRay = ray;

                        foundObstacle = true;
                    }
                }
            }

            

            /*
            if (Physics.Raycast(raySteerAround_r, out RaycastHit hitSteerAround_r, steerVisionDistance * sideSteerRayMultiplier, steerAroundLayerMask))
            {
                steerAroundObject(speed, hitSteerAround_r, distanceToEndTarget, dirToMovePosition, dotProduct, rotatedOffset, steerVisionDistance * sideSteerRayMultiplier);
            }
            else if (Physics.Raycast(raySteerAround_l, out RaycastHit hitSteerAround_l, steerVisionDistance * sideSteerRayMultiplier, steerAroundLayerMask))
            {
                steerAroundObject(speed, hitSteerAround_l, distanceToEndTarget, dirToMovePosition, dotProduct, rotatedOffset, steerVisionDistance * sideSteerRayMultiplier);
            }
            else if (Physics.Raycast(raySteerAround, out RaycastHit hitSteerAround, steerVisionDistance, steerAroundLayerMask))
            {
                steerAroundObject(speed, hitSteerAround, distanceToEndTarget, dirToMovePosition, dotProduct, rotatedOffset, steerVisionDistance);
            }
            */

            // 30 degree rays
            if (Physics.Raycast(rayForward_rf_angled, out RaycastHit hit_rf_angled, obstacleVisionDistance * 0.5f, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_rf_angled);
            }
            else if (Physics.Raycast(rayForward_lf_angled, out RaycastHit hit_lf_angled, obstacleVisionDistance * 0.5f, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_lf_angled);
            }
            // 22.5 degrees rays
            else if (Physics.Raycast(rayForward_rfm_angled, out RaycastHit hit_rfm_angled, obstacleVisionDistance * 0.75f, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_rfm_angled);
            }
            else if (Physics.Raycast(rayForward_lfm_angled, out RaycastHit hit_lfm_angled, obstacleVisionDistance * 0.75f, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_lfm_angled);
            }
            // 15 degree rays
            else if (Physics.Raycast(rayForward_r_angled, out RaycastHit hit_r_angled, obstacleVisionDistance * 0.75f, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_r_angled);
            }
            else if (Physics.Raycast(rayForward_l_angled, out RaycastHit hit_l_angled, obstacleVisionDistance * 0.75f, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_l_angled);
            }
            // 7.5 degree rays
            else if (Physics.Raycast(rayForward_frm_angled, out RaycastHit hit_frm_angled, obstacleVisionDistance, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_frm_angled);
            }
            else if (Physics.Raycast(rayForward_flm_angled, out RaycastHit hit_flm_angled, obstacleVisionDistance, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_flm_angled);
            }
            else if (Physics.Raycast(rayForward_r, out RaycastHit hit_r, obstacleVisionDistance, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_r);
            }
            else if (Physics.Raycast(rayForward_l, out RaycastHit hit_l, obstacleVisionDistance, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit_l);
            }
            else if (Physics.Raycast(rayForward, out RaycastHit hit, obstacleVisionDistance, obstacleLayerMask))
            {
                forwardObstacleAvoidance(speed, hit);

            }

            // ======================================================
            // USE BEST OBSTACLE
            // ======================================================

            else if (foundObstacle)
            {
                steerAroundObject(
                    speed,
                    bestHit,
                    distanceToEndTarget,
                    dirToMovePosition,
                    dotProduct,
                    rotatedOffset,
                    steerVisionDistance
                );

                // Draw selected steering ray
                Debug.DrawRay(
                    bestRay.origin,
                    bestRay.direction * bestHit.distance,
                    Color.yellow
                );


            }

            else if (Physics.Raycast(rayBackward, out RaycastHit back_hit, obstacleReverseVisionDistance, obstacleLayerMask))
            {
                backwardObstacleAvoidance(speed, back_hit, dirToMovePosition);
            }
            else if (Physics.Raycast(rayBackward_r, out RaycastHit back_hit_r, obstacleReverseVisionDistance, obstacleLayerMask))
            {
                backwardObstacleAvoidance(speed, back_hit_r, dirToMovePosition);
            }
            else if (Physics.Raycast(rayBackward_l, out RaycastHit back_hit_l, obstacleReverseVisionDistance, obstacleLayerMask))
            {
                backwardObstacleAvoidance(speed, back_hit_l, dirToMovePosition);
            }
            else
            {
                // standard steering behaviour when no obstacles are detected
                standardSteeringBehaviour(dotProduct, distanceToEndTarget, dirToMovePosition, rotatedOffset);
            }
            // if we are not in a brake zone, send the inputs to the car controller AI
            if (areWeInBrakeZone == false)
            {
                brakeInput = false; // reset brake

                // Send this movement information to the car controller AI
                carControllerAI.SetInputs(forwardAmount, turnAmount, brakeInput);
            }
            else
            {
                float targetSpeed; // Default target speed

                brakeInput = false; // reset brake

                // In break zone
                // get the speed target to slow down to
                targetSpeed = brakeZoneHintProperties.targetSpeed * brakeZoneTargetSpeedModifier; // Get the target speed to brake towards

                // brake until we reach the target speed
                if (targetSpeed < Vector3.Magnitude(rigidBody.linearVelocity))
                {
                    brakeInput = true; // brake
                    forwardAmount = 0f; // release gas
                    //Debug.Log("Braking in Brake Zone, target speed: " + targetSpeed + ", current speed: " + Vector3.Magnitude(rigidBody.linearVelocity));
                }
                else
                {
                    brakeInput = false; // release brake
                    //forwardAmount = 1f; // move forward
                }



            }


        }
        else
        {
            // Reached the target position, stop moving
            forwardAmount = 0f; // No forward movement
            turnAmount = 0f; // No steering needed

            brakeInput = true; // Apply brake

        }


        //steerAmount = Mathf.Lerp(steerAmount, turnAmount, steerSmoothSpeed * Time.deltaTime);

        // check if we are offroad with downwards raycast
        if (Physics.Raycast(rayDown, out RaycastHit hitDown, 5f, offroadTerrainLayerMask))
        {
            // we are offroad, set target speed to 30
            // + 10% of max speed to not slow down too much
            float targetSpeed; // Default target speed

            // max speed referenced from car controller ai
            float maxSpeed = carControllerAI.getMaxSpeed();

            brakeInput = false; // reset brake

            // In break zone
            // get the speed target to slow down to
            targetSpeed = 25 + (maxSpeed * 0.1f); // Get the target speed to brake towards

            // brake until we reach the target speed
            if (targetSpeed < Vector3.Magnitude(rigidBody.linearVelocity))
            {
                brakeInput = true; // brake
                forwardAmount = 0f; // release gas
                                    //Debug.Log("Braking in Brake Zone, target speed: " + targetSpeed + ", current speed: " + Vector3.Magnitude(rigidBody.linearVelocity));
            }
            else
            {
                brakeInput = false; // release brake
                                    //forwardAmount = 1f; // move forward
            }

            // set offroad flag to true
            isOffroad = true;

        }

        // debug log, show forward amount, turn amount, and brake input for tuning purposes

        // Debug.Log(transform.name + " FINAL Forward Amount: " + forwardAmount + ", Turn Amount: " + turnAmount + ", Brake Input: " + brakeInput);

        // Send this movement information to the car controller AI
        carControllerAI.SetInputs(forwardAmount, turnAmount, brakeInput);
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        // Additional logic can be added to adjust steering based on target position
    }

    // Handle breaking for trigger zones
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("BrakeZone"))
        {

            brakeZoneHintProperties = other.GetComponent<Scr_BrakeZone_Hint_Properties>(); // Get the brake zone hint properties script

            areWeInBrakeZone = true; // set brake zone flag


        }


    }

    // Handle breaking for trigger zones
    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("BrakeZone"))
        {

            brakeZoneHintProperties = other.GetComponent<Scr_BrakeZone_Hint_Properties>(); // Get the brake zone hint properties script

            areWeInBrakeZone = false; // set brake zone flag


        }


    }

    // OLD STEER AROUND BEHAVIOUR
    
    // steer around behaviour function
    private void steerAroundObject(float speed, RaycastHit hitSteerAround, float distanceToEndTarget, Vector3 dirToMovePosition, float dotProduct, Vector3 rotatedOffset, float steerVisionDistance) 
    {

        // get the transform of the object we are steering around
        Transform steerObstacleTransform = hitSteerAround.transform;

        // make sure we have a valid transform
        if (steerObstacleTransform != null) 
        {
            // create a direction to the obstacle
            Vector3 dirToSteerObject = (hitSteerAround.point - carOrigin).normalized;

            // debug, show the direction of the obstacle we are steering around
            // Debug.DrawRay(carOrigin, dirToSteerObject * steerVisionDistance, Color.orange);

            // get the direction of the movement target
            // Vector3 dirToMovementTarget = (targetPosition - carOrigin).normalized;
            Vector3 dirToMovementTarget = (new Vector3 (targetPosition.x, carOrigin.y, targetPosition.z) - carOrigin).normalized;


            // debug, show the direction to the target position
            // Debug.DrawRay(carOrigin, dirToMovementTarget * steerVisionDistance, Color.cyan);

            // check if the angle between the car and the target is too large
            float angleToTarget = Vector3.SignedAngle(transform.forward, dirToMovementTarget, Vector3.up);
            // if the angle is too big between the direction of the steering object and the target, just steer towards the target

            // get the vector to the end target
            // Vector3 dirToEndTarget = (scrCarPathfinder.endtarget.position - carOrigin).normalized;
            Vector3 dirToEndTarget = (new Vector3(scrCarPathfinder.endtarget.position.x, carOrigin.y, scrCarPathfinder.endtarget.position.z) - carOrigin).normalized;


            // debug, show the direction to the end target
            // Debug.DrawRay(carOrigin, dirToEndTarget * steerVisionDistance, Color.yellow);

            // Mirror direction around car's forward axis 
            Vector3 localDirToObstacle = transform.InverseTransformDirection(dirToSteerObject);

            // mirror left/right around forward
            localDirToObstacle.x *= -1f;

            // set the steering target position that we are going to steer towards
            Vector3 steerTargetPosition = transform.TransformDirection(localDirToObstacle).normalized;

            // Determine steering direction
            float angleToSteer = Vector3.SignedAngle(transform.forward, steerTargetPosition, Vector3.up);

            // try not to overtake on turns -------------------------
            float angleBetweenSteerAwayDirectionAndEndTarget = Vector3.SignedAngle(steerTargetPosition, dirToEndTarget, Vector3.up);

            // get angle between this cars forward direction and the end target vector
            float angleBetweenCarFrontToEndTarget = Vector3.SignedAngle(transform.forward, dirToEndTarget, Vector3.up);

            

            // if we are going slow, you can overtake no matter what, but no overtaking while reversing
            if (speed < ((carControllerAI.getMaxSpeed() * 0.2f) + 20.0f) && forwardAmount >= 0)
            {
                // if the angle is too big between where we want to steer to and the end target, dont steer away, just follow race path
                if (Mathf.Abs(angleBetweenSteerAwayDirectionAndEndTarget) > 25f)
                {
                    
                    // execute standard steering behaviour
                    standardSteeringBehaviour(dotProduct, distanceToEndTarget, dirToMovePosition, rotatedOffset);

                    // debug, show the direction we are steering towards to avoid the object
                    Debug.DrawRay(carOrigin, dirToMovementTarget * steerVisionDistance, Color.black);

                    // direction we are steering towards
                    Debug.DrawRay(carOrigin, steerTargetPosition * steerVisionDistance, Color.hotPink);

                    return; // exit the function

                }

                //standardSteeringBehaviour(dotProduct, distanceToEndTarget, steerTargetPosition, rotatedOffset);

                // steer away from obstacle
                turnAmount = Mathf.Clamp((angleToSteer / 70f), -1f, 1f);

                // debug log the angle to the steering target and the angle between the steering target and the end target for tuning purposes
                // Debug.Log(transform.name + " Angle to steer target: " + angleToSteer + " Turn amount: " + turnAmount);


                Debug.DrawRay(carOrigin, dirToMovementTarget * steerVisionDistance, Color.white);

                // direction we are steering towards
                Debug.DrawRay(carOrigin, steerTargetPosition * steerVisionDistance, Color.green);

            }
            else 
            {
                // if the angle is too big between where we want to steer to and the end target, dont steer away, just follow race path
                if (Mathf.Abs(angleBetweenSteerAwayDirectionAndEndTarget) > 15f)
                {
                    // steer towards the target
                    //turnAmount = Mathf.Clamp((angleToTarget / 70f), -1f, 1f);

                    // execute standard steering behaviour
                    standardSteeringBehaviour(dotProduct, distanceToEndTarget, dirToMovePosition, rotatedOffset);

                    // debug, show the direction we are steering towards to avoid the object
                    // Debug.DrawRay(carOrigin, dirToMovementTarget * steerVisionDistance, Color.black);

                    return; // exit the function

                }

                // are we approaching a turn? do not overtake on turns
                if (Mathf.Abs(angleBetweenCarFrontToEndTarget) > 15f)
                {
                    // execute standard steering behaviour
                    standardSteeringBehaviour(dotProduct, distanceToEndTarget, dirToMovePosition, rotatedOffset);

                    // debug, show the direction we are steering towards to avoid the object
                    // Debug.DrawRay(carOrigin, dirToMovementTarget * steerVisionDistance, Color.purple);

                    return; // exit the function
                }

                // try not to overtake if we would turn too steeply away from our movement target direction
                float angleBetweenSteerObjectAndTarget = Vector3.SignedAngle(dirToSteerObject, dirToMovementTarget, Vector3.up);
                if (Mathf.Abs(angleBetweenSteerObjectAndTarget) > 7.5f)
                {

                    // execute standard steering behaviour
                    standardSteeringBehaviour(dotProduct, distanceToEndTarget, dirToMovePosition, rotatedOffset);

                    // debug, show the direction we are steering towards to avoid the object
                    // Debug.DrawRay(carOrigin, dirToMovementTarget * steerVisionDistance, Color.blue);

                    return; // exit the function
                }

            }

            // steer towards the target
            // turnAmount = Mathf.Clamp((angleToSteer / 70f), -1f, 1f);

            

        }

    }
  

    // standard steering behaviour function
    private void standardSteeringBehaviour(float dotProduct, float distanceToTarget, Vector3 dirToMovePosition, Vector3 rotatedOffset) 
    {
        // obstacle is further than the target, no need to avoid
        // determine forward and backwards movement

        if (dotProduct > 0)
        {
            float brakeDistance = 15f; // distance at which we start braking

            // brake once we are close to the target
            if (distanceToTarget < brakeDistance)
            {
                forwardAmount = 0.25f; // no forward movement
                brakeInput = false; // apply brake
            }
            else
            {
                forwardAmount = 1f; // move forward
                brakeInput = false; // no braking
            }

        }
        else
        {
            float reverseDistance = 15f; // distance at which we start reversing

            // determine if we are too far to reverse
            if (distanceToTarget > reverseDistance)
            {
                forwardAmount = 1f; // move forward
            }
            else
            {

                forwardAmount = -1f; // move backward

            }



        }

        // Determine steering direction
        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);


        if (angleToDir == 0 || (angleToDir < steeringAngleThreshold && angleToDir > -steeringAngleThreshold))
        {
            // check for obstacles
            turnAmount = 0f; // no steering needed

        }
        else if ((angleToDir < 180 && angleToDir > (180 - steeringAngleThreshold)) && forwardAmount < 0f)
        {
            turnAmount = 0; // turn left; // no steering needed

        }
        else if ((angleToDir < -(180 + steeringAngleThreshold) && angleToDir > -180) && forwardAmount < 0f)
        {
            turnAmount = 0; // turn left; // no steering needed
        }
        else
        {
            // if we are reversed, flip steering direction
            if (forwardAmount < 0f)
            {
                // reverse steering direction
                turnAmount = Mathf.Clamp((angleToDir / 70f) * (-1), -1f, 1f);
            }
            else
            {
                // steer towards the target
                turnAmount = Mathf.Clamp((angleToDir / 70f), -1f, 1f);
            }

        }

        // Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.green);
        // Debug.DrawRay(carOrigin + rotatedOffset, transform.forward * obstacleVisionDistance, Color.lightGreen);
        // Debug.DrawRay(carOrigin - rotatedOffset, transform.forward * obstacleVisionDistance, Color.lightGreen);

        // Debug.DrawRay(carOrigin, -transform.forward * obstacleReverseVisionDistance, Color.pink);
        // Debug.DrawRay(carOrigin + rotatedOffset, -transform.forward * obstacleReverseVisionDistance, Color.purple);
        // Debug.DrawRay(carOrigin - rotatedOffset, -transform.forward * obstacleReverseVisionDistance, Color.purple);

    }
    
    /*
    // ======================================================
    // IMPROVED STEER AROUND BEHAVIOUR
    // ======================================================

    private void steerAroundObject(float speed, RaycastHit hitSteerAround, float distanceToEndTarget, Vector3 dirToMovePosition, float dotProduct, 
       Vector3 rotatedOffset,float steerVisionDistance)
    {
        Transform steerObstacleTransform = hitSteerAround.transform;

        if (steerObstacleTransform == null)
            return;

        // ======================================================
        // DIRECTIONS
        // ======================================================

        Vector3 dirToObstacle =
            (hitSteerAround.point - carOrigin).normalized;

        Vector3 dirToMovementTarget =
            (new Vector3(
                targetPosition.x,
                carOrigin.y,
                targetPosition.z
            ) - carOrigin).normalized;

        Vector3 dirToEndTarget =
            (new Vector3(
                scrCarPathfinder.endtarget.position.x,
                carOrigin.y,
                scrCarPathfinder.endtarget.position.z
            ) - carOrigin).normalized;

        // ======================================================
        // ANGLES
        // ======================================================

        float angleToObstacle =
            Vector3.SignedAngle(
                transform.forward,
                dirToObstacle,
                Vector3.up
            );

        float angleToEndTarget =
            Vector3.SignedAngle(
                transform.forward,
                dirToEndTarget,
                Vector3.up
            );

        // ======================================================
        // SHARP TURN CHECK
        // Avoid overtaking during sharp turns
        // ======================================================

        float noOvertakeTurnThreshold = 7.5f;

        if (Mathf.Abs(angleToEndTarget) > noOvertakeTurnThreshold)
        {
            standardSteeringBehaviour(
                dotProduct,
                distanceToEndTarget,
                dirToMovePosition,
                rotatedOffset
            );

            Debug.DrawRay(carOrigin, dirToMovePosition * obstacleVisionDistance, Color.black);

            return;
        }

        // ======================================================
        // DETERMINE OVERTAKE SIDE
        // ======================================================

        // Positive = turn right
        // Negative = turn left

        bool trackTurningRight = angleToEndTarget > 0f;

        // ======================================================
        // OUTSIDE OVERTAKE LOGIC
        // ======================================================

        // If track turns right:
        // overtake on LEFT (outside)

        // If track turns left:
        // overtake on RIGHT (outside)

        float desiredSteerDirection = 0f;

        if (trackTurningRight)
        {
            desiredSteerDirection = -1f;
        }
        else
        {
            desiredSteerDirection = 1f;
        }

        // ======================================================
        // OBSTACLE POSITION FACTOR
        // ======================================================

        float obstacleForwardFactor =
            1f - Mathf.Clamp01(
                Mathf.Abs(angleToObstacle) / 90f
            );

        // Directly ahead = 1
        // To side = 0

        // ======================================================
        // STEERING STRENGTH
        // More aggressive if obstacle directly ahead
        // ======================================================

        float minSteerStrength = 0.01f;
        float maxSteerStrength = 1.0f;

        float steerStrength =
            Mathf.Lerp(
                minSteerStrength,
                maxSteerStrength,
                obstacleForwardFactor
            );

        // ======================================================
        // DISTANCE FACTOR
        // ======================================================

        // 0 = far away
        // 1 = extremely close

        float distanceFactor =
            1f - Mathf.Clamp01(
                hitSteerAround.distance / steerVisionDistance
            );

        // ======================================================
        // NONLINEAR CURVE
        // Makes distant cars MUCH less influential
        // ======================================================

        distanceFactor = Mathf.Pow(distanceFactor, 2.5f);

        // ======================================================
        // APPLY DISTANCE TO STEERING
        // ======================================================

        steerStrength *= Mathf.Lerp(
            0.01f,   // almost no steering at long range
            1f,   // strong steering up close
            distanceFactor
        );

        // ======================================================
        // FINAL STEERING
        // ======================================================

        float targetSteer =
        Mathf.Clamp(
            desiredSteerDirection * steerStrength,
            -1f,
            1f
        );

        // Smooth steering response
        currentAvoidanceSteer =
        Mathf.Lerp(
            currentAvoidanceSteer,
            targetSteer,
            Time.deltaTime * 3.5f
        );

        turnAmount = currentAvoidanceSteer;

        // ======================================================
        // SPEED CONTROL
        // Slow slightly when directly behind another car
        // Prevents pushing cars off track
        // ======================================================

        bool directlyBehindCar =
            Mathf.Abs(angleToObstacle) < 4f;

        if (directlyBehindCar)
        {
            float slowDownFactor =
                Mathf.InverseLerp(
                    steerVisionDistance,
                    steerVisionDistance * 0.2f,
                    hitSteerAround.distance
                );

            forwardAmount =
                Mathf.Lerp(
                    1f,
                    0.6f,
                    slowDownFactor
                );
        }
        else
        {
            forwardAmount = 1f;
        }

        // ======================================================
        // EXTRA SLOWDOWN FOR VERY CLOSE OBSTACLES
        // ======================================================

        if (hitSteerAround.distance <
            steerVisionDistance * 0.1f)
        {
            forwardAmount *= 0.8f;
        }

        // ======================================================
        // DEBUG
        // ======================================================

        
        Debug.DrawRay(
            carOrigin,
            dirToObstacle * steerVisionDistance,
            Color.red
        );

        Debug.DrawRay(
            carOrigin,
            dirToEndTarget * steerVisionDistance,
            Color.green
        );
        
    }
    
    */


    
    // ======================================================
    // IMPROVED STANDARD STEERING
    // ======================================================
    
    /*
    private void standardSteeringBehaviour(float dotProduct, float distanceToTarget, Vector3 dirToMovePosition, Vector3 rotatedOffset)
    {
        // ======================================================
        // FORWARD / REVERSE
        // ======================================================

        if (dotProduct > 0)
        {
            float brakeDistance = 15f;

            if (distanceToTarget < brakeDistance)
            {
                forwardAmount = 0.25f;
                brakeInput = false;
            }
            else
            {
                forwardAmount = 1f;
                brakeInput = false;
            }
        }
        else
        {
            float reverseDistance = 15f;

            if (distanceToTarget > reverseDistance)
            {
                forwardAmount = 1f;
            }
            else
            {
                forwardAmount = -1f;
            }
        }

        // ======================================================
        // TARGET STEERING
        // ======================================================

        float angleToDir =
            Vector3.SignedAngle(
                transform.forward,
                dirToMovePosition,
                Vector3.up
            );

        // ======================================================
        // DYNAMIC STEERING RESPONSE
        // Sharper turns = more steering
        // ======================================================

        float absAngle = Mathf.Abs(angleToDir);

        float steeringSensitivity =
            Mathf.Lerp(
                55f,
                30f,
                Mathf.Clamp01(absAngle / 90f)
            );

        // ======================================================
        // DEADZONE
        // ======================================================

        if (absAngle < steeringAngleThreshold)
        {
            turnAmount = 0f;
        }
        else
        {
            float steering =
                Mathf.Clamp(
                    angleToDir / steeringSensitivity,
                    -1f,
                    1f
                );

            // Reverse steering correction
            if (forwardAmount < 0f)
            {
                steering *= -1f;
            }

            // ======================================================
            // SMOOTH STEERING CURVE
            // Less twitchy near center
            // ======================================================

            steering =
                Mathf.Sign(steering) *
                Mathf.Pow(Mathf.Abs(steering), 1.35f);

            turnAmount = steering;
        }

        // ======================================================
        // DEBUG
        // ======================================================

        //Debug.DrawRay(carOrigin,dirToMovePosition * obstacleVisionDistance,Color.white);
    }
    
    */
    // forward obstacle avoidance behaviour function
    private void forwardObstacleAvoidance(float speed, RaycastHit hitObstacleAvoid) 
    {
        // get angle between car and obstacle
        // get direction
        Vector3 dirToObstacle = (hitObstacleAvoid.point - carOrigin).normalized;

        // get angle
        float angleToObstacle = Vector3.SignedAngle(transform.forward, dirToObstacle, Vector3.up);

        // if we are faster than this, steer away while accelerating
        if (speed > 10f)
        {
            forwardAmount = 1f; // reverse
        }
        else if (speed <= 10f && speed > 7f)
        {
            forwardAmount = 0f; // ;et wheels spin, no gas
        }
        else
        {
            forwardAmount = -1f; // reverse
        }

        // move away from obstacle
        turnAmount = (Mathf.Clamp((angleToObstacle / 70f) * (-1f), -1f, 1f)); // turn away

        // Debug.DrawRay(carOrigin, dirToObstacle * obstacleVisionDistance, Color.red);
    }

    // backward obstacle avoidance behaviour function
    private void backwardObstacleAvoidance(float speed, RaycastHit hitObstacleAvoid, Vector3 dirToMovePosition) 
    {
        // get angle between car and obstacle
        // get direction
        Vector3 dirToObstacle = (targetPosition - hitObstacleAvoid.point).normalized;

        // get angle
        float angleToObstacle = Vector3.SignedAngle(transform.forward, dirToObstacle, Vector3.up);

        forwardAmount = 1f; // reverse

        // Determine steering direction
        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);


        if (angleToDir == 0 || (angleToDir < steeringAngleThreshold && angleToDir > -steeringAngleThreshold))
        {
            // check for obstacles
            turnAmount = 0f; // no steering needed

        }
        else if ((angleToDir < 180 && angleToDir > (180 - steeringAngleThreshold)) && forwardAmount < 0f)
        {
            turnAmount = 0; // turn left; // no steering needed

        }
        else if ((angleToDir < -(180 + steeringAngleThreshold) && angleToDir > -180) && forwardAmount < 0f)
        {
            turnAmount = 0; // turn left; // no steering needed
        }
        else
        {
            // if we are reversed, flip steering direction
            if (forwardAmount < 0f)
            {
                // reverse steering direction
                turnAmount = Mathf.Clamp((angleToDir / 70f) * (-1), -1f, 1f);
            }
            else
            {
                // steer towards the target
                turnAmount = Mathf.Clamp((angleToDir / 70f), -1f, 1f);
            }

        }

        // move away from obstacle
        // turnAmount = (Mathf.Clamp((angleToObstacle / 70f) * (-1f), -1f, 1f)); // turn away

        // Debug.DrawRay(carOrigin, -transform.forward * obstacleReverseVisionDistance, Color.red);


    }

    // draw gizmos
    private void OnDrawGizmos()
    {
        // DRAW GIZMO SPHERE on the car target position
        //Gizmos.DrawSphere(targetPosition, 0.5f);

        
    }

    // get car origin for item handler script and item behaviour scripts
    public Vector3 getCarOrigin() 
    {
        return carOrigin;
    }

    // return car height off ground offset
    public Vector3 getCarHeightOffGroundRaycastOffset() 
    {

        return raycastOffsetFromGround;
    }

    // return if we are offroad
    public bool isCarOffroad() 
    { 
        return isOffroad;

    }

}
