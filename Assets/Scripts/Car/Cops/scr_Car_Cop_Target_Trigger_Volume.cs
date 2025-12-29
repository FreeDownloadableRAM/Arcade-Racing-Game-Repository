using UnityEngine;

public class scr_Car_Cop_Target_Trigger_Volume : MonoBehaviour
{
    // game object that entered the trigger volume
    public GameObject targetedGameObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // get collider trigger enter event
    private void OnTriggerEnter(Collider other)
    {
        // check if the object that entered the trigger volume has the tag "Racer"
        if (other.CompareTag("Cars"))
        {
            // set this racer as the targeted game object
            targetedGameObject = other.gameObject;

            

        }
        if (other.CompareTag("PlayerCar"))
        {
            // set this racer as the targeted game object
            targetedGameObject = other.gameObject;



        }
    }


}
