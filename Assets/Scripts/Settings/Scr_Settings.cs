using UnityEngine;

public class Scr_Settings : MonoBehaviour
{
    // This is the master settings script
    // here are the default settings for the game, they can be changed in the inspector or through code or UI 

    // graphics settings
    [SerializeField] private bool enableFullScreen = true;

    // Frame rate settings

    // do we set frame rate to refresh rate of the monitor?
    [SerializeField] private bool targetMonitorRefreshRate = true;

    [SerializeField] private double targetFrameRate = 60.0;
    [SerializeField] private double defaultFrameRate = 60.0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // if targetMonitorRefreshRate is true, set the target frame rate to the refresh rate of the monitor
        if (targetMonitorRefreshRate)
        {
            targetFrameRate = Screen.currentResolution.refreshRateRatio.value;
        }
        else 
        {
            targetFrameRate = defaultFrameRate;
        }

        // set the application target frame rate
        Application.targetFrameRate = (int)targetFrameRate;

    }

}
