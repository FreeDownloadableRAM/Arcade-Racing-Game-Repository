using UnityEngine;

public class Scr_Player_Car_Lights_Handler : MonoBehaviour
{
    // get light component references
    // front lights
    [SerializeField] private Light headLightLeft;
    [SerializeField] private Light headLightRight;

    // cone lights
    [SerializeField] private Light headConeLightLeft;
    [SerializeField] private Light headConeLightRight;

    // back lights
    [SerializeField] private Light brakeLightLeft;
    [SerializeField] private Light brakeLightRight;

    // light intensity variables
    [SerializeField] private float brakeLightOnIntensity;
    [SerializeField] private float brakeLightOffIntensity;

    // we want to reference the car controller script
    // to see when we are braking or reversing
    [SerializeField] private CarController carControllerScript;

    // car braking state
    bool isCarBraking = false;

    // is it night time in the level
    [SerializeField] bool isNightTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // turn off all lights at start
        headLightLeft.enabled = false;
        headLightRight.enabled = false;
        headConeLightLeft.enabled = false;
        headConeLightRight.enabled = false;
        brakeLightLeft.enabled = false;
        brakeLightRight.enabled = false;

        // get the car controller script from the root object if not assigned
        carControllerScript = GetComponentInParent<CarController>();
        

    }

    // Update is called once per frame
    void Update()
    {
        // check if car is braking using the car controller script reference
        if (carControllerScript != null)
        {
            if (isCarBraking)
            {
                // set the intensity of the brake lights to the on intensity
                brakeLightLeft.intensity = brakeLightOnIntensity;
                brakeLightRight.intensity = brakeLightOnIntensity;

            }
            else
            {
                // set the intensity of the brake lights to the off intensity
                brakeLightLeft.intensity = brakeLightOffIntensity;
                brakeLightRight.intensity = brakeLightOffIntensity;

            }

            // check if it is night time to turn on headlights and cone lights
            if (isNightTime)
            {
                // turn on headlights and cone lights
                headLightLeft.enabled = true;
                headLightRight.enabled = true;
                headConeLightLeft.enabled = true;
                headConeLightRight.enabled = true;

                // turn on brake lights
                brakeLightLeft.enabled = true;
                brakeLightRight.enabled = true;
            }
            else
            {
                // turn off headlights and cone lights
                headLightLeft.enabled = false;
                headLightRight.enabled = false;
                headConeLightLeft.enabled = false;
                headConeLightRight.enabled = false;

                // turn off brake lights
                brakeLightLeft.enabled = false;
                brakeLightRight.enabled = false;
            }
        }
    }

    public void SetBrakeCondition(bool brake)
    {

        isCarBraking = brake;
    }

    
}
