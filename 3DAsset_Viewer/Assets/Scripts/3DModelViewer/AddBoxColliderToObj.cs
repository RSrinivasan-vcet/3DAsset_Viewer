using System;
using UnityEngine;

public class AddBoxColliderToObj : MonoBehaviour
{
    public static Action ColliderAddedSucess;
    private void Start()
    {
        gameObject.transform.localScale = new Vector3(1,1,1);
        AddBoxCollider();
    }
    /// <summary>
    /// Add mesh collider to game object
    /// Gets all child components, looks for meshes and assings
    /// them to gameobject meshcollider
    /// </summary>
    private void AddBoxCollider()
    {
        gameObject.AddComponent<BoxCollider>();

        BoxCollider collider = gameObject.GetComponent<BoxCollider>();
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter f in filters)
        {
            bounds.Encapsulate(f.sharedMesh.bounds);
            bounds.center = Vector3.zero;
        }
        collider.size = bounds.size;
        collider.center = bounds.center;

        ColliderAddedSucess?.Invoke();
    }
}