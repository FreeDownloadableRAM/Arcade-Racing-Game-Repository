using UnityEngine;

public class Scr_Camera_Player : MonoBehaviour
{

    // target for the camera to follow
    [SerializeField] private Transform target;

    // offset from the target
    [SerializeField] private Vector3 offset;

    // smooth time for camera movement
    [SerializeField] float smoothTime;

    // camera velocity
    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void LateUpdate()
    {
        // get target vector position
        Vector3 targetPosition = target.position + offset;

        // set this camera position to follow the target position with smooth damp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // set camera to smoothly look at the target
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), smoothTime);

    }
}
