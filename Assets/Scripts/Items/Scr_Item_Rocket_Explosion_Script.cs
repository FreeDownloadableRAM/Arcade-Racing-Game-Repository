using UnityEngine;

public class Scr_Item_Rocket_Explosion_Script : MonoBehaviour
{

    // object lifetime in seconds
    [SerializeField] private float lifetime = 2.0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        // count down lifetime
        // once lifetime hits zero, destroy the explosion object
        lifetime -= Time.fixedDeltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
