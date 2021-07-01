using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class ObjectRotator1 : MonoBehaviour
{
    public static Action ResetRotationEvent = null;
    float sensitivity = 20f;
    Quaternion ObjRotation;//= Quaternion.identity;

    private void Start()
    {
        ObjRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        ResetRotationEvent += ResetObjRot;
    }
    private void OnDisable()
    {
        ResetRotationEvent -= ResetObjRot;
    }

    public void ResetObjRot()
    {
        transform.localRotation = ObjRotation;
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
            if (Input.GetMouseButton(0))
            {
                float rotationX = Mathf.Deg2Rad * Input.GetAxis("Mouse X") * sensitivity * 10;
                float rotationY = Mathf.Deg2Rad * Input.GetAxis("Mouse Y") * sensitivity * 10;

                //print("rotationX : " + -rotationX);
                //print("rotationY : " + rotationY);

                //Rotate the object around the camera's "up" axis, and the camera's "right" axis.
                transform.Rotate(Vector3.up, -rotationX, Space.World);
                transform.Rotate(Vector3.right, rotationY, Space.World);

                //transform.RotateAround(centerPos, Vector3.up, -rotationX);
                //transform.RotateAround(centerPos, Vector3.right, rotationY);
            }
    }
}