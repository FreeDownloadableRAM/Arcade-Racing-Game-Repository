using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    private Vector3 carOrigin; // Origin of the car for raycasting

    [SerializeField] private Vector3 raycastOffsetFromGround; // Offset for the raycast to come from origin of the car

    // ray cast pointing towards the ground
    Ray rayDown; // if this collides with terrain layer objects, we are on offroad, not the road.

    // layer mask for offroad terrain detection
    private LayerMask offroadTerrainLayerMask; // Layer mask for offroad terrain detection

    // boolean for if we are offroad or not
    private bool isOffroad = false; // Are we currently offroad?

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

    private Rigidbody carRb;

    [SerializeField] float maxSpeed; // Maximum speed of the car
    //[SerializeField] float downwardForce; // Downward force for stability

    // get the car cop lights handler script
    [SerializeField] private Scr_Player_Car_Lights_Handler carLightsHandlerScript;

    // get race manager script reference
    private scr_My_Race_Progress scr_myRaceProgress;

    private bool brakeInput;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();

        carRb.centerOfMass = _centerOfMass;

        // get the car lights handler script component if not assigned
        carLightsHandlerScript = GetComponentInChildren<Scr_Player_Car_Lights_Handler>();

        // get race manager component reference
        scr_myRaceProgress = GetComponent<scr_My_Race_Progress>();

        // Layer mask for offroad terrain detection
        offroadTerrainLayerMask = LayerMask.GetMask("Terrain");
    }

    void Update()
    {

        GetInputs();
        AnimateWheels();
        WheelSkidEffects();
        TireParticleEffects();

        // avoid obstacles
        carOrigin = transform.position + raycastOffsetFromGround; // Set the origin of the raycast

        // offroad terrain detection raycast
        rayDown = new Ray(carOrigin, -transform.up);

        // restart scene when pushing the R key, for testing purposes
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        // close application when pushing the T key, for testing purposes
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // when pushing the E key, move to next unity scene, for testing purposes
        if (Input.GetKeyDown(KeyCode.E))
        {
            int nextSceneIndex = (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1) % UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
    }

    void FixedUpdate()
    {
        // set offroad flag to false by default, will be set to true if we detect offroad terrain below us
        isOffroad = false;

        Move();
        Steer();
        Brake();

        // check if we are offroad with downwards raycast
        if (Physics.Raycast(rayDown, out RaycastHit hitDown, 5f, offroadTerrainLayerMask))
        {
            // set offroad flag to true
            isOffroad = true;

        }

        // if we are offroad, reduce the max speed by half
        if (isOffroad)
        {
            // force slow down car velocity until it reaches half of max speed
            carRb.linearVelocity = Vector3.Lerp(carRb.linearVelocity, Vector3.ClampMagnitude(carRb.linearVelocity, maxSpeed / 4), 0.02f);

        }
        else
        {
            carRb.linearVelocity = Vector3.ClampMagnitude(carRb.linearVelocity, maxSpeed);
        }

        // set brake lights based on brake input
        carLightsHandlerScript.SetBrakeCondition(brakeInput);

        // apply downward force for stability
        //carRb.AddForce(Vector3.down * downwardForce * carRb.linearVelocity.magnitude, ForceMode.Impulse);
    }

    void GetInputs()
    {
        // check if we have competed the race
        // if so, do not accept player input for movement, only allow brake input to stop the car after
        // if we finished race, do not run any logic
        if (scr_myRaceProgress.completedRace == true)
        {
            steerInput = Input.GetAxis("Horizontal");

            return;
        }

        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void Move()
    {
        // check if we have competed the race
        // if we finished race, do not run any logic
        if (scr_myRaceProgress.completedRace == true)
        {
            return;
        }

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
        // check if we have competed the race
        // if so, do not accept player input for movement, only allow brake input to stop the car after
        // if we finished race, do not run any logic
        if (scr_myRaceProgress.completedRace == true)
        {
            brakeInput = true;

            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 600 * brakeAcceleration * Time.deltaTime; // originally was at 600

            }

            return;
        }



        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 450 * brakeAcceleration * Time.deltaTime;

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

    // wheel effects functions
    // skid marks
    public void WheelSkidEffects()
    {
        foreach (var wheel in wheels)
        {
            // set each wheel trail renderer object transform rotation to lay flat on the ground
            wheel.WheelEffectObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            if (carRb.linearVelocity.magnitude > 35f)
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