using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarAI : MonoBehaviour
{

    // ai obstacle detection distance
    private float obstacleVisionDistance; // distance to check for obstacles
    [SerializeField] private float obstacleBaseVisionDistance; // distance to check for obstacles
    [SerializeField] private float obstacleMinVisionDistance; // minimum distance to check for obstacles
    [SerializeField] private float obstacleMaxVisionDistance; // maximum distance to check for obstacles

    // move onto next vertex on path distance threshold
    [SerializeField] private float NextVertexDistanceThreshold; // distance to move to next vertex on path

    Ray ray; // forward
    Ray ray_fr; // forward right
    Ray ray_fl; // forward left
    Ray ray_sr; // side right 
    Ray ray_sl; // side left
    Ray ray_br; // backward right
    Ray ray_bl; // backward left
    Ray ray_back; // backward

    Ray ray_is_path_obstructed_r; // ray to check if path is obstructed right
    Ray ray_is_path_obstructed_l; // ray to check if path is obstructed left 

    // their respective angles

    [SerializeField] private float rayAngleForwardRight; // Angle for forward right ray
    [SerializeField] private float rayAngleForwardLeft; // Angle for forward left ray
    [SerializeField] private float rayAngleSideRight; // Angle for side right ray
    [SerializeField] private float rayAngleSideLeft; // Angle for side left ray
    [SerializeField] private float rayAngleBackwardRight; // Angle for backward right ray
    [SerializeField] private float rayAngleBackwardLeft; // Angle for backward left ray
    [SerializeField] private float rayAngleBackward; // Angle for backward 

    [SerializeField] private Vector3 raycastOffsetFromGround; // Offset for the raycast to come from origin of the car
    [SerializeField] private Vector3 raycastCarWidth; // car width
    [SerializeField] private Rigidbody rigidBody; // Rigidbody of the car, to get velocity    

    // movement target

    //[SerializeField] private Transform targetPositionTransform;
    private Vector3 targetPositionTransform;
    private Scr_Car_Pathfinder scrCarPathfinder;

    // car information

    private Vector3 carOrigin; // Origin of the car for raycasting
    private Vector3 carForwardRight; // Forward right direction of the car
    private Vector3 carForwardLeft; 
    private Vector3 carSideRight; // Side right direction of the car
    private Vector3 carSideLeft; // Side left direction of the car
    private Vector3 carBackRight; // Backward right direction of the car
    private Vector3 carBackLeft; // Backward left direction of the car
    private Vector3 carBack;

    // AI Steer Avoidence Properties
    [SerializeField] private float ray_fr_fl_steer_min; // Angle minimum angle we can steer while avoiding obstacle
    [SerializeField] private float ray_sr_sl_steer_min; // side
    [SerializeField] private float ray_br_bl_steer_min; // back


    private CarControllerAI carControllerAI;
    [SerializeField] private Vector3 targetPosition; // Target position for AI to follow, can be set externally
    
    

    private void Awake()
    {
        carControllerAI = GetComponent<CarControllerAI>();

        //aiStuckTimerReset = aiIsStuckCounter; // set these up once
        //aiRevTimeReset = aiHardReverseTime;

        scrCarPathfinder = GetComponent<Scr_Car_Pathfinder>(); // Get the target position from the Scr_Car_Pathfinder script
    }

    private void Start()
    {

        
    }

    private void Update()
    {
        // get target for the car AI to follow
        targetPosition = scrCarPathfinder.carTarget; // Get the target position from the Scr_Car_Pathfinder script
        //Debug.Log("Target Position: " + targetPosition);

        //SetTargetPosition(targetPositionTransform.position);
        SetTargetPosition(targetPosition);

        float forwardAmount = 0f; 
        float turnAmount = 0f;

        bool brakeInput = false; // No braking by default

        // get our speed to scale detection rays
        float speed = Vector3.Magnitude(rigidBody.linearVelocity);
        obstacleVisionDistance = Mathf.Clamp(speed * obstacleBaseVisionDistance, obstacleMinVisionDistance, obstacleMaxVisionDistance); // Set the distance based on speed, between 10 and 50 units

        // Did we reach the target position?
        float reachedTargetDistance = 7.5f; // Distance at which we consider the target reached

        // determine distance to target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        float distanceToEndTarget = Vector3.Distance(transform.position, scrCarPathfinder.endtarget.position); // Distance to the end target

        // determine if target is in front or behind
        Vector3 dirToMovePosition = (targetPosition - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, dirToMovePosition); // if result is above zero, target is infront, if below, behind.

        // if we are close enough to current target, swap to end target
        if (distanceToTarget < NextVertexDistanceThreshold)
        {
            targetPosition = scrCarPathfinder.endtarget.position; // Swap to the end target
           
        }

        // if target is too far, go to it
        if (distanceToEndTarget > reachedTargetDistance)
        {
            // reset stuck timer

            // avoid obstacles
            carOrigin = transform.position + raycastOffsetFromGround; // Set the origin of the raycast

            // forward right ray
            Quaternion rotation_fr = Quaternion.AngleAxis(rayAngleForwardRight, Vector3.up);
            carForwardRight = (rotation_fr * transform.forward).normalized; // Forward right direction

            // forward left ray
            Quaternion rotation_fl = Quaternion.AngleAxis(rayAngleForwardLeft, Vector3.up);
            carForwardLeft = (rotation_fl * transform.forward).normalized; // Forward left direction

            // side right ray
            Quaternion rotation_sr = Quaternion.AngleAxis(rayAngleSideRight, Vector3.up);
            carSideRight = (rotation_sr * transform.forward).normalized; // Side right direction

            // side left ray
            Quaternion rotation_sl = Quaternion.AngleAxis(rayAngleSideLeft, Vector3.up);
            carSideLeft = (rotation_sl * transform.forward).normalized; // Side left direction

            // back right ray
            Quaternion rotation_br = Quaternion.AngleAxis(rayAngleBackwardRight, Vector3.up);
            carBackRight = (rotation_br * transform.forward).normalized; // Back right direction

            // back left ray
            Quaternion rotation_bl = Quaternion.AngleAxis(rayAngleBackwardLeft, Vector3.up);
            carBackLeft = (rotation_bl * transform.forward).normalized; // Back left direction


            LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacles"); // Layer mask to filter obstacles


            // set up rays for raycasting
            ray = new Ray(carOrigin, transform.forward);
            ray_fr = new Ray(carOrigin, carForwardRight);
            ray_fl = new Ray(carOrigin, carForwardLeft);
            ray_sr = new Ray(carOrigin, carSideRight);
            ray_sl = new Ray(carOrigin, carSideLeft);
            ray_br = new Ray(carOrigin, carBackRight);
            ray_bl = new Ray(carOrigin, carBackLeft);
            ray_back = new Ray(carOrigin, -transform.forward);

            ray_is_path_obstructed_r = new Ray(carOrigin + raycastCarWidth, dirToMovePosition); // Ray to check if path is obstructed
            ray_is_path_obstructed_l = new Ray(carOrigin - raycastCarWidth, dirToMovePosition); // Ray to check if path is obstructed

            if ((Physics.Raycast(ray_is_path_obstructed_r, out RaycastHit hit_obst_r, distanceToTarget, obstacleLayerMask) || Physics.Raycast(ray_is_path_obstructed_l, out RaycastHit hit_obst_l, distanceToTarget, obstacleLayerMask)))
            {

                if (Physics.Raycast(ray, out RaycastHit hit, obstacleVisionDistance, obstacleLayerMask))
                {
                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle = Vector3.Distance(transform.position, hit.point);

                    if (distanceToObstacle < distanceToTarget)
                    {

                        // obstacle is closer than the target, avoid obstacle

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle = (targetPosition - hit.point).normalized;

                        // get angle
                        float angleToObstacle = Vector3.SignedAngle(transform.forward, dirToObstacle, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle <= 10f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle < 7.5f)
                            {
                                forwardAmount = -1f; // reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp((angleToObstacle / 70f) * (-1f), -1f, 1f)); // turn away

                            }
                            else
                            {
                                // if our angle is too shallow, we need to hard turn

                                if (Mathf.Abs(angleToObstacle) < 60f)
                                {   
                                    // are we facing to the left or right
                                    if (angleToObstacle > 0) 
                                    {
                                        // facing right
                                        forwardAmount = 0.5f; // less gas
                                        turnAmount = -1f; // turn left
                                    } 
                                    else 
                                    {
                                        // facing left
                                        forwardAmount = 0.5f; // less gas
                                        turnAmount = 1f; // turn right
                                    }

                                    
                                }
                                else 
                                {

                                    forwardAmount = 0.65f; // less gas



                                    turnAmount = (Mathf.Clamp((angleToObstacle / 70f) * (-1), -1f, 1f)); // turn away

                                }

                            }

                            Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.red);

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle / 70f) * (-1), -1f, 1f)); // turn away
                            forwardAmount = 1f; // full gas

                            Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.orange);

                        }

                        // debug line
                        //Debug.Log("Angle towards obstacle: " +  angleToObstacle + " Turn amount (+ is right, - is left): " + turnAmount + " forward amount (+ is forward, - is backward): " + forwardAmount);

                        

                    }

                    else
                    {
                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.yellow);



                    }

                    // Debug.Log("Angle: " + angleToDir + " Braking?: " + brakeInput);



                }
                // front right ray
                else if (Physics.Raycast(ray_fr, out RaycastHit hit_fr, obstacleVisionDistance, obstacleLayerMask))
                {

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle_fr = Vector3.Distance(transform.position, hit_fr.point);

                    if (distanceToObstacle_fr < distanceToTarget)
                    {

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle_fr = (targetPosition - hit_fr.point).normalized;

                        // get angle
                        float angleToObstacle_fr = Vector3.SignedAngle(transform.forward, dirToObstacle_fr, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle_fr < 15f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle_fr < 3.5f)
                            {
                                forwardAmount = -1f; // reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp(angleToObstacle_fr / 70f, -1f, 1f)); // turn away

                            }
                            else
                            {
                                forwardAmount = 0.5f; // less gas


                                turnAmount = (Mathf.Clamp((angleToObstacle_fr / 70f) * (-1), -1f, -ray_fr_fl_steer_min)); // turn away

                            }

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle_fr / 70f) * (-1), -1f, -ray_fr_fl_steer_min)); // turn away
                            forwardAmount = 1f; // full gas

                        }

                        // debug line
                        //Debug.Log("Angle towards obstacle FR: " + angleToObstacle_fr + " Turn amount (+ is right, - is left): " + turnAmount + " forward amount (+ is forward, - is backward): " + forwardAmount);


                        Debug.DrawRay(carOrigin, carForwardRight * obstacleVisionDistance, Color.red);


                    }
                    else
                    {

                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carForwardRight * obstacleVisionDistance, Color.deepPink);


                    }

                }

                // front left ray
                else if (Physics.Raycast(ray_fl, out RaycastHit hit_fl, obstacleVisionDistance, obstacleLayerMask))
                {

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle_fl = Vector3.Distance(transform.position, hit_fl.point);

                    if (distanceToObstacle_fl < distanceToTarget)
                    {

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle_fl = (targetPosition - hit_fl.point).normalized;

                        // get angle
                        float angleToObstacle_fl = Vector3.SignedAngle(transform.forward, dirToObstacle_fl, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle_fl < 15f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle_fl < 1.5f)
                            {
                                forwardAmount = -1f; // reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp(angleToObstacle_fl / 70f, -1f, 1f)); // turn away

                            }
                            else
                            {
                                forwardAmount = 0.5f; // less gas


                                turnAmount = (Mathf.Clamp((angleToObstacle_fl / 70f) * (-1f), ray_fr_fl_steer_min, 1f)); // turn away

                            }

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle_fl / 70f) * (-1f), ray_fr_fl_steer_min, 1f)); // turn away
                            forwardAmount = 1f; // full gas

                        }

                        Debug.DrawRay(carOrigin, carForwardLeft * obstacleVisionDistance, Color.red);


                    }
                    else
                    {

                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carForwardLeft * obstacleVisionDistance, Color.deepPink);


                    }

                }

                // Side right ray
                else if (Physics.Raycast(ray_sr, out RaycastHit hit_sr, obstacleVisionDistance, obstacleLayerMask))
                {

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle_sr = Vector3.Distance(transform.position, hit_sr.point);

                    if (distanceToObstacle_sr < distanceToTarget)
                    {

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle_sr = (targetPosition - hit_sr.point).normalized;

                        // get angle
                        float angleToObstacle_sr = Vector3.SignedAngle(transform.forward, dirToObstacle_sr, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle_sr < 10f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle_sr < 1.5f)
                            {
                                forwardAmount = 1f; // dont reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp(angleToObstacle_sr / 70f, -1f, 1f) * (-1f)); // turn away

                            }
                            else
                            {
                                forwardAmount = 0.5f; // less gas


                                turnAmount = (Mathf.Clamp((angleToObstacle_sr / 70f) * (-1f), -1f, -ray_sr_sl_steer_min)); // turn away

                            }

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle_sr / 70f) * (-1f), -1f, -ray_sr_sl_steer_min)); // turn away
                            forwardAmount = 1f; // full gas

                        }

                        Debug.DrawRay(carOrigin, carSideRight * obstacleVisionDistance, Color.red);

                        // debug line
                        //Debug.Log("Angle towards obstacle SR: " + angleToObstacle_sr + " Turn amount (+ is right, - is left): " + turnAmount + " forward amount (+ is forward, - is backward): " + forwardAmount);

                    }
                    else
                    {

                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carSideRight * obstacleVisionDistance, Color.deepPink);


                    }

                }

                // Side left ray
                else if (Physics.Raycast(ray_sl, out RaycastHit hit_sl, obstacleVisionDistance, obstacleLayerMask))
                {

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle_sl = Vector3.Distance(transform.position, hit_sl.point);

                    if (distanceToObstacle_sl < distanceToTarget)
                    {

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle_sl = (targetPosition - hit_sl.point).normalized;

                        // get angle
                        float angleToObstacle_sl = Vector3.SignedAngle(transform.forward, dirToObstacle_sl, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle_sl < 10f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle_sl < 1.5f)
                            {
                                forwardAmount = 1f; // dont reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp(angleToObstacle_sl / 70f, -1f, 1f) * (-1f)); // turn away

                            }
                            else
                            {
                                forwardAmount = 0.5f; // less gas


                                turnAmount = (Mathf.Clamp((angleToObstacle_sl / 70f) * (-1f), ray_sr_sl_steer_min, 1f)); // turn away

                            }

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle_sl / 70f) * (-1f), ray_sr_sl_steer_min, 1f)); // turn away
                            forwardAmount = 1f; // full gas

                        }

                        Debug.DrawRay(carOrigin, carSideLeft * obstacleVisionDistance, Color.red);


                    }
                    else
                    {

                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carSideLeft * obstacleVisionDistance, Color.deepPink);


                    }

                }

                // back right ray
                else if (Physics.Raycast(ray_br, out RaycastHit hit_br, obstacleVisionDistance, obstacleLayerMask))
                {

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle_br = Vector3.Distance(transform.position, hit_br.point);

                    if (distanceToObstacle_br < distanceToTarget)
                    {

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle_br = (targetPosition - hit_br.point).normalized;

                        // get angle
                        float angleToObstacle_br = Vector3.SignedAngle(transform.forward, dirToObstacle_br, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle_br < 5f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle_br < 0.5f)
                            {
                                forwardAmount = 1f; // dont reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp((angleToObstacle_br / 70f) * (-1f), -1f, 1f)); // turn away

                            }
                            else
                            {
                                forwardAmount = 0.75f; // less gas


                                turnAmount = (Mathf.Clamp((angleToObstacle_br / 70f) * (-1f), -1f, -ray_br_bl_steer_min)); // turn away

                            }

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle_br / 70f) * (-1f), -1f, -ray_br_bl_steer_min)); // turn away
                            forwardAmount = 1f; // full gas

                        }

                        Debug.DrawRay(carOrigin, carBackRight * obstacleVisionDistance, Color.red);


                    }
                    else
                    {

                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carBackRight * obstacleVisionDistance, Color.deepPink);


                    }

                }

                // back right ray
                else if (Physics.Raycast(ray_bl, out RaycastHit hit_bl, obstacleVisionDistance, obstacleLayerMask))
                {

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle_bl = Vector3.Distance(transform.position, hit_bl.point);

                    if (distanceToObstacle_bl < distanceToTarget)
                    {

                        // get angle between car and obstacle
                        // get direction
                        Vector3 dirToObstacle_bl = (targetPosition - hit_bl.point).normalized;

                        // get angle
                        float angleToObstacle_bl = Vector3.SignedAngle(transform.forward, dirToObstacle_bl, Vector3.up);

                        // forward and backward movement 
                        if (distanceToObstacle_bl < 5f)
                        {
                            // obstacle is too close, brake
                            if (distanceToObstacle_bl < 0.5f)
                            {
                                forwardAmount = 1f; // dont reverse

                                // move away from obstacle
                                turnAmount = (Mathf.Clamp((angleToObstacle_bl / 70f) * (-1f), -1f, 1f)); // turn away

                            }
                            else
                            {
                                forwardAmount = 0.75f; // less gas


                                turnAmount = (Mathf.Clamp((angleToObstacle_bl / 70f) * (-1f), ray_br_bl_steer_min, 1f)); // turn away

                            }

                        }
                        else
                        {
                            // move away from obstacle
                            turnAmount = (Mathf.Clamp((angleToObstacle_bl / 70f) * (-1f), ray_br_bl_steer_min, 1f)); // turn away
                            forwardAmount = 1f; // full gas

                        }

                        Debug.DrawRay(carOrigin, carBackLeft * obstacleVisionDistance, Color.red);


                    }
                    else
                    {

                        // obstacle is further than the target, no need to avoid

                        brakeInput = false; // release brake

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

                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn left
                        }

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carBackLeft * obstacleVisionDistance, Color.deepPink);


                    }

                }


                else
                {

                    // Determine steering direction

                    float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

             
                    // no wall in front of us
                    // proceed as normal
                    

                        brakeInput = false; // release brake

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


                        if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                        {
                            // check for obstacles
                            turnAmount = 0f; // no steering needed

                        }
                        else
                        {
                            // Target is to the left
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 1f); ; // turn 
                        }

                        // Debug.Log("Angle: " + angleToDir + " Braking?: " + brakeInput);

                        Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.white);

                        // other Ray lines
                        Debug.DrawRay(carOrigin, carForwardRight * obstacleVisionDistance, Color.pink);
                        Debug.DrawRay(carOrigin, carForwardLeft * obstacleVisionDistance, Color.pink);
                        Debug.DrawRay(carOrigin, carSideRight * obstacleVisionDistance, Color.pink);
                        Debug.DrawRay(carOrigin, carSideLeft * obstacleVisionDistance, Color.pink);
                        Debug.DrawRay(carOrigin, carBackRight * obstacleVisionDistance, Color.pink);
                        Debug.DrawRay(carOrigin, carBackLeft * obstacleVisionDistance, Color.pink);
                        Debug.DrawRay(carOrigin, -transform.forward * obstacleVisionDistance, Color.pink);



                    



                }

                Debug.DrawRay(carOrigin + raycastCarWidth, dirToMovePosition * distanceToTarget, Color.orange);
                Debug.DrawRay(carOrigin - raycastCarWidth, dirToMovePosition * distanceToTarget, Color.orange);
            }

            else {

                // Determine steering direction
                float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);


                // we might have "direct" line to the target, but we might be driving into a wall
                if (Physics.Raycast(ray, out RaycastHit hit_im_stuck_oni_chan, 25f, obstacleLayerMask))
                {
                    brakeInput = false; // apply brake

                    // Calculate the distance to the nearest obstacle
                    float distanceToObstacle = Vector3.Distance(transform.position, hit_im_stuck_oni_chan.point);

                    // get direction
                    Vector3 dirToObstacle = (targetPosition - hit_im_stuck_oni_chan.point).normalized;

                    // get angle
                    float angleToObstacle = Vector3.SignedAngle(transform.forward, dirToObstacle, Vector3.up);


                    if (distanceToObstacle < distanceToTarget)
                    {
                        // dont turn if angle is too shallow
                        /*
                        if (angleToObstacle >= 135)
                        {
                            turnAmount = Mathf.Clamp(angleToDir / 70f, 0f, 1f); // hard steer
                        }
                        else if (angleToObstacle <= -135) 
                        {
                            turnAmount = Mathf.Clamp(angleToDir / 70f, -1f, 0f); // hard steer
                        }
                        else
                        {
                            turnAmount = 0f; // no steering needed
                        }
                        */

                        forwardAmount = -1f; // full gas in reverse

                        Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.black);
                        // debug line
                        //Debug.Log("Angle towards obstacle: " + angleToObstacle + " Turn amount (+ is right, - is left): " + turnAmount + " forward amount (+ is forward, - is backward): " + forwardAmount);


                    }
                    else 
                    {

                        brakeInput = false; // release brake

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

                        Debug.DrawRay(carOrigin, transform.forward * obstacleVisionDistance, Color.orange);

                    }

                }
                else 
                {

                    brakeInput = false; // release brake

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

                }


                if (angleToDir == 0 || (angleToDir < 5 && angleToDir > -5))
                {
                    // check for obstacles
                    turnAmount = 0f; // no steering needed

                }
                else if ((angleToDir < 180 && angleToDir > 175) && forwardAmount < 0f) 
                {
                    turnAmount = 0; // turn left; // no steering needed

                }
                else if ((angleToDir < -175 && angleToDir > -180) && forwardAmount < 0f)
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

                Debug.DrawRay(carOrigin + raycastCarWidth, dirToMovePosition * distanceToTarget, Color.green);
                Debug.DrawRay(carOrigin - raycastCarWidth, dirToMovePosition * distanceToTarget, Color.green);
            }


        }
        else {
            // Reached the target position, stop moving
            forwardAmount = 0f; // No forward movement
            turnAmount = 0f; // No steering needed

            
            brakeInput = true; // Apply brake

            
                

        }

        // Send this movement information to the car controller AI
        carControllerAI.SetInputs(forwardAmount, turnAmount, brakeInput);

        

    }

    

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        // Additional logic can be added to adjust steering based on target position
    }

   
}
