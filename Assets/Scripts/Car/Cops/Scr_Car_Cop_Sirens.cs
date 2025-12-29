using System.Runtime.CompilerServices;
using UnityEngine;
public class Scr_Car_Cop_Sirens : MonoBehaviour
{
    // get siren lights from child objects
    [SerializeField] private Light sirenTopBlueLight;
    [SerializeField] private Light sirenTopBlueLightMiddle;

    [SerializeField] private Light sirenTopRedLight;
    [SerializeField] private Light sirenTopRedLightMiddle;

    // On and off duration for the lights
    [SerializeField] private float lightOnDuration;
    [SerializeField] private float lightOffDuration;

    // Get cop car target handler script
    [SerializeField] private scr_Car_Cop_Target_Handler copCarTargetHandlerScript;

    // Support lights that are always on while in chase mode
    [SerializeField] private Light[] supportLightsArrayRed;
    [SerializeField] private Light[] supportLightsArrayBlue;

    // siren flash duration
    [SerializeField] private float sirenFlashDuration;
    [SerializeField] private float sirenPauseFlashDuration;

    // siren internal timer
    private float sirenFlashTimer;
    private float sirenPauseFlashTimer;

    // switch sides of siren on this number of cycles
    [SerializeField] private int sirenSwitchSidesCycles;

    // determine which side is currently active
    private bool isSirenRedSideActive = true;

    // so we can flash one side at a time
    private bool turnLightOn = true;


    // set renderer component
    [SerializeField] private Renderer sirenRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // car cop target handler script
        if (copCarTargetHandlerScript == null)
        {
            copCarTargetHandlerScript = GetComponent<scr_Car_Cop_Target_Handler>();
        }

        // turn off the support lights if not in chase state
        foreach (Light supportLight in supportLightsArrayRed)
        {
            supportLight.enabled = false;
        }

