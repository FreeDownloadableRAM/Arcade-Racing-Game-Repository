using UnityEngine;

public class Scr_Item_Flamethrower : MonoBehaviour
{
    // damage amount of the flamethrower per second
    // this is per projectile
    [SerializeField] private int damageAmountPerSecond;

    // self destruction timer
    [SerializeField] private float destructionTimer = 7f; // in seconds

    // particle spawn duration
    [SerializeField] private float particleSpawnDuration = 5f; // in seconds

    // Update 
    void FixedUpdate()
    {
        // count down destruction timer
        destructionTimer -= Time.fixedDeltaTime;

        // count down particle spawn duration
        // if particle spawn duration is greater than zero
        if ( particleSpawnDuration >= 0)
        {
            particleSpawnDuration -= Time.fixedDeltaTime;
        }
        
        // destroy flamethrower when timer hits zero
        if (destructionTimer <= 0f)
        {
            FlamethrowerTurnOffEffect();
        }
    }

    // return the damage amount the flamethrower does on impact
    public int GetFlamethrowerDamagePerSecondAmount()
    {
        return damageAmountPerSecond;
    }

    // return the current destruction timer value
    public float GetDestructionTimerValue()
    {
        return destructionTimer;
    }

    // return the particle spawn duration value
    public float GetParticleSpawnDurationValue()
    {
        return particleSpawnDuration;
    }

    // flamethrower destruction effect function
    private void FlamethrowerTurnOffEffect()
    {
        // destroy our laser object
        Destroy(transform.parent.gameObject);
    }
}
