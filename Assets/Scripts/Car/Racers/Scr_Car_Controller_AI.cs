using System;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerAI : MonoBehaviour
{
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
        public GameObject WheelEffectObject;
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

    bool brakeInput;

    private Rigidbody carRb;

    // car specific
    [SerializeField] float maxSpeed; // Maximum speed of the car
    //[SerializeField] float downwardForce; // Downward force for stability
    [SerializeField] float steeringWheelTurnSpeed; // Speed of the steering wheel animation

    // get the car cop lights handler script
    [SerializeField] private Scr_Car_Lights_Handler carLightsHandlerScript;

    // get car ai script to know if we are on offroad terrain
    private CarAISimple carAiScript;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();

        carRb.centerOfMass = _centerOfMass;

        // get the car lights handler script component if not assigned
        carLightsHandlerScript = GetComponentInChildren<Scr_Car_Lights_Handler>();

        // get car ai script component
        carAiScript = GetComponent<CarAISimple>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelSkidEffects();
    }

    void FixedUpdate()
    {
        Move();
        Steer();
        Brake();

        // Limit the speed of the car
        if (carRb.linearVelocity.magnitude > maxSpeed)
        {
            carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity, maxSpeed);
        }

        // set brake lights based on brake input
        carLightsHandlerScript.SetBrakeCondition(brakeInput);

    }

    public void SetInputs(float forwardAmount, float turnAmount, bool brake)
    {
        moveInput = forwardAmount;
        steerInput = turnAmount;
        brakeInput = brake;
    }


    void GetInputs()
    {
        //moveInput = Input.GetAxis("Vertical");
        //steerInput = Input.GetAxis("Horizontal");
    }


    void Move()
    {
        // if we are on offroad terrain, cut acceleration in half
        if (carAiScript.isCarOffroad())
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = moveInput * 600 * (maxAcceleration / 2) * Time.deltaTime;
            }

        }
        // if not use default acceleration
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
            }

        }
    }

    void Steer()
    {

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, steeringWheelTurnSpeed);

            }

        }

    }

    void Brake()
    {
        if (brakeInput == true)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;

            }

        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;


            }

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
            // reset wheel spin to zero by setting the wheel collider's rpm to zero
            wheel.wheelCollider.motorTorque = 0f;

            // get rid of its angular velocity so it is not spinning
            wheel.wheelCollider.attachedRigidbody.angularVelocity = Vector3.zero;
        }
    }

    // wheel effects functions
    // skid marks
    public void WheelSkidEffects() 
    {
        foreach (var wheel in wheels)
        {
            // set each wheel trail renderer object transform rotation to lay flat on the ground
            wheel.WheelEffectObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            // if we are braking, play skid marks trail effects
            if (brakeInput == true)
            {
                // check if we are grounded
                if (wheel.wheelCollider.isGrounded)
                {
                    wheel.WheelEffectObject.GetComponent<TrailRenderer>().emitting = true;
                }
                else 
                {
                    wheel.WheelEffectObject.GetComponent<TrailRenderer>().emitting = false;
                }
            }
            // if are not braking, stop skid marks trail effects
            else
            {
                wheel.WheelEffectObject.GetComponent<TrailRenderer>().emitting = false;
            }
        }
    }

}
