using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    private Vector3 carOrigin; // Origin of the car for raycasting

    [SerializeField] private Vector3 raycastOffsetFromGround; // Offset for the raycast to come from origin of the car

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 45.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    [SerializeField] float maxSpeed; // Maximum speed of the car
    //[SerializeField] float downwardForce; // Downward force for stability

    // get the car cop lights handler script
    [SerializeField] private Scr_Player_Car_Lights_Handler carLightsHandlerScript;

    private bool brakeInput;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();

        carRb.centerOfMass = _centerOfMass;

        // get the car lights handler script component if not assigned
        carLightsHandlerScript = GetComponentInChildren<Scr_Player_Car_Lights_Handler>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();

        // avoid obstacles
        carOrigin = transform.position + raycastOffsetFromGround; // Set the origin of the raycast

    }

    void FixedUpdate()
    {
        Move();
        Steer();
        Brake();

        // Limit the speed of the car
        if (carRb.linearVelocity.magnitude > maxSpeed)
        {
            carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity,maxSpeed);
        }

        // set brake lights based on brake input
        carLightsHandlerScript.SetBrakeCondition(brakeInput);

        // apply downward force for stability
        //carRb.AddForce(Vector3.down * downwardForce * carRb.linearVelocity.magnitude, ForceMode.Impulse);
    }

    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);

            }

        }

    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;

            }

            brakeInput = true;
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;


            }

            brakeInput = false;

        }

    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;

        }

    }

    // item modifier functions
    // nitro
    public float getMaxSpeed()
    {
        return maxSpeed;
    }

    public float getAcceleration()
    {
        return maxAcceleration;
    }

    public void setMaxSpeed(float setMaxSpeed)
    {
        maxSpeed = setMaxSpeed;
    }

    public void setAcceleration(float setAcceleration)
    {
        maxAcceleration = setAcceleration;
    }

    // reset wheel to default position
    public void resetWheelsToDefaultPosition()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.transform.localRotation = Quaternion.identity;
        }
    }

    // set wheel spin to zero so it is not spinning when car is reset
    public void resetWheelSpinToZero()
    {
        foreach (var wheel in wheels)
        {
            // set brake torque high to stop wheel from spinning
            wheel.wheelCollider.brakeTorque = 10000f;
        }
    }

    public Vector3 getCarOrigin()
    {
        return carOrigin;
    }

    // return car height off ground offset
    public Vector3 getCarHeightOffGroundRaycastOffset()
    {
        return raycastOffsetFromGround;
    }
}
