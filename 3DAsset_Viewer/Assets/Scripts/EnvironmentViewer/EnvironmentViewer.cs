using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentViewer : MonoBehaviour
{
    [SerializeField]
    private Button _HomeBtn = null;
    [SerializeField]
    private GameObject _MainMenuUIObj = null;

    void Start()
    {
        _HomeBtn.onClick.AddListener(()=> { _MainMenuUIObj.SetActive(true);gameObject.SetActive(false); });   
    }
}
