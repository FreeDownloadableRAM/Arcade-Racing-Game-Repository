using UnityEngine;

public class Scr_Ion_Beam_Particle_Handler : MonoBehaviour
{
    // line renderer component
    private LineRenderer lineRenderer;
    public float laserDistance = 225f; // Maximum distance if nothing is hit
    public LayerMask hitLayers; // Define which layers the laser should hit

    // get particle system from child object
    [SerializeField] private ParticleSystem PartSysImpctEff;

    // destroy this object after a set time
    private float destructionTimer = 1f; // in seconds

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Line Renderer component attached to the GameObject
        lineRenderer = GetComponent<LineRenderer>();
        // Set the first point of the line renderer to the laser's position
        lineRenderer.SetPosition(0, transform.position);

        // get particle system from child object
        PartSysImpctEff = GetComponentInChildren<ParticleSystem>();

        // make sure the particle system is stopped at start
        // PartSysImpctEff.Stop();

    }

    void Update()
    {
        // Set the first point of the line renderer to the laser's position
        lineRenderer.SetPosition(0, transform.position);

        // Cast a ray forward from the laser's position
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, laserDistance, hitLayers))
        {
            // If the ray hits something, set the end position of the line renderer to the hit point
            lineRenderer.SetPosition(1, hit.point);

            // Move the particle system to the hit point and play it
            PartSysImpctEff.transform.position = hit.point;
            // play particle system if not already playing
            if (!PartSysImpctEff.isPlaying)
            {
                PartSysImpctEff.Play();
            }

        }
        else
        {
            // If the ray doesn't hit anything, set the end position to the max distance in the forward direction
            lineRenderer.SetPosition(1, transform.position + transform.forward * laserDistance);

            // turn off particle system if no hit
            PartSysImpctEff.Stop();
        }

        // increment destruction timer
        destructionTimer -= Time.deltaTime;
        // when timer hits zero, turn off line renderer
        if (destructionTimer <= 0f)
        {
            // turn off line renderer
            lineRenderer.enabled = false;
        }
    }
}
