using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_AI_ChatGPT : MonoBehaviour
{
    [Header("Pathfinding & Core")]
    [SerializeField] private float NextVertexDistanceThreshold;
    [SerializeField] private Rigidbody rigidBody;
    private Scr_Car_Pathfinder scrCarPathfinder;
    private Scr_BrakeZone_Hint_Properties brakeZoneHintProperties;
    private CarControllerAI carControllerAI;
    private scr_My_Race_Progress scrMyRaceProgress;
    [SerializeField] private Vector3 targetPosition;

    [Header("Steering")]
    [SerializeField] private float steeringAngleThreshold = 3f;

    [Header("Vision Distances")]
    [SerializeField] private float proximityBaseVisionDistance = 1.0f;
    [SerializeField] private float proximityMinVisionDistance = 2.5f;
    [SerializeField] private float proximityMaxVisionDistance = 8.0f;

    [Header("Flow-Field Avoidance")]
    private float proximityDetectionRadius = 3.5f;
    [SerializeField] private float proximityAvoidStrength = 1.5f;
    [SerializeField] private float targetFollowWeight = 1.0f;
    [SerializeField] private float avoidWeight = 1.2f;
    [SerializeField] private LayerMask proximityObstacleMask;

    [Header("Stuck Detection & Recovery")]
    [SerializeField] private float stuckSpeedThreshold = 1.0f;
    [SerializeField] private float stuckTimeThreshold = 2.0f;
    [SerializeField] private float recoveryReverseTime = 1.5f;
    [SerializeField] private float recoveryTurnStrength = 0.8f;
    private float stuckTimer = 0f;
    private float recoveryTimer = 0f;
    private bool isRecovering = false;

    [Header("Smart Overtaking Settings")]
    [SerializeField] private float overtakeDetectionDistance = 10f;
    //[SerializeField] private float overtakeSideOffset = 2.5f;
    [SerializeField] private float overtakeStrength = 1.2f;
    [SerializeField] private float overtakeAngleThreshold = 25f;
    [SerializeField] private LayerMask carMask; // Assign the "Car" layer here
    [SerializeField] private float sideCheckDistance = 5f;

    // Internal
    private bool areWeInBrakeZone = false;
    private float forwardAmount = 0f;
    private float turnAmount = 0f;
    private bool brakeInput = false;
    private float raceFinishedRandomSteer = 0f;

    // Debug
    private Vector3 debugDirToTarget;
    private Vector3 debugAvoidance;
    private Vector3 debugOvertake;
    private Vector3 debugCombined;
    private Color debugOvertakeColor = Color.magenta;

    private void Awake()
    {
        carControllerAI = GetComponent<CarControllerAI>();
        scrCarPathfinder = GetComponent<Scr_Car_Pathfinder>();
        scrMyRaceProgress = GetComponent<scr_My_Race_Progress>();
        raceFinishedRandomSteer = Random.Range(-1f, 1f);
    }

    private void Update()
    {
        targetPosition = scrCarPathfinder.carTarget;
        SetTargetPosition(targetPosition);

        forwardAmount = 0f;
        turnAmount = 0f;
        brakeInput = false;

        float speed = rigidBody.linearVelocity.magnitude;
        proximityDetectionRadius = Mathf.Clamp(speed * proximityBaseVisionDistance, proximityMinVisionDistance, proximityMaxVisionDistance);

        if (scrMyRaceProgress.completedRace)
        {
            carControllerAI.SetInputs(0f, raceFinishedRandomSteer, true);
            return;
        }

        Vector3 dirToMovePosition = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        float distanceToEndTarget = Vector3.Distance(transform.position, scrCarPathfinder.endtarget.position);
        float dotProduct = Vector3.Dot(transform.forward, dirToMovePosition);

        if (distanceToTarget < NextVertexDistanceThreshold)
            targetPosition = scrCarPathfinder.endtarget.position;

        if (distanceToEndTarget > 7.5f)
        {
            if (dotProduct > 0)
            {
                float brakeDistance = 15f;
                forwardAmount = (distanceToTarget < brakeDistance) ? 0.25f : 1f;
            }
            else forwardAmount = -1f;

            Vector3 desiredDir = ComputeFlowFieldDirection(targetPosition);
            float desiredAngle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);

            turnAmount = Mathf.Abs(desiredAngle) > steeringAngleThreshold
                ? Mathf.Clamp(desiredAngle / 70f, -1f, 1f)
                : 0f;
        }
        else
        {
            forwardAmount = 0f;
            turnAmount = 0f;
            brakeInput = true;
        }

        HandleStuckRecovery(ref forwardAmount, ref turnAmount);

        if (areWeInBrakeZone)
        {
            float targetSpeed = brakeZoneHintProperties.targetSpeed;
            if (targetSpeed < rigidBody.linearVelocity.magnitude)
            {
                brakeInput = true;
                forwardAmount = 0f;
            }
        }

        carControllerAI.SetInputs(forwardAmount, turnAmount, brakeInput);
    }

    /// <summary>
    /// Combines steering direction toward target with obstacle and overtaking behavior.
    /// </summary>
    private Vector3 ComputeFlowFieldDirection(Vector3 targetPos)
    {
        Vector3 dirToTarget = (targetPos - transform.position).normalized;
        Vector3 avoidance = Vector3.zero;
        Vector3 overtake = Vector3.zero;

        // --- Obstacle avoidance ---
        Collider[] hits = Physics.OverlapSphere(transform.position, proximityDetectionRadius, proximityObstacleMask);
        foreach (var hit in hits)
        {
            if (hit.attachedRigidbody == rigidBody) continue;
            Vector3 away = transform.position - hit.ClosestPoint(transform.position);
            float distance = away.magnitude;
            if (distance < 0.001f) continue;

            float weight = Mathf.Clamp01(1f - (distance / proximityDetectionRadius));
            avoidance += away.normalized * weight * proximityAvoidStrength;
        }

        // --- Smart Overtaking ---
        bool avoidingObstacle = avoidance.sqrMagnitude > 0.1f;
        if (!avoidingObstacle)
        {
            Collider[] carsAhead = Physics.OverlapSphere(transform.position, overtakeDetectionDistance, carMask);
            foreach (var other in carsAhead)
            {
                if (other.attachedRigidbody == rigidBody) continue;

                Vector3 toOther = other.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, toOther);

                if (angle < overtakeAngleThreshold && Vector3.Dot(transform.forward, toOther.normalized) > 0)
                {
                    // Car detected ahead, choose safest side
                    float leftClear = CheckSideClearance(-transform.right);
                    float rightClear = CheckSideClearance(transform.right);

                    Vector3 overtakeDir;
                    if (leftClear > rightClear)
                    {
                        overtakeDir = Quaternion.AngleAxis(-30f, Vector3.up) * transform.forward;
                        debugOvertakeColor = Color.cyan; // left
                    }
                    else
                    {
                        overtakeDir = Quaternion.AngleAxis(30f, Vector3.up) * transform.forward;
                        debugOvertakeColor = Color.magenta; // right
                    }

                    overtake += overtakeDir * overtakeStrength;
                    break;
                }
            }
        }

        Vector3 combined = (dirToTarget * targetFollowWeight) + (avoidance * avoidWeight) + overtake;

        debugDirToTarget = dirToTarget;
        debugAvoidance = avoidance;
        debugOvertake = overtake;
        debugCombined = combined.normalized;

        return combined.normalized;
    }

    /// <summary>
    /// Checks how much free space exists on a given side using raycasts.
    /// </summary>
    private float CheckSideClearance(Vector3 sideDir)
    {
        Vector3 start = transform.position + Vector3.up * 0.5f;
        Vector3 dir = (transform.forward + sideDir * 0.5f).normalized;

        if (Physics.Raycast(start, dir, out RaycastHit hit, sideCheckDistance, proximityObstacleMask))
        {
            Debug.DrawRay(start, dir * hit.distance, Color.red);
            return hit.distance;
        }

        Debug.DrawRay(start, dir * sideCheckDistance, Color.green);
        return sideCheckDistance;
    }

    private void HandleStuckRecovery(ref float forwardAmount, ref float turnAmount)
    {
        float currentSpeed = rigidBody.linearVelocity.magnitude;

        if (!isRecovering)
        {
            if (currentSpeed < stuckSpeedThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckTimeThreshold)
                {
                    isRecovering = true;
                    recoveryTimer = 0f;
                    Debug.Log($"{name} is stuck! Starting adaptive recovery...");
                }
            }
            else stuckTimer = 0f;
        }
        else
        {
            recoveryTimer += Time.deltaTime;
            forwardAmount = -1f;

            Vector3 toTarget = (targetPosition - transform.position).normalized;
            float angleToTarget = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);

            if (Mathf.Abs(angleToTarget) > steeringAngleThreshold)
            {
                float alignmentFactor = Mathf.Clamp01(Mathf.Abs(angleToTarget) / 90f);
                turnAmount = Mathf.Sign(angleToTarget) * recoveryTurnStrength * alignmentFactor;
            }
            else turnAmount = 0f;

            Debug.DrawRay(transform.position, -transform.forward * 3f, Color.magenta);
            Debug.DrawRay(transform.position, toTarget * 3f, Color.yellow);

            if (recoveryTimer > recoveryReverseTime)
            {
                isRecovering = false;
                stuckTimer = 0f;
                Debug.Log($"{name} finished adaptive recovery.");
            }
        }
    }

    public void SetTargetPosition(Vector3 targetPosition) => this.targetPosition = targetPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BrakeZone"))
        {
            brakeZoneHintProperties = other.GetComponent<Scr_BrakeZone_Hint_Properties>();
            areWeInBrakeZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BrakeZone"))
            areWeInBrakeZone = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, proximityDetectionRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + debugDirToTarget * 3f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + debugAvoidance.normalized * 3f);

            Gizmos.color = debugOvertakeColor;
            Gizmos.DrawLine(transform.position, transform.position + debugOvertake.normalized * 3f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + debugCombined * 4f);
        }
    }
}
