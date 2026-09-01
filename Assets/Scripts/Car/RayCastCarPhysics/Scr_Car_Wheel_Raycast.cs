/*
using UnityEngine;
public class Scr_Car_Wheel_Raycast : MonoBehaviour
{
    [Header("Suspension")]
    [SerializeField] private float suspensionTravel = 0.35f;
    [SerializeField] private float springStrength = 45000f;
    [SerializeField] private float damperStrength = 6000f;
    [SerializeField] private float wheelRadius = 0.45f;

    [Header("Wheel Visual")]
    [Tooltip("The child object containing the visible wheel mesh.")]
    [SerializeField] private Transform wheelMesh;

    [Tooltip("Optional offset for the wheel mesh in the suspension's local space.")]
    [SerializeField] private Vector3 wheelMeshOffset = Vector3.zero;

    [Header("Tire Friction")]
    [SerializeField] private float sidewaysFriction = 1.5f;
    [SerializeField] private float forwardFriction = 0.15f;
    [SerializeField] private float frictionCoefficient = 1.2f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private bool showDebugRay = true;

    private Rigidbody carRigidbody;

    // Current suspension compression in meters.
    private float currentCompression;

    // The original local position of the visual wheel.
    private Vector3 wheelMeshStartLocalPosition;

    private void Awake()
    {
        carRigidbody = GetComponentInParent<Rigidbody>();

        if (carRigidbody == null)
        {
            Debug.LogError(
                $"No Rigidbody found in the parents of {gameObject.name}."
            );
        }

        // Automatically use the first child if a wheel mesh
        // hasn't been assigned manually.
        if (wheelMesh == null && transform.childCount > 0)
        {
            wheelMesh = transform.GetChild(0);
        }

        if (wheelMesh != null)
        {
            wheelMeshStartLocalPosition =
                wheelMesh.localPosition;
        }
        else
        {
            Debug.LogWarning(
                $"{gameObject.name} has no wheel mesh assigned."
            );
        }
    }

    private void FixedUpdate()
    {
        if (carRigidbody == null)
            return;

        ApplyWheelForces();
    }

    private void LateUpdate()
    {
        UpdateWheelVisual();
    }

    private void ApplyWheelForces()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -transform.up;

        float rayLength =
            suspensionTravel + wheelRadius;

        if (Physics.Raycast(
            rayOrigin,
            rayDirection,
            out RaycastHit hit,
            rayLength,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            // ============================================================
            // SUSPENSION
            // ============================================================

            float currentSuspensionLength =
                hit.distance - wheelRadius;

            currentSuspensionLength =
                Mathf.Clamp(
                    currentSuspensionLength,
                    0f,
                    suspensionTravel
                );

            // How far the suspension has compressed.
            //
            // 0 = fully extended
            // suspensionTravel = fully compressed
            currentCompression =
                suspensionTravel - currentSuspensionLength;

            // Spring force.
            float springForce =
                currentCompression * springStrength;

            // ============================================================
            // SUSPENSION DAMPING
            // ============================================================

            Vector3 pointVelocity =
                carRigidbody.GetPointVelocity(
                    transform.position
                );

            float suspensionVelocity =
                Vector3.Dot(
                    transform.up,
                    pointVelocity
                );

            // Damper opposes suspension movement.
            float damperForce =
                -suspensionVelocity * damperStrength;

            float normalForce =
                springForce + damperForce;

            // Never allow suspension to pull downward.
            normalForce =
                Mathf.Max(normalForce, 0f);

            // Apply suspension force.
            carRigidbody.AddForceAtPosition(
                transform.up * normalForce,
                transform.position,
                ForceMode.Force
            );

            // ============================================================
            // FRICTION
            // ============================================================

            ApplyFriction(
                hit,
                pointVelocity,
                normalForce
            );

            // ============================================================
            // DEBUG
            // ============================================================

            if (showDebugRay)
            {
                Debug.DrawRay(
                    rayOrigin,
                    rayDirection * rayLength,
                    Color.green
                );

                Debug.DrawRay(
                    hit.point,
                    transform.up *
                    (normalForce / 10000f),
                    Color.yellow
                );
            }
        }
        else
        {
            // No ground underneath the wheel.
            currentCompression = 0f;

            if (showDebugRay)
            {
                Debug.DrawRay(
                    rayOrigin,
                    rayDirection * rayLength,
                    Color.red
                );
            }
        }
    }

    private void UpdateWheelVisual()
    {
        if (wheelMesh == null)
            return;

        // Convert compression into a 0-1 value.
        float compression01 =
            currentCompression / suspensionTravel;

        compression01 =
            Mathf.Clamp01(compression01);

        // Move the visual wheel DOWN as the suspension compresses.
        //
        // At 0 compression:
        //     wheel = original position
        //
        // At full compression:
        //     wheel = suspensionTravel lower
        Vector3 suspensionOffset =
            -transform.up *
            currentCompression;

        // Convert the world-space suspension offset into
        // the wheel mesh parent's local space.
        Vector3 localSuspensionOffset =
            transform.InverseTransformVector(
                suspensionOffset
            );

        wheelMesh.localPosition =
            wheelMeshStartLocalPosition +
            localSuspensionOffset +
            wheelMeshOffset;
    }

    private void ApplyFriction(
        RaycastHit hit,
        Vector3 pointVelocity,
        float normalForce)
    {
        if (normalForce <= 0f)
            return;

        // ================================================================
        // WHEEL DIRECTIONS
        // ================================================================

        Vector3 forward =
            Vector3.ProjectOnPlane(
                transform.forward,
                hit.normal
            ).normalized;

        Vector3 sideways =
            Vector3.Cross(
                hit.normal,
                forward
            ).normalized;

        // ================================================================
        // CONTACT VELOCITY
        // ================================================================

        Vector3 contactVelocity =
            carRigidbody.GetPointVelocity(
                hit.point
            );

        float forwardVelocity =
            Vector3.Dot(
                contactVelocity,
                forward
            );

        float sidewaysVelocity =
            Vector3.Dot(
                contactVelocity,
                sideways
            );

        // ================================================================
        // FRICTION
        // ================================================================

        float forwardFrictionForce =
            -forwardVelocity *
            forwardFriction *
            carRigidbody.mass;

        float sidewaysFrictionForce =
            -sidewaysVelocity *
            sidewaysFriction *
            carRigidbody.mass;

        // ================================================================
        // FRICTION LIMIT
        // ================================================================

        float maximumFrictionForce =
            normalForce *
            frictionCoefficient;

        forwardFrictionForce =
            Mathf.Clamp(
                forwardFrictionForce,
                -maximumFrictionForce,
                maximumFrictionForce
            );

        sidewaysFrictionForce =
            Mathf.Clamp(
                sidewaysFrictionForce,
                -maximumFrictionForce,
                maximumFrictionForce
            );

        Vector3 frictionForce =
            forward * forwardFrictionForce +
            sideways * sidewaysFrictionForce;

        carRigidbody.AddForceAtPosition(
            frictionForce,
            hit.point,
            ForceMode.Force
        );

        // ================================================================
        // DEBUG
        // ================================================================

        if (showDebugRay)
        {
            Debug.DrawRay(
                hit.point,
                forward *
                (forwardFrictionForce / 5000f),
                Color.blue
            );

            Debug.DrawRay(
                hit.point,
                sideways *
                (sidewaysFrictionForce / 5000f),
                Color.magenta
            );
        }
    }
}
*/

using UnityEngine;

public class Scr_Car_Wheel_Raycast : MonoBehaviour
{
    [Header("Suspension")]
    [SerializeField] private float suspensionTravel = 0.35f;
    [SerializeField] private float springStrength = 45000f;
    [SerializeField] private float damperStrength = 6000f;
    [SerializeField] private float wheelRadius = 0.35f;

    [Header("Wheel Visual")]
    [Tooltip("The child object containing the wheel mesh.")]
    [SerializeField] private Transform wheelMesh;

    [Tooltip(
        "Visual offset from the wheel-point. " +
        "Y offset is used only while airborne."
    )]
    [SerializeField] private Vector3 wheelMeshOffset = Vector3.zero;

    [Header("Tire Friction")]
    [SerializeField] private float sidewaysFriction = 1.5f;
    [SerializeField] private float forwardFriction = 0.15f;
    [SerializeField] private float frictionCoefficient = 1.2f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private bool showDebugRay = true;

    private Rigidbody carRigidbody;

    // Original local position of the wheel mesh.
    private Vector3 wheelMeshStartLocalPosition;

    // Current suspension compression.
    private float currentCompression;

    // Current grounded state.
    private bool isGrounded;

    // Current raycast hit.
    private RaycastHit currentHit;

    private void Awake()
    {
        carRigidbody =
            GetComponentInParent<Rigidbody>();

        if (carRigidbody == null)
        {
            Debug.LogError(
                $"No Rigidbody found for wheel {gameObject.name}."
            );
        }

        // Automatically use the first child as the wheel mesh.
        if (wheelMesh == null &&
            transform.childCount > 0)
        {
            wheelMesh =
                transform.GetChild(0);
        }

        if (wheelMesh != null)
        {
            wheelMeshStartLocalPosition =
                wheelMesh.localPosition;
        }
        else
        {
            Debug.LogWarning(
                $"No wheel mesh assigned to {gameObject.name}."
            );
        }
    }

    private void FixedUpdate()
    {
        if (carRigidbody == null)
            return;

        CalculateWheelPhysics();
    }

    private void LateUpdate()
    {
        UpdateWheelVisual();
    }

    private void CalculateWheelPhysics()
    {
        Vector3 rayOrigin =
            transform.position;

        Vector3 rayDirection =
            -transform.up;

        float rayLength =
            suspensionTravel +
            wheelRadius;

        // ================================================================
        // RAYCAST
        // ================================================================

        if (Physics.Raycast(
            rayOrigin,
            rayDirection,
            out RaycastHit hit,
            rayLength,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            currentHit = hit;

            // ============================================================
            // SUSPENSION LENGTH
            // ============================================================

            float suspensionLength =
                hit.distance -
                wheelRadius;

            suspensionLength =
                Mathf.Clamp(
                    suspensionLength,
                    0f,
                    suspensionTravel
                );

            // ============================================================
            // SUSPENSION COMPRESSION
            // ============================================================

            currentCompression =
                suspensionTravel -
                suspensionLength;

            // ============================================================
            // SPRING FORCE
            // ============================================================

            float springForce =
                currentCompression *
                springStrength;

            // ============================================================
            // DAMPING
            // ============================================================

            Vector3 wheelPointVelocity =
                carRigidbody.GetPointVelocity(
                    transform.position
                );

            float suspensionVelocity =
                Vector3.Dot(
                    transform.up,
                    wheelPointVelocity
                );

            float damperForce =
                -suspensionVelocity *
                damperStrength;

            // ============================================================
            // TOTAL SUSPENSION FORCE
            // ============================================================

            float normalForce =
                springForce +
                damperForce;

            normalForce =
                Mathf.Max(
                    normalForce,
                    0f
                );

            // ============================================================
            // APPLY SUSPENSION FORCE
            // ============================================================

            carRigidbody.AddForceAtPosition(
                transform.up *
                normalForce,
                transform.position,
                ForceMode.Force
            );

            // ============================================================
            // FRICTION
            // ============================================================

            ApplyFriction(
                hit,
                wheelPointVelocity,
                normalForce
            );

            // ============================================================
            // DEBUG
            // ============================================================

            if (showDebugRay)
            {
                Debug.DrawRay(
                    rayOrigin,
                    rayDirection *
                    rayLength,
                    Color.green
                );

                Debug.DrawRay(
                    hit.point,
                    transform.up *
                    (normalForce / 10000f),
                    Color.yellow
                );
            }
        }
        else
        {
            // ============================================================
            // AIRBORNE
            // ============================================================

            isGrounded = false;
            currentCompression = 0f;

            if (showDebugRay)
            {
                Debug.DrawRay(
                    rayOrigin,
                    rayDirection *
                    rayLength,
                    Color.red
                );
            }
        }
    }

    private void UpdateWheelVisual()
    {
        if (wheelMesh == null)
            return;

        // ================================================================
        // AIRBORNE
        // ================================================================

        if (!isGrounded)
        {
            /*
             * When airborne, use the complete visual offset.
             *
             * This is important because your wheel-point empty is
             * positioned above the actual wheel center.
             */

            wheelMesh.localPosition =
                wheelMeshStartLocalPosition +
                wheelMeshOffset;

            return;
        }

        // ================================================================
        // GROUNDED
        // ================================================================

        /*
         * When grounded, DO NOT use the Y component of
         * wheelMeshOffset.
         *
         * Instead, the raycast determines exactly where the
         * wheel center needs to be.
         */

        Vector3 wheelCenterWorld =
            currentHit.point +
            transform.up *
            wheelRadius;

        // Convert the raycast-based wheel center into
        // the wheel-point's local coordinate space.
        Vector3 wheelCenterLocal =
            transform.InverseTransformPoint(
                wheelCenterWorld
            );

        // ================================================================
        // SUSPENSION LIMITS
        // ================================================================

        /*
         * The wheel-point is the suspension attachment point.
         *
         * The wheel cannot move farther than suspensionTravel
         * from its fully extended position.
         */

        float maximumY =
            wheelMeshStartLocalPosition.y;

        float minimumY =
            wheelMeshStartLocalPosition.y -
            suspensionTravel;

        float constrainedY =
            Mathf.Clamp(
                wheelCenterLocal.y,
                minimumY,
                maximumY
            );

        // ================================================================
        // FINAL WHEEL POSITION
        // ================================================================

        Vector3 finalLocalPosition =
            wheelMeshStartLocalPosition;

        // X and Z offsets are still allowed.
        finalLocalPosition.x +=
            wheelMeshOffset.x;

        finalLocalPosition.z +=
            wheelMeshOffset.z;

        // Y is controlled ONLY by the raycast.
        finalLocalPosition.y =
            constrainedY;

        wheelMesh.localPosition =
            finalLocalPosition;
    }

    private void ApplyFriction(
        RaycastHit hit,
        Vector3 wheelPointVelocity,
        float normalForce)
    {
        if (normalForce <= 0f)
            return;

        // ================================================================
        // WHEEL DIRECTIONS
        // ================================================================

        Vector3 forward =
            Vector3.ProjectOnPlane(
                transform.forward,
                hit.normal
            ).normalized;

        Vector3 sideways =
            Vector3.Cross(
                hit.normal,
                forward
            ).normalized;

        // ================================================================
        // CONTACT VELOCITY
        // ================================================================

        Vector3 contactVelocity =
            carRigidbody.GetPointVelocity(
                hit.point
            );

        float forwardVelocity =
            Vector3.Dot(
                contactVelocity,
                forward
            );

        float sidewaysVelocity =
            Vector3.Dot(
                contactVelocity,
                sideways
            );

        // ================================================================
        // FRICTION
        // ================================================================

        float forwardFrictionForce =
            -forwardVelocity *
            forwardFriction *
            carRigidbody.mass;

        float sidewaysFrictionForce =
            -sidewaysVelocity *
            sidewaysFriction *
            carRigidbody.mass;

        // ================================================================
        // FRICTION LIMIT
        // ================================================================

        float maximumFrictionForce =
            normalForce *
            frictionCoefficient;

        forwardFrictionForce =
            Mathf.Clamp(
                forwardFrictionForce,
                -maximumFrictionForce,
                maximumFrictionForce
            );

        sidewaysFrictionForce =
            Mathf.Clamp(
                sidewaysFrictionForce,
                -maximumFrictionForce,
                maximumFrictionForce
            );

        // ================================================================
        // APPLY FRICTION
        // ================================================================

        Vector3 frictionForce =
            forward *
            forwardFrictionForce +
            sideways *
            sidewaysFrictionForce;

        carRigidbody.AddForceAtPosition(
            frictionForce,
            hit.point,
            ForceMode.Force
        );

        // ================================================================
        // DEBUG
        // ================================================================

        if (showDebugRay)
        {
            Debug.DrawRay(
                hit.point,
                forward *
                (forwardFrictionForce / 5000f),
                Color.blue
            );

            Debug.DrawRay(
                hit.point,
                sideways *
                (sidewaysFrictionForce / 5000f),
                Color.magenta
            );
        }
    }
}

