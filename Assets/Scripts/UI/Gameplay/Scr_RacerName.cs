using UnityEngine;

public class Scr_RacerName : MonoBehaviour
{
    // This script takes the name of the parent game object its under
    // and sets the text of the TextMeshProUGUI component to that name
    [SerializeField] private GameObject parentGameObject;
    [SerializeField] private string racerName;

    // UI TextMeshProUGUI component to set the text of
    // this is in a child object
    [SerializeField] private GameObject racerNameTextObject;
    [SerializeField] private TMPro.TextMeshPro racerNameTextUI;

    // down triangle text
    [SerializeField] private GameObject downTriangleTextObject;
    [SerializeField] private TMPro.TextMeshPro downTriangleTextUI;

    // text control variables
    // Hide text based off the distance towards the camera
    // fade out, the closer the camera is to the close limit, 
    [SerializeField] private float showTextDistanceFarLimit = 30.0f;

    [SerializeField] private float showTextDistanceCloseLimit = 17.5f;

    [SerializeField] private float textFadeOutDistance = 5.0f;

    // get the camera object, we will use this to get the distance between the camera and the racer name text UI
    [SerializeField] private Camera mainCamera;

    // text vertical placement offset
    // some vehicles are taller than others, so we need to account for that.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get the name of the parent game object and set it to the racerName variable
        parentGameObject = transform.parent.gameObject;

        racerName = parentGameObject.name;

        // set the text of the TextMeshProUGUI component to the racerName variable
        // this object name of the component we are getting is "RacerName"
        racerNameTextObject = transform.Find("RacerName").gameObject;
        racerNameTextUI = racerNameTextObject.GetComponent<TMPro.TextMeshPro>();

        downTriangleTextObject = transform.Find("Arrow").gameObject;
        downTriangleTextUI = downTriangleTextObject.GetComponent<TMPro.TextMeshPro>();

        // set the text of the TextMeshProUGUI component to the racerName variable
        racerNameTextUI.text = racerName;

        // get the main camera object, we will use this to get the distance between the camera and the racer name text UI
        // its the object tagged as "MainCamera"
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // get the distance between the camera and the racer name text UI
        float distanceToCamera = Vector3.Distance(mainCamera.transform.position, transform.position);

        // rotate the text to face the camera, we only want to rotate on the y axis, so we will set the x and z rotation to 0
        racerNameTextUI.transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y, 0);
        
        // rotate the down triangle text to face the camera as well
        downTriangleTextUI.transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y, 0);

        // if the distance to the camera is greater than the showTextDistanceFarLimit, hide the text
        if (distanceToCamera > showTextDistanceFarLimit)
        {
            racerNameTextUI.enabled = false;
            downTriangleTextUI.enabled = false;

            
        }
        else if (distanceToCamera > showTextDistanceFarLimit - textFadeOutDistance)
        {
            float fadeInValue = Mathf.InverseLerp(showTextDistanceFarLimit, showTextDistanceFarLimit - textFadeOutDistance, distanceToCamera);

            // fade in the text the closer the camera is to our text object
            racerNameTextUI.alpha = fadeInValue;
            downTriangleTextUI.alpha = fadeInValue;

            racerNameTextUI.enabled = true;
            downTriangleTextUI.enabled = true;
        }
        else if (distanceToCamera < showTextDistanceCloseLimit)
        {
            // if the distance to the camera is less than the showTextDistanceCloseLimit, hide the text
            racerNameTextUI.enabled = false;
            downTriangleTextUI.enabled = false;
        }
        else
        {
            // if the distance to the camera is between the showTextDistanceFarLimit and showTextDistanceCloseLimit, show the text
            racerNameTextUI.enabled = true;
            downTriangleTextUI.enabled = true;
            // fade out the text based off the distance to the camera, we will use a linear interpolation to fade out the text
            float fadeOutValue = Mathf.InverseLerp(showTextDistanceCloseLimit, showTextDistanceFarLimit, distanceToCamera);
            racerNameTextUI.alpha = fadeOutValue;
            downTriangleTextUI.alpha = fadeOutValue;
        }

        
    }
}
