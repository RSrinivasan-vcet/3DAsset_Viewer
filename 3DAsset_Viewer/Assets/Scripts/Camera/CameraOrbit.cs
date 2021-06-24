using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform targetPosition;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 90.0f;

    //public float distanceMin = .5f;
    //public float distanceMax = 15f;

    private float FOVIn = 15;
    private float FOVOut = 60;

    float x = 0.0f;
    float y = 0.0f;

    void Start()
    {
        ResetX();
    }

    private void ResetX()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }
    public Camera myCamera;
    public float zoomSpeedScroll = 10f;
    //public MinMaxLimits zoomLimit = new MinMaxLimits { min = 10, max = 60 };

    void LateUpdate()
    {
        if (targetPosition)
        {
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02f;

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                RaycastHit hit;
                if (Physics.Linecast(targetPosition.position, transform.position, out hit))
                {
                    distance -= hit.distance;
                }
                if (y >= 90)
                    y = 90;
                else if (y <= -90)
                    y = -90;
                
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + targetPosition.position;

                transform.rotation = rotation;
                transform.position = position;

                
            }
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        //Mouse Scroll Zoom
        if (scroll != 0)
            ZoomCameraScroll(scroll, zoomSpeedScroll);
    }

    public void ResetBtn()
    {
        transform.position = new Vector3(0, 0.451f, -0.641f);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        myCamera.fieldOfView = 60;
        ResetX();
    }
    private void ZoomCameraScroll(float offset, float speed)
    {
        print("Scroll");

        myCamera.fieldOfView = Mathf.Clamp(myCamera.fieldOfView - (offset * speed), FOVIn, FOVOut);
       
    }
}
