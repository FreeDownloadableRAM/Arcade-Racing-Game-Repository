using UnityEngine;

public class Scr_Item_Ion_Beam : MonoBehaviour
{
    // object duration timer
    private float destructionTimer; // in seconds

    // damage per second
    [SerializeField] private int IonBeamDPS;

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

    // return the damage amount the flamethrower does on impact
    public int GetIonBeamDPS()
    {
        return IonBeamDPS;
    }

    // beam fire destruction effect function
    private void BeamFireOffEffect()
    {
        // destroy our laser object
        Destroy(transform.parent.gameObject);
    }

    // set shield destruction timer value in seconds
    public void SetBeamFireDestructionTimer(float timerValue)
    {
        destructionTimer = timerValue;
    }
}
