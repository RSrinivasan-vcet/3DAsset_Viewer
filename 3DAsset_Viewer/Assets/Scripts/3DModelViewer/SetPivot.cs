using UnityEngine;

public class SetPivot : MonoBehaviour
{    
    private void Start()
    {
        transform.localScale = Vector3.one;
        Bounds bounds = GetRealSize(gameObject);
        print("bounds : " + bounds);
        print("bounds.center : " + bounds.center);
        PivotTo(bounds.center);
    }

    public void PivotTo(Vector3 position)
    {
        Vector3 offset = transform.position - position;
        foreach (Transform child in transform)
            child.transform.position += offset;
        transform.position = position;
        
    }

    public Bounds GetRealSize(GameObject parent)
    {
        MeshFilter[] childrens = parent.GetComponentsInChildren<MeshFilter>();
        //print("childrens.Length : " + childrens.Length);
        //print(childrens[0].mesh.bounds.size + "*" + childrens[0].transform.localScale + " / 2 + " + childrens[0].transform.position);

        Vector3 minV = childrens[0].transform.position - MultVect(childrens[0].mesh.bounds.size, childrens[0].transform.localScale) / 2;
        Vector3 maxV = childrens[0].transform.position + MultVect(childrens[0].mesh.bounds.size, childrens[0].transform.localScale) / 2;
        //Debug.Log(maxV);
        for (int i =1; i < childrens.Length; i++)
        {
            maxV = Vector3.Max(maxV, childrens[i].transform.position + MultVect(childrens[i].mesh.bounds.size, childrens[i].transform.localScale) / 2);
            minV = Vector3.Min(minV, childrens[i].transform.position - MultVect(childrens[i].mesh.bounds.size, childrens[i].transform.localScale) / 2);
          //  Debug.Log(maxV);
        }
        Vector3 v3 = maxV - minV;
        
        return new Bounds(minV + v3 / 2, v3);
    }

    private Vector3 MultVect(Vector3 a, Vector3 b)
    {
        a.x *= b.x;
        a.y *= b.y;
        a.z *= b.z;
        return a;
    }
}