using UnityEngine;

public class Scr_Item_Pierce_Laser_Impact : MonoBehaviour
{
    // timer until we destroy this object
    private float destroyTimer = 3f; // in seconds

    // Update is called once per frame
    void Update()
    {
        // count down the timer
        // once we hit zero, destroy this object
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
