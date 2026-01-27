using UnityEngine;

public class Scr_Item_Beam_Charge : MonoBehaviour
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
            BeamChargeOffEffect();
        }
    }

    // beam charge destruction effect function
    private void BeamChargeOffEffect()
    {
        // destroy our laser object
        Destroy(transform.gameObject);
    }

    // set shield destruction timer value in seconds
    public void SetBeamChargeDestructionTimer(float timerValue)
    {
        destructionTimer = timerValue;
    }
}
