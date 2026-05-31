using UnityEngine;

public class Scr_OrbitalRayFire : MonoBehaviour
{
    // self destruct timer in seconds
    [SerializeField] private float selfDestructTimer = 0.2f;

    // get orbital ray impact object from inspector to spawn at the end of the fire effect
    [SerializeField] private GameObject orbitalRayFireImpactEffectObject;

    // get line renderer component of this object
    private LineRenderer lineRenderer;

    // track if we spawned an orbital ray Impact object yet
    private bool hasSpawnedOrbitalRayImpact = false;

    // orbital ray damage per second
    [SerializeField] private int orbitalRayDPS = 1200;

    // in create event get line renderer component of this object, and set its first position to this object position
    // and its second 30 units above this object position

    void Start()
    {
        // get line renderer component of this object
        lineRenderer = GetComponent<LineRenderer>();

        // set line renderer positions to create a vertical line 60 units long starting from this object's position
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + Vector3.up * 60f);

        // start line alpha at 0 then 1, so it looks like the line is firing up from the ground
        //lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // fade in line alpha from 0 to 1 over the entire object duration, so it looks like the line is firing up from the ground
        //lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, 1f - (selfDestructTimer / 0.2f));

        // count down self destruct timer
        // destroy this object when it reaches zero and spawn the impact effect object at this position and rotation
        if (selfDestructTimer <= 0f)
        {
            
            // destroy this object and its children
            Destroy(gameObject);
        }
        else if (selfDestructTimer <= 0.15f)
        {
            // only spawn one of these, so check if we already spawned one before spawning another one
            if (hasSpawnedOrbitalRayImpact == false)
            {
                // spawn the impact effect object at this position and rotation
                Instantiate(orbitalRayFireImpactEffectObject, transform.position, transform.rotation);
                hasSpawnedOrbitalRayImpact = true;

            }

            selfDestructTimer -= Time.deltaTime;
        }
        else
        {
            selfDestructTimer -= Time.deltaTime;
        }
    }

    // return the damage amount the orbital ray does on impact
    public int GetOrbitalRayDPS()
    {
        return orbitalRayDPS;
    }
}