        // turn off the support lights if not in chase state
        foreach (Light supportLight in supportLightsArrayBlue)
        {
            supportLight.enabled = false;
        }

        

    }

    // Update is called once per frame
    void Update()
    {
        // if we are in a chase state, flash the sirens
        if (copCarTargetHandlerScript.AIState == "Chase")
        {
            FlashSirens();

        }
        else
        {
            // turn off the lights if not in chase state
            sirenTopBlueLight.enabled = false;
            sirenTopBlueLightMiddle.enabled = false;
            sirenTopRedLight.enabled = false;
            sirenTopRedLightMiddle.enabled = false;

            foreach (Light supportLight in supportLightsArrayBlue)
            {
                supportLight.enabled = false;
            }

            foreach (Light supportLight in supportLightsArrayRed)
            {
                supportLight.enabled = false;
            }

        }

    }

    // Flash the sirens on and off function
    void FlashSirens() 
    {
        /*
        // turn on and off the lights based on the duration
        // in an alternating pattern
        if (Time.time % (lightOnDuration + lightOffDuration) < lightOnDuration)
        {
            sirenTopBlueLight.enabled = true;
            sirenTopBlueLightMiddle.enabled = true;
            sirenTopRedLight.enabled = false;
            sirenTopRedLightMiddle.enabled = false;

            foreach (Light supportLight in supportLightsArrayRed)
            {
                supportLight.enabled = false;
            }

            foreach (Light supportLight in supportLightsArrayBlue)
            {
                supportLight.enabled = true;
            }
        }
        else
        {
            sirenTopBlueLight.enabled = false;
            sirenTopBlueLightMiddle.enabled = false;
            sirenTopRedLight.enabled = true;
            sirenTopRedLightMiddle.enabled = true;

            foreach (Light supportLight in supportLightsArrayRed)
            {
                supportLight.enabled = true;
            }

            foreach (Light supportLight in supportLightsArrayBlue)
            {
                supportLight.enabled = false;
            }
        }
        */

        // red side is active, blue lights are off
        if (isSirenRedSideActive)
        {
            
            // turn all blue lights off
            sirenTopBlueLight.enabled = false;
            sirenTopBlueLightMiddle.enabled = false;

            // support blue lights off
            foreach (Light supportLight in supportLightsArrayBlue)
            {
                supportLight.enabled = false;
            }
            // support red lights on
            foreach (Light supportLight in supportLightsArrayRed)
            {
                supportLight.enabled = true;
            }

            // turn on all red lights
            if (turnLightOn)
            {
                // turn on red lights
                sirenTopRedLight.enabled = true;
                sirenTopRedLightMiddle.enabled = true;
                /*
                foreach (Light supportLight in supportLightsArrayRed)
                {
                    supportLight.enabled = true;
                }
                */
                // increment flash timer
                sirenFlashTimer += Time.deltaTime;

                if (sirenFlashTimer >= sirenFlashDuration)
                {
                    // reset flash timer
                    sirenFlashTimer = 0f;
                    turnLightOn = false;
                }
            }
            else
            {
                // turn off red lights
                sirenTopRedLight.enabled = false;
                sirenTopRedLightMiddle.enabled = false;
                /*
                foreach (Light supportLight in supportLightsArrayRed)
                {
                    supportLight.enabled = false;
                }
                */
                // increment pause flash timer
                sirenPauseFlashTimer += Time.deltaTime;

                if (sirenPauseFlashTimer >= sirenPauseFlashDuration)
                {
                    // reset pause flash timer
                    sirenPauseFlashTimer = 0f;

                    // decrement cycle counter
                    sirenSwitchSidesCycles--;
                    turnLightOn = true;
                }

            }

            if (sirenSwitchSidesCycles == 0)
            {
                // switch sides
                isSirenRedSideActive = false;

                // reset cycle counter
                sirenSwitchSidesCycles = 2;

                // reset timers
                sirenFlashTimer = 0f;
                sirenPauseFlashTimer = 0f;

            }
        }
        else 
        {
            
            // turn all red lights off
            sirenTopRedLight.enabled = false;
            sirenTopRedLightMiddle.enabled = false;

            // support red lights off
            foreach (Light supportLight in supportLightsArrayRed)
            {
                supportLight.enabled = false;
            }

            foreach (Light supportLight in supportLightsArrayBlue)
            {
                supportLight.enabled = true;
            }

            // turn on all blue lights
            if (turnLightOn)
            {
                // turn on blue lights
                sirenTopBlueLight.enabled = true;
                sirenTopBlueLightMiddle.enabled = true;
                /*
                foreach (Light supportLight in supportLightsArrayBlue)
                {
                    supportLight.enabled = true;
                }
                */
                // increment flash timer
                sirenFlashTimer += Time.deltaTime;

                if (sirenFlashTimer >= sirenFlashDuration)
                {
                    // reset flash timer
                    sirenFlashTimer = 0f;
                    turnLightOn = false;
                }
            }
            else
            {
                // turn off blue lights
                sirenTopBlueLight.enabled = false;
                sirenTopBlueLightMiddle.enabled = false;
                /*
                foreach (Light supportLight in supportLightsArrayBlue)
                {
                    supportLight.enabled = false;
                }
                */
                // increment pause flash timer
                sirenPauseFlashTimer += Time.deltaTime;

                if (sirenPauseFlashTimer >= sirenPauseFlashDuration)
                {
                    // reset pause flash timer
                    sirenPauseFlashTimer = 0f;

                    // decrement cycle counter
                    sirenSwitchSidesCycles--;
                    turnLightOn = true;
                }

            }

            if (sirenSwitchSidesCycles == 0)
            {
                // switch sides
                isSirenRedSideActive = true;

                // reset cycle counter
                sirenSwitchSidesCycles = 2;

                // reset timers
                sirenFlashTimer = 0f;
                sirenPauseFlashTimer = 0f;

            }

        }

    }
}


