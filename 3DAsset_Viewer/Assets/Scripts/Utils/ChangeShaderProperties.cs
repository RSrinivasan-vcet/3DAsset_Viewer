using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeShaderProperties : MonoBehaviour
{
    public enum RenderMode
    {
        None,
        Opaque,
        Cutout,
        Fade,
        Transparent,
        UnlitTexture,
    };

    public RenderMode renderMode = RenderMode.Fade;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (GetComponent<Renderer>() && renderMode != RenderMode.None)
        {
            Material m = GetComponent<Renderer>().material;
            m.shader = Shader.Find("Standard");
            yield return new WaitForSeconds(0.1f);
            m.shader = Shader.Find("Standard");
            Material newMat = m;

            this.GetComponent<Renderer>().material = newMat;
          

            ChangeRenderMode(newMat, renderMode);
        }
    }

    // Update is called once per frame
    public void ChangeRenderMode(Material material, RenderMode renderMode)
    {
        switch (renderMode)
        {
            case RenderMode.Opaque:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case RenderMode.Cutout:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case RenderMode.Fade:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case RenderMode.Transparent:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;

            case RenderMode.UnlitTexture:
                material.shader = Shader.Find("Unlit/Texture");
                break;
        }

    }
}
