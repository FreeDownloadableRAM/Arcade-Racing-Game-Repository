using UnityEngine;

public class Scr_Palm_Tree : MonoBehaviour
{

    // get palm tree rigidbody component of its children and store it in an array
    [SerializeField] private Rigidbody[] palmTreeCocoNutRBs;

    // get the colliders of the palm tree and its children and store them in an array
    // [SerializeField] private Collider[] palmTreeColliders;

    // drop coconut variable to check if we need to drop the coconut or not
    private bool dropCoconut = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get all rigidbody components of the children of the palm tree and store them in the array
        palmTreeCocoNutRBs = GetComponentsInChildren<Rigidbody>();

        // get all colliders of the palm tree and its children and store them in the array
        // palmTreeColliders = GetComponentsInChildren<Collider>();

        // set all rigidbody components of the children of the palm tree to kinematic so they won't be affected by physics until we need to drop the coconut
        foreach (Rigidbody rb in palmTreeCocoNutRBs)
        {
            rb.isKinematic = true;
        }
    }

    // check if anything collided with our tree colliders with a high enough force to drop the coconut.
    private void OnCollisionEnter(Collision collision)
    {
        //  check each collider of the palm tree and its children to see if another object collided with it and if the collision force is high enough to drop the coconut
        
        if (collision.relativeVelocity.magnitude > 5f)
        {
            dropCoconut = true;
                
        }
        

    }

    private void OnCollisionExit(Collision collision) 
    {
        // if we collide with a car object or we will enable the rigidbody components of the palm tree and its children and add a force to them to make them fall down
        if (dropCoconut == true)
        {
            foreach (Rigidbody rb in palmTreeCocoNutRBs)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
            }
        }
    }
}
