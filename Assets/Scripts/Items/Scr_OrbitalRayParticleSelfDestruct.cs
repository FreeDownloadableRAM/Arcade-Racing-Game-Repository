using UnityEngine;

public class Scr_OrbitalRayParticleSelfDestruct : MonoBehaviour
{
    // set self destruct timer value in seconds
    // from the inspector
    [SerializeField] private float selfDestructTimer = 5f;

    // get the particle system component of this object and its children
    [SerializeField] private ParticleSystem[] particleSystems;

    // on start, get the particle system component of this object and its children
    void Start()
    {
        // particleSystems = GetComponentsInChildren<ParticleSystem>();

        // play all particle systems in this object and its children
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // After the self destruct timer hits zero, destroy this object and its children
        if (selfDestructTimer <= 0f)
        {
            // destroy this object and its children
            Destroy(gameObject);
        }
        else
        {
            // count down self destruct timer
            selfDestructTimer -= Time.deltaTime;
        }
    }
}
