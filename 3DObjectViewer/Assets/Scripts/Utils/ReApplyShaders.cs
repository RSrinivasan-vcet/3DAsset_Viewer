using UnityEngine;
using System.Collections;


public class ReApplyShaders : MonoBehaviour
{
    public Renderer[] renderers;
    public Material[] materials;
    //public string[] shaders;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        foreach (var rend in renderers)
        {
            materials = rend.sharedMaterials;
            foreach (Material m in materials)
            {
                if (m)
                {
                    var shaderName = m.shader.name;
                    var newShader = Shader.Find(shaderName);
                    if (!shaderName.Equals("Standard"))
                    {
                        if (newShader != null)
                        {
                            m.shader = newShader;
                        }
                        else
                        {
                            Debug.LogWarning("unable to refresh shader: " + shaderName + " in material " + m.name);
                        }
                    }
                }
            }
        }
    }

    //void Start()
    //{
    //    //foreach (var rend in renderers)
    //    //{
    //    //    materials = rend.sharedMaterials;
    //    //    shaders = new string[materials.Length];

    //    //    for (int i = 0; i < materials.Length; i++)
    //    //    {
    //    //        shaders[i] = materials[i].shader.name;
    //    //    }

    //    //    for (int i = 0; i < materials.Length; i++)
    //    //    {
    //    //        materials[i].shader = Shader.Find(shaders[i]);
    //    //    }
    //    //}
    //   
    //}
}