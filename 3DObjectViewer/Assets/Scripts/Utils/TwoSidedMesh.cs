using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;


//public class TwoSidedMesh :  EditorWindow
public class TwoSidedMesh :  MonoBehaviour
{
    Object _originalMeshObject;
    string _outputPath = "Assets/Meshes/";
    enum CombineOrder
    {
        InvertedThenOriginal,
        OriginalThenInverted
    }
    CombineOrder _combineOrder;

    //private void Start()
    //{
    //    Mesh originalMesh = GetComponent<MeshFilter>().mesh;
    //    if (originalMesh != null)
    //    {
    //        Mesh invertedMesh = CreateInvertedMesh(originalMesh);
    //        Mesh combinedMesh = CombineMeshes(ChangeUV3(originalMesh, 0), ChangeUV3(invertedMesh, 1));
    //        //string path = GetDoubleSidedMeshPath(originalMesh.name, combinedMesh, true);
    //        //SaveMesh(path, combinedMesh);


    //        GetComponent<MeshFilter>().sharedMesh = combinedMesh;
    //        originalMesh.RecalculateBounds();
    //    }
    //}

    private void Start()
    {
        int verts = 0;
        int tris = 0;
        
        MeshFilter[] originalMeshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (var meshFilter in originalMeshFilters)
        {

            tris += meshFilter.sharedMesh.triangles.Length / 3;
            verts += meshFilter.sharedMesh.vertexCount;

            Mesh originalMesh = meshFilter.mesh;
            if (originalMesh != null)
            {
                Mesh invertedMesh = CreateInvertedMesh(originalMesh);
                Mesh combinedMesh = CombineMeshes(ChangeUV3(originalMesh, 0), ChangeUV3(invertedMesh, 1));
                //string path = GetDoubleSidedMeshPath(originalMesh.name, combinedMesh, true);
                //SaveMesh(path, combinedMesh);

                meshFilter.sharedMesh = combinedMesh;
                meshFilter.mesh.RecalculateBounds();
            }
        }
        //AssetViewer.OnModelPolyCountEvent?.Invoke(tris, verts);
    }

    /*
    [MenuItem("Meshes/Double-Sided Mesh Generator")]
    public static void OpenWindow()
    {
        GetWindow<TwoSidedMesh>("Double-Sided Mesh Generator");
    }

    void OnGUI()
    {
        _originalMeshObject = EditorGUILayout.ObjectField(_originalMeshObject, typeof(Mesh), true);
        _outputPath = EditorGUILayout.TextField("Output path", _outputPath);
        _combineOrder = (CombineOrder)EditorGUILayout.EnumPopup("Combine order", _combineOrder);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate inverted"))
        {
            Mesh originalMesh = _originalMeshObject as Mesh;
            if (originalMesh != null)
            {
                Mesh invertedMesh = CreateInvertedMesh(originalMesh);
                string path = GetInvertedMeshPath(originalMesh.name, invertedMesh);
                SaveMesh(path, invertedMesh);
            }
        }

        if (GUILayout.Button("Generate double-sided"))
        {
            Mesh originalMesh = _originalMeshObject as Mesh;
            if (originalMesh != null)
            {
                Mesh invertedMesh = CreateInvertedMesh(originalMesh);
                Mesh combinedMesh = CombineMeshes(originalMesh, invertedMesh);
                string path = GetDoubleSidedMeshPath(originalMesh.name, combinedMesh);
                SaveMesh(path, combinedMesh);
            }
        }

        if (GUILayout.Button("Generate double-sided with UV3 = 1 for inverted mesh"))
        {
            Mesh originalMesh = _originalMeshObject as Mesh;
            if (originalMesh != null)
            {
                Mesh invertedMesh = CreateInvertedMesh(originalMesh);
                Mesh combinedMesh = CombineMeshes(ChangeUV3(originalMesh, 0), ChangeUV3(invertedMesh, 1));
                string path = GetDoubleSidedMeshPath(originalMesh.name, combinedMesh, true);
                SaveMesh(path, combinedMesh);
            }
        }
    }
    */

