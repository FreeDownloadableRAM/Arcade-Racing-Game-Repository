using UnityEngine;

public class Scr_Item_Beam : MonoBehaviour
{
    // object duration timer
    private float destructionTimer; // in seconds

    // Update 
    void FixedUpdate()
    {
        // count down destruction timer
        destructionTimer -= Time.fixedDeltaTime;

        // destroy this object when timer hits zero
        if (destructionTimer <= 0f)
        {
            BeamFireOffEffect();
        }
    }

    // beam fire destruction effect function
    private void BeamFireOffEffect()
    {
        // destroy our laser object
        Destroy(transform.gameObject);
    }

    // set shield destruction timer value in seconds
    public void SetBeamFireDestructionTimer(float timerValue)
    {
        destructionTimer = timerValue;
    }
}
