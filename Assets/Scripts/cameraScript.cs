using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public Transform orientation;

    private Camera cam;

    public float rotationSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate the player in the direction the camera is looking
        Quaternion origin = orientation.transform.rotation;
        Quaternion target = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
        orientation.transform.rotation = Quaternion.Slerp(origin, target, Time.deltaTime * rotationSpeed);
    }
}