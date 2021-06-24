using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _3DModelViewer=null;
    [SerializeField]
    private GameObject _EnvironmentViewer = null;

    [SerializeField]
    private Button _3DModelViewerBtn = null;
    [SerializeField]
    private Button _EnvironmentViewerBtn = null;

    private void Start()
    {
        _3DModelViewerBtn.onClick.AddListener(()=> { _3DModelViewer.SetActive(true); gameObject.SetActive(false); });
        _EnvironmentViewerBtn.onClick.AddListener(()=> { _EnvironmentViewer.SetActive(true); gameObject.SetActive(false); });        
    }

    private void OnEnable()
    {
        _3DModelViewer.SetActive(false);
        _EnvironmentViewer.SetActive(false);
    }

    private void OnDisable()
    {
        
    }
}
