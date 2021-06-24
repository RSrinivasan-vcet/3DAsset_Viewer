using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereWithMaterialPropertyBlock : MonoBehaviour
{
    public bool LineRenderObj;
    public Color Color1, Color2;    
    public float Speed = 1, Offset;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        if (LineRenderObj)
            _renderer = GetComponent<LineRenderer>();
        else
            _renderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        // Get the current value of the material properties in the renderer.
        _renderer.GetPropertyBlock(_propBlock);
        // Assign our new value.        
        //_propBlock.SetColor("_Color", Color1);        

        Color color = _renderer.material.color;
        color.a = 0.5f;
        _renderer.material.color = color;

        //_propBlock.SetColor("_Color", Color.Lerp(Color1.a, Color2.a,0.1f));
        // Apply the edited values to the renderer.
        //_renderer.SetPropertyBlock(_propBlock);
    }

    private void OnDisable()
    {        
        _renderer.GetPropertyBlock(_propBlock);       
        _propBlock.SetColor("_Color", Color.Lerp(Color2, Color1, 0.1f));
        _renderer.SetPropertyBlock(_propBlock);
    }

    //void Update()
    //{
    //    _renderer.GetPropertyBlock(_propBlock);
    //    _propBlock.SetColor("_Color", Color. //Color.Lerp(Color1, Color2, (Mathf.Sin(Time.time * Speed + Offset) + 1) / 2f));
    //    _renderer.SetPropertyBlock(_propBlock);
    //}
}