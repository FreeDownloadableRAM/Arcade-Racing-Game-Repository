using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Scr_RollBar : MonoBehaviour
{

    // we will use the rear back left and right wheels for anti-roll bar calculations
    public WheelCollider wheelL; // left wheel
    public WheelCollider wheelR; // right wheel

    [SerializeField] private float AntiRollBarForce = 5000.0f; // force applied to counteract roll
    [SerializeField] private float downwardForce; // downward force for stability

    private Rigidbody car_rb; // reference to the car's rigidbody

    private void Start()
    {
        // Get the Rigidbody component attached to the car
        car_rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        WheelHit hit;

        float travelL = 1.0f; // left wheel travel
        float travelR = 1.0f; // right wheel travel

        // make sure we are grounded before applying anti-roll bar force
        // left wheel calculations
        bool groundedL = wheelL.GetGroundHit(out hit); // check if left wheel is grounded
        if (groundedL)
        {
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
            // apply downward force for stability
            
        }

        // Right wheel calculations
        bool groundedR = wheelR.GetGroundHit(out hit); // check if right wheel is grounded
        if (groundedR)
        {
            travelR = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
        }

        // apply anti-roll bar force
        float antiRollForce = (travelL - travelR) * AntiRollBarForce; // calculate the force to apply

        // apply if grounded
        if (groundedL)
        {
            car_rb.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position); // apply force to left wheel
            car_rb.AddForce(Vector3.down * downwardForce * car_rb.linearVelocity.magnitude, ForceMode.Impulse);
        }
        if (groundedR)
        {
            car_rb.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position); // apply force to right wheel
            car_rb.AddForce(Vector3.down * downwardForce * car_rb.linearVelocity.magnitude, ForceMode.Impulse);
        }

    }

}
