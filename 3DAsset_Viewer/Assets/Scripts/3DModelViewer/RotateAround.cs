using UnityEngine;

public class RotateAround : MonoBehaviour
{

    public Transform pivot;
    public float degreesPerSecond=10f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            transform.RotateAround(transform.position, Vector3.up, degreesPerSecond * Time.deltaTime);
        }
    }
}