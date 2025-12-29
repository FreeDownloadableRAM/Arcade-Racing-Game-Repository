using UnityEngine;

public class Scr_Item_Box : MonoBehaviour
{

    // rigidbody component of the box
    private Rigidbody rb;

    // rotation speed modifier
    [SerializeField] private float rotationSpeed;

    // internal Item Box Counter
    [SerializeField] private float internalBoxCD;

    // the item box cool down
    [SerializeField] private float itemBoxCD;

    // shrink size 
    [SerializeField] private Vector3 shrinkSize;

    // internal tracker, see if item is Available
    private bool itemAvailable = true;

    // ai checkpoint helper
    // if we are moving towards this checkpoint, consider this item box for collection
    [SerializeField] private int aiItemBoxTargetCheckpointRequirment;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get rigidbody component
        rb = GetComponent<Rigidbody>();

        // set internal box cool down to item box cool down
        internalBoxCD = itemBoxCD;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // rotate the box around all its axis
        rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0.5f, 1f, 0.5f) * rotationSpeed));

        // count down internal timer until it hits zero
        if (!itemAvailable)
        {
            //shrink the box
            transform.localScale = shrinkSize;

            internalBoxCD -= Time.fixedDeltaTime;
            // when internal timer hits zero, reset the box
            if (internalBoxCD <= 0f)
            {
                // reset the box scale
                transform.localScale = Vector3.one;
                // set item available to true
                itemAvailable = true;
                // reset internal box cool down
                internalBoxCD = itemBoxCD;
            }
        }

        
    }

    // return if item is available
    public bool IsItemAvailable()
    {

        return itemAvailable;
    }

    // set item available
    public void SetItemAvailable(bool availability)
    {
        itemAvailable = availability;
    }

    public int GetAIItemBoxTargetCheckpointRequirment()
    {
        return aiItemBoxTargetCheckpointRequirment;
    }

}





