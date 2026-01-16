using UnityEngine;

public class Scr_Item_Shield : MonoBehaviour
{
    // self destruction timer
    private float destructionTimer; // in seconds

    // Update 
    void FixedUpdate()
    {
        // count down destruction timer
        destructionTimer -= Time.fixedDeltaTime;

        // destroy flamethrower when timer hits zero
        if (destructionTimer <= 0f)
        {
            ShieldTurnOffEffect();
        }
    }

    // flamethrower destruction effect function
    private void ShieldTurnOffEffect()
    {
        
        // destroy our laser object
        Destroy(transform.parent.gameObject);
    }

    // set shield destruction timer value in seconds
    public void SetShieldDestructionTimer(float timerValue)
    {
        destructionTimer = timerValue;
    }

}
