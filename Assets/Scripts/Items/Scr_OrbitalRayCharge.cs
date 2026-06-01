using UnityEngine;

public class Scr_OrbitalRayCharge : MonoBehaviour
{
    // get particle system component of this object and its children
    [SerializeField] private ParticleSystem[] particleSystems;

    // get self destruct timer from inspector ins seconds
    [SerializeField] private float selfDestructTimer = 5f;

    // get orbital ray fire object from inspector to spawn at the end of the charge effect
    [SerializeField] private GameObject orbitalRayFirePrefab;

    // track if we spawned an orbital ray fire object yet
    private bool hasSpawnedOrbitalRayFire = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        // play all particle systems in this object and its children
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        selfDestructTimer -= Time.deltaTime;

        // count down self destruct timer
        // once it reaches zero, destroy this object and its children
        if (selfDestructTimer <= 0f)
        {
            // destroy this object and its children
            Destroy(gameObject);

        }
        // disable collider shortly after the orbital ray goes off
        else if (selfDestructTimer <= 3.75f) 
        { 
            // disable our collider
            GetComponent<Collider>().enabled = false;
        }
        else if (selfDestructTimer <= 4f)
        {
            // only spawn one of these, so check if we already spawned one before spawning another one
            if (hasSpawnedOrbitalRayFire == false)
            {
                // create orbital ray fire object at this position and rotation
                Instantiate(orbitalRayFirePrefab, transform.position, transform.rotation);
                hasSpawnedOrbitalRayFire = true;

            }

        }
        else
        {
            // move this object across the ground
        }
    }
}
