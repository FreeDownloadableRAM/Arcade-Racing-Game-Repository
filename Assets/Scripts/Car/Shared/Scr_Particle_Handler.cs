using UnityEngine;

public class Scr_Particle_Handler : MonoBehaviour
{
    // reference the particle system for exhausts
    [SerializeField] private ParticleSystem exhaustParticleLeft;
    [SerializeField] private ParticleSystem exhaustParticleRight;

    // reference the particle systems for nitros
    [SerializeField] private ParticleSystem nitroParticleLeft;
    [SerializeField] private ParticleSystem nitroParticleRight;

    // reference the particle system for healing
    [SerializeField] private ParticleSystem healParticles;

    // reference the particle system for damage states
    [SerializeField] private ParticleSystem lowDamageStateParticles;
    [SerializeField] private ParticleSystem mediumDamageStateParticles;
    [SerializeField] private ParticleSystem highDamageStateParticles;
    [SerializeField] private ParticleSystem severeDamageStateParticles;

    // reference car destruction particles
    [SerializeField] private ParticleSystem carDestructionParticlesFlare;
    [SerializeField] private ParticleSystem carDestructionParticlesDebris;

    // reference rigidbody to determine our speed
    private Rigidbody carRigidbody;

    // reference item handler to check if nitro is active
    private Scr_Item_Handler scr_ItemHandler;

    // reference to car health to determine damage state
    private Scr_Car_Health scr_CarHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get rigidbody component
        carRigidbody = GetComponent<Rigidbody>();

        // get item handler component
        scr_ItemHandler = GetComponent<Scr_Item_Handler>();

        // get car health component
        scr_CarHealth = GetComponent<Scr_Car_Health>();

        // stop particle systems initially
        exhaustParticleLeft.Stop();
        exhaustParticleRight.Stop();
        nitroParticleLeft.Stop();
        nitroParticleRight.Stop();

        // heal particles stop
        healParticles.Stop();

        // damage state particles stop
        lowDamageStateParticles.Stop();
        mediumDamageStateParticles.Stop();
        highDamageStateParticles.Stop();
        severeDamageStateParticles.Stop();

        // stop destruction particles
        carDestructionParticlesFlare.Stop();
        carDestructionParticlesDebris.Stop();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // determine if we should be creating exhaust particles based on speed
        if (carRigidbody.linearVelocity.magnitude < 20f)
        {
            // play exhaust particles if not already playing
            if (!exhaustParticleLeft.isPlaying)
            {
                exhaustParticleLeft.Play();
            }
            if (!exhaustParticleRight.isPlaying)
            {
                exhaustParticleRight.Play();
            }
        }
        else
        {
            // stop exhaust particles if speed is low
            if (exhaustParticleLeft.isPlaying)
            {
                exhaustParticleLeft.Stop();
            }
            if (exhaustParticleRight.isPlaying)
            {
                exhaustParticleRight.Stop();
            }
        }

        // if nitro is active, play nitro particles
        if (scr_ItemHandler.isNitroActive())
        {
            // play nitro particles if not already playing
            if (!nitroParticleLeft.isPlaying)
            {
                nitroParticleLeft.Play();
            }
            if (!nitroParticleRight.isPlaying)
            {
                nitroParticleRight.Play();
            }

        }
        else 
        {
            // stop nitro particles if nitro not active
            if (nitroParticleLeft.isPlaying)
            {
                nitroParticleLeft.Stop();
            }
            if (nitroParticleRight.isPlaying)
            {
                nitroParticleRight.Stop();
            }

        }

        // determine damage state particles based on health
        if (((float)scr_CarHealth.GetCurrentHealth() / (float)scr_CarHealth.GetMaxHealth()) > 0.8)
        {
            // dont play any damage particles
            lowDamageStateParticles.Stop();
            mediumDamageStateParticles.Stop();
            highDamageStateParticles.Stop();
            severeDamageStateParticles.Stop();

        }
        else if (((float)scr_CarHealth.GetCurrentHealth() / (float)scr_CarHealth.GetMaxHealth()) > 0.6)
        {
            // play light damage particles
            if (!lowDamageStateParticles.isPlaying) 
            { 
                lowDamageStateParticles.Play();
            }

            // turn off other damage particles
            mediumDamageStateParticles.Stop();
            highDamageStateParticles.Stop();
            severeDamageStateParticles.Stop();
        }
        else if (((float)scr_CarHealth.GetCurrentHealth() / (float)scr_CarHealth.GetMaxHealth()) > 0.4)
        {
            // play light damage particles
            if (!lowDamageStateParticles.isPlaying)
            {
                lowDamageStateParticles.Play();
            }

            // play medium damage particles
            if (!mediumDamageStateParticles.isPlaying) 
            { 
                mediumDamageStateParticles.Play();
            }

            // turn off other damage particles
            highDamageStateParticles.Stop();
            severeDamageStateParticles.Stop();
        }
        else if (((float)scr_CarHealth.GetCurrentHealth() / (float)scr_CarHealth.GetMaxHealth()) > 0.2)
        {
            // play light damage particles
            if (!lowDamageStateParticles.isPlaying)
            {
                lowDamageStateParticles.Play();
            }

            // play medium damage particles
            if (!mediumDamageStateParticles.isPlaying)
            {
                mediumDamageStateParticles.Play();
            }

            // play high damage particles
            if (!highDamageStateParticles.isPlaying) 
            { 
                highDamageStateParticles.Play();
            }

            // turn off other damage particles
            severeDamageStateParticles.Stop();

        }
        else 
        {
            
            // play light damage particles
            if (!lowDamageStateParticles.isPlaying)
            {
                lowDamageStateParticles.Play();
            }

            // play medium damage particles
            if (!mediumDamageStateParticles.isPlaying)
            {
                mediumDamageStateParticles.Play();
            }

            // play high damage particles
            if (!highDamageStateParticles.isPlaying)
            {
                highDamageStateParticles.Play();
            }

            // play severe damage particles
            if (!severeDamageStateParticles.isPlaying) 
            { 
                severeDamageStateParticles.Play();
            }

        }

        // debug health percentage
        // Debug.Log(gameObject.name + "'s Health Percentage: " + ((float)scr_CarHealth.GetCurrentHealth() / (float)scr_CarHealth.GetMaxHealth()) * 100f + "%");

    }

    // play heal particles
    public void PlayHealParticles()
    {
        
        // play heal particles
        healParticles.Play();
        
    }

    // play car destruction particles
    public void PlayCarDestructionParticles()
    {
        // play destruction particles
        carDestructionParticlesFlare.Play();
        carDestructionParticlesDebris.Play();
    }

    public void StopCarDestructionParticles()
    {
        // stop destruction particles
        carDestructionParticlesFlare.Stop();
        carDestructionParticlesDebris.Stop();
    }

}