    Mesh CreateInvertedMesh(Mesh mesh)
    {
        Vector3[] normals = mesh.normals;
        Vector3[] invertedNormals = new Vector3[normals.Length];
        for (int i = 0; i < invertedNormals.Length; i++)
        {
            invertedNormals[i] = -normals[i];
        }
        Vector4[] tangents = mesh.tangents;
        Vector4[] invertedTangents = new Vector4[tangents.Length];

        for (int i = 0; i < invertedTangents.Length; i++)
        {
            invertedTangents[i] = tangents[i];
            invertedTangents[i].w = -invertedTangents[i].w;
        }

        return new Mesh
        {
            vertices = mesh.vertices,
            uv = mesh.uv,
            normals = invertedNormals,
            tangents = invertedTangents,
            triangles = mesh.triangles.Reverse().ToArray()
        };
    }

    Mesh ChangeUV3(Mesh mesh, float value)
    {
        Mesh result = Instantiate(mesh);

        Vector2[] uv3 = new Vector2[mesh.vertexCount];
        for (int i = 0; i < uv3.Length; i++)
        {
            uv3[i] = new Vector2(value, value);
        }

        result.uv3 = uv3;
        return result;
    }

    Mesh CombineMeshes(Mesh mesh, Mesh invertedMesh)
    {
        CombineInstance[] combineInstancies = new CombineInstance[2]
        {
            new CombineInstance(){mesh = invertedMesh, transform = Matrix4x4.identity},
            new CombineInstance(){mesh = mesh, transform = Matrix4x4.identity}
        };

        if (_combineOrder == CombineOrder.OriginalThenInverted)
        {
            combineInstancies = combineInstancies.Reverse().ToArray();
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstancies);
        return combinedMesh;
    }

    /*
    void SaveMesh(string path, Mesh mesh)
    {
        if (!Directory.Exists(_outputPath))
        {
            Directory.CreateDirectory(_outputPath);
        }
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    string GetDoubleSidedMeshPath(string name, Mesh mesh, bool uv3Modified = false)
    {
        string uv3 = (uv3Modified) ? "_UV3_" : "_";
        string order = (_combineOrder == CombineOrder.InvertedThenOriginal) ? "I_O_" : "O_I_";
        return _outputPath + "DoubleSidedMesh_" + name + uv3 + order + mesh.GetInstanceID() + ".asset";
    }

    string GetInvertedMeshPath(string name, Mesh mesh)
    {
        return _outputPath + "InvertedMesh_" + name + mesh.GetInstanceID() + ".asset";
    }
    */
}




//using UnityEngine;

//public class TwoSidedMesh : MonoBehaviour
//{
//    private void Start()
//    {
//        //var mesh = GetComponent<MeshFilter>().mesh;
//        var mesh = GetComponent<MeshFilter>().sharedMesh;
//        var vertices = mesh.vertices;
//        var uv = mesh.uv;
//        var normals = mesh.normals;
//        var szV = vertices.Length;
//        var newVerts = new Vector3[szV * 2];
//        var newUv = new Vector2[szV * 2];
//        var newNorms = new Vector3[szV * 2];
//        for (var j = 0; j < szV; j++)
//        {
//            // duplicate vertices and uvs:
//            newVerts[j] = newVerts[j + szV] = vertices[j];
//            newUv[j] = newUv[j + szV] = uv[j];
//            // copy the original normals...
//            newNorms[j] = normals[j];
//            // and revert the new ones
//            newNorms[j + szV] = -normals[j];
//        }
//        var triangles = mesh.triangles;
//        var szT = triangles.Length;
//        var newTris = new int[szT * 2]; // double the triangles
//        for (var i = 0; i < szT; i += 3)
//        {
//            // copy the original triangle
//            newTris[i] = triangles[i];
//            newTris[i + 1] = triangles[i + 1];
//            newTris[i + 2] = triangles[i + 2];
//            // save the new reversed triangle
//            var j = i + szT;
//            newTris[j] = triangles[i] + szV;
//            newTris[j + 2] = triangles[i + 1] + szV;
//            newTris[j + 1] = triangles[i + 2] + szV;
//        }
//        mesh.vertices = newVerts;
//        mesh.uv = newUv;
//        mesh.normals = newNorms;
//        mesh.triangles = newTris; // assign triangles last!
//        mesh.RecalculateBounds();
//    }
//}
