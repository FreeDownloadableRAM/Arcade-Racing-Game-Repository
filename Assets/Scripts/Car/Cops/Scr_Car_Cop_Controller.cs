using UnityEngine;
using System;
using System.Collections.Generic;

public class CarCopController : MonoBehaviour
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

    private scr_Car_Cop_Target_Handler carCopTargetHandler;

    // car specific
    [SerializeField] float maxSpeed; // Maximum speed of the car
    [SerializeField] float patrolSpeed; // Speed of the car when in Idle state

    [SerializeField] float steeringWheelTurnSpeed; // Speed of the steering wheel animation

    // get the car cop lights handler script
    [SerializeField] private Scr_Car_Cop_Lights_Handler carLightsHandlerScript;

    // get car ai script to know if we are on offroad terrain
    private Scr_Car_Cop_AI carCopAiScript;


    void Start()
    {
        carRb = GetComponent<Rigidbody>();

        carRb.centerOfMass = _centerOfMass;

        // get cop car target handler script component
        carCopTargetHandler = GetComponent<scr_Car_Cop_Target_Handler>();

        // get the car lights handler script component if not assigned
        carLightsHandlerScript = GetComponentInChildren<Scr_Car_Cop_Lights_Handler>();

        // get cop car ai script component
        carCopAiScript = GetComponent<Scr_Car_Cop_AI>();

    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelSkidEffects();
        TireParticleEffects();
    }

    void FixedUpdate()
    {
        Move();
        Steer();
        Brake();



        // determine ai state from target handler script
        if (carCopTargetHandler.AIState == "Idle")
        {
            // Limit the speed of the car to patrol speed when idle
            if (carRb.linearVelocity.magnitude > patrolSpeed)
            {
                carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity, patrolSpeed);
            }

        }
        else
        {
            // Limit the speed of the car to its defined max speed
            if (carRb.linearVelocity.magnitude > maxSpeed)
            {
                carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity, maxSpeed);
            }

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
        if (carCopAiScript.isCarOffroad())
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

    
    // wheel effects functions
    // skid marks
    public void WheelSkidEffects()
    {
        foreach (var wheel in wheels)
        {
            // set each wheel trail renderer object transform rotation to lay flat on the ground
            wheel.WheelEffectObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            if (carRb.linearVelocity.magnitude > 30f)
            {
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
            else
            {
                wheel.WheelEffectObject.GetComponent<TrailRenderer>().emitting = false;
            }

        }
    }

    // tire particle effects
    // when driving these effects will play
    public void TireParticleEffects()
    {
        foreach (var wheel in wheels)
        {
            // if we are braking, play tire particle effects
            if (brakeInput == true)
            {

                // check if we are grounded
                if (wheel.wheelCollider.isGrounded)
                {
                    // play the particle system if it is not already playing
                    if (!wheel.WheelEffectObject.GetComponent<ParticleSystem>().isPlaying)
                    {
                        wheel.WheelEffectObject.GetComponent<ParticleSystem>().Play();
                    }
                }
                else
                {
                    wheel.WheelEffectObject.GetComponent<ParticleSystem>().Stop();
                }
            }
            // if are not braking, stop tire particle effects
            else
            {
                wheel.WheelEffectObject.GetComponent<ParticleSystem>().Stop();
            }
        }
    }

    // getter for max speed
    public float getMaxSpeed()
    {
        return maxSpeed;
    }

}