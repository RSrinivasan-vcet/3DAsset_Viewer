using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.EventSystems;

public class AssetViewer : MonoBehaviour
{
    [Serializable]
    public struct MinMaxLimits
    {
        public float min;
        public float max;
    }
    
    public static Action DisableLoaderEvent;
    public static Action<bool> OnModelViewEvent;
    public static Action<int,int> OnModelPolyCountEvent;

    public static bool isZoom = false;
    public new Camera camera;
    public Transform transformObj;
    //public GameObject testObj;
    public float zoomSpeed = 1;    
    public GameObject resetOb;    
    //public GameObject testObject = null;

    private GameObject assetObj = null;
    private Vector3 _SpawnObjPos;
    private Vector3 _LastPosition = Vector3.zero;

    public float panSpeed = 20f;
    public MinMaxLimits panXLimit = new MinMaxLimits { min = -5, max = 5 };
    public MinMaxLimits panYLimit = new MinMaxLimits { min = -10, max = 10 };
    public MinMaxLimits panZLimit = new MinMaxLimits { min = -5, max = 5 };
    public MinMaxLimits panYLimitCam = new MinMaxLimits { min = 0, max = 0 };

    Vector3 ObjCamPos = Vector3.zero;

    public Vector3 transformPosition;
    public Vector3 euler;
    private Vector3 localScale=Vector3.zero;

    Dictionary<string, object> tmpDic = new Dictionary<string, object>();

    #region ModelViewer UI related items
    public Dropdown dropdown = null;
    public string selectedAssetPath = "";
    private UnityWebRequest uwr;
    private string[] _filePaths;
    private string _path;
    [SerializeField]
    private Button _HomeBtn = null;
    [SerializeField]
    private GameObject _MainMenuUIObj = null;
    [SerializeField]
    private GameObject _LoadingImg = null;
    [SerializeField]
    private Text _TriCounts = null;
    [SerializeField]
    private Text _PolyCounts = null;

    #endregion
    private AssetBundleDownloader assetBundleDownloader;

    private void Start()
    {
        assetBundleDownloader = new AssetBundleDownloader();

        _SpawnObjPos = transformObj.localPosition;
        resetOb.GetComponent<Button>().onClick.AddListener(OnResetButton);
        _HomeBtn.onClick.AddListener(() => { _MainMenuUIObj.SetActive(true); gameObject.SetActive(false); Destroy(assetObj); });
        
        dropdown.onValueChanged.AddListener(delegate
        {
            OnValueChanged(dropdown);
        });
    }

    private void OnValueChanged(Dropdown pDropdown)
    {
        if (dropdown == pDropdown)
        {
            if (pDropdown.value > 0)
            {
                selectedAssetPath = _filePaths[pDropdown.value-1].Trim();
                //LoadAssetObj();

                _LoadingImg.SetActive(true);
                assetBundleDownloader.LoadAssetBundleCompleted += ViewAssetViewer;
                StopAllCoroutines();
                assetBundleDownloader.DownloadAndCache_AssetBundle(selectedAssetPath, false);
            }
            else
            {
                selectedAssetPath = "https://sifyapps.s3.ap-south-1.amazonaws.com/AssetBundles_3DViewer/empty.unity3d";
                assetBundleDownloader.DownloadAndCache_AssetBundle(selectedAssetPath, false);
            }
        }
    }
  
    private void DisableLoader()
    {
        _LoadingImg.SetActive(false);
    }
    private void OnResetButton()
    {
        ResetZoom();
    }

    public void ViewAssetViewer(object obj,bool isScene)
    {
        assetBundleDownloader.LoadAssetBundleCompleted -= ViewAssetViewer;
        _LoadingImg.SetActive(false);

        if (assetObj!=null)
        {
            DestroyImmediate(assetObj);
        }

        assetObj = Instantiate((GameObject)obj);        
        assetObj.name = "3DModelViewerObj";
        assetObj.SetActive(true);
        localScale=assetObj.transform.localScale;

        if (Application.isEditor)
        {
            assetObj.AddComponent<ReApplyShaders>();
        }

        if (assetObj.GetComponentInChildren<Renderer>().materials.Length < 2)
        {
            assetObj.AddComponent<TwoSidedMesh>();
        }
        assetObj.AddComponent<SetPivot1>();
        assetObj.AddComponent<ObjectRotator1>();
        assetObj.AddComponent<AddBoxColliderToObj>();
        
        AddBoxColliderToObj.ColliderAddedSucess += ColliderAdded;
    }

    void ColliderAdded()
    {
        AddBoxColliderToObj.ColliderAddedSucess -= ColliderAdded;
        //transformObj.GetComponent<BoxCollider>().enabled = true;
        //var szA = transformObj.GetComponent<BoxCollider>().bounds.size;
        var szA = new Vector3(12f, 12f, 12f);
//        var szA = new Vector3(12.5f, 12.5f, 12.5f);
        var szB = assetObj.GetComponent<BoxCollider>().bounds.size;
        var diff = szA - szB;
        transformObj.GetComponent<BoxCollider>().enabled = false;

        float scaleX = szA.x / szB.x;
        float scaleY = szA.y / szB.y;
        float scaleZ = szA.z / szB.z;
        float scaleVal = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));
        //float scaleVal = Mathf.Max(scaleX, Math.Max(scaleY, scaleZ));

        if ((diff.x > 0 && diff.y > 0 && diff.z > 0) || (diff.x > 18.25f))
        {
            scaleVal = Mathf.Max(scaleX, Mathf.Max(scaleY, scaleZ));
            if (scaleVal > 10)
            {
                scaleVal = 10;
            }
        }
        var scale = new Vector3(scaleVal, scaleVal, scaleVal);
        //scaleVal = scaleVal <= 1 ? scaleVal : 1;

        PlaceModel(scale);
        resetOb.SetActive(true);
    }

    private void OnEnable()
    {
        DisableLoaderEvent += DisableLoader;
        OnModelPolyCountEvent += OnModelPolyCount;
        _LoadingImg.SetActive(false);
        panYLimitCam.min = camera.transform.localPosition.y + panYLimit.min;
        panYLimitCam.max = camera.transform.localPosition.y + panYLimit.max;
        //panYLimitCam.min = camera.transform.localPosition.y + panYLimit.min;
        //panYLimitCam.max = camera.transform.localPosition.y + panYLimit.max;

        //panZLimit.min = transformObj.localPosition.z - 5;
        //panZLimit.max = transformObj.localPosition.z + 5;

        ObjCamPos = camera.transform.position;
        DropdownListUpdate();
    }

    private void OnDisable()
    {
        DisableLoaderEvent -= DisableLoader;
        OnModelPolyCountEvent -= OnModelPolyCount;

        isZoom = false;
    }

    private void OnModelPolyCount(int triCount, int polyCount)
    {
        _TriCounts.gameObject.SetActive(true);
        _PolyCounts.gameObject.SetActive(true);

        _TriCounts.text += triCount;
        _PolyCounts.text += polyCount;
    }

    private void DropdownListUpdate()
    {
        //clear/remove all option item
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData() { text = "Select" });

        //_path = GetFileLocation("AssetBundles");
        //print("_path : " + _path);

        StartCoroutine(ReadFileNames());
       
    }

    private IEnumerator ReadFileNames()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "FileNameList.txt");
        //print("filePath  : " + filePath);

        string result;
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            result = www.downloadHandler.text.Trim();
        }
        else
        {
            result = File.ReadAllText(filePath).Trim();
        }
        print("result : " + result);
        _filePaths = result.Split('\n');
        foreach (var s in _filePaths)
        {
            string[] tmp = s.Split('/');
            string str = tmp[tmp.Length - 1].Replace(".unity3d","");
            //print("_filePaths  : " + str);

            dropdown.options.Add(new Dropdown.OptionData() { text = str });
        }
        dropdown.value = 0;
    }
    
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            ZoomCamera(scroll, zoomSpeed);
            //_ZoomCamera(scroll);

            if (Input.GetMouseButtonDown(1))
                _LastPosition = Input.mousePosition;

            if (Input.GetMouseButton(1))
            {
                //Object Pan
                PanGameObject();
            }
        }
    }

    public void ResetZoom()
    {
        //transformObj.localPosition= _SpawnObjPos;
        //camera.fieldOfView = 60f;
        camera.transform.position = ObjCamPos;
        ObjectRotator1.ResetRotationEvent?.Invoke();

        assetObj.transform.localPosition = transformPosition;
        //assetObj.transform.localRotation = Quaternion.identity;
        assetObj.transform.localRotation = Quaternion.Euler(euler);
        camera.fieldOfView = 60;
        //assetObj.transform.localRotation = Quaternion.Euler(-10, 16, 0);
    }

    //public void LoadAssetObj()
    //{
    //    StartCoroutine(LoadAssetObj_Co());
    //}
    
    //public IEnumerator LoadAssetObj_Co()
    //{
    //    print("selectedAssetPath : " + selectedAssetPath);
    //    using (uwr = UnityWebRequestAssetBundle.GetAssetBundle(selectedAssetPath))
    //    {
    //        yield return uwr.SendWebRequest();
    //        if (uwr.isNetworkError || uwr.isHttpError)
    //        {
    //            Debug.Log(uwr.error);
    //        }
    //        else
    //        {
    //            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
    //            ViewAssetViewer((GameObject)bundle.LoadAsset(bundle.GetAllAssetNames()[0]));
                
    //            // try to cleanup memory
    //            Resources.UnloadUnusedAssets();
    //            bundle.Unload(false);
    //            bundle = null;
    //        }                        
    //    }
    //}
    public void PlaceModel(Vector3 scale)
    {
        //_Rotator.Reset();
        ObjectRotator1.ResetRotationEvent?.Invoke();

        ResetZoom();
        transform.GetChild(0).gameObject.SetActive(true);
        assetObj.transform.SetParent(transformObj, true);
        assetObj.SetActive(true);
        assetObj.transform.localScale = scale;

        //if(localScale.x>12)
        //{
        //    assetObj.transform.localScale = localScale;
        //}
        assetObj.transform.localPosition = transformPosition;
        assetObj.transform.localRotation = Quaternion.Euler(euler);

        AssetProperties assetProperties = assetObj.transform.GetComponent<AssetProperties>();
        if(assetProperties)
        {
            assetObj.transform.localPosition = assetProperties.Position;
            assetObj.transform.localRotation = Quaternion.Euler(assetProperties.Rotation);
            if(assetProperties.Scale.x>0)
                assetObj.transform.localScale= assetProperties.Scale;
        }
        transformPosition = assetObj.transform.localPosition;
        euler= assetObj.transform.localRotation.eulerAngles;

        //print("assetObj.transform.localRotation : " + assetObj.transform.localRotation);

        resetOb.SetActive(true);
        OnModelViewEvent?.Invoke(true);
        isZoom = true;
    }

    void ZoomCamera(float offset, float speed)
    {
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView - (offset * speed), 10, 60);
    }

    void _ZoomCamera(float scroll)
    {
        if (scroll != 0)
        {
            RaycastHit hit;

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Vector3 desiredPosition;

            if (Physics.Raycast(ray, out hit))
            {
                desiredPosition = hit.point;
            }
            else
            {
                desiredPosition = camera.transform.position;
            }

            float distance = Vector3.Distance(desiredPosition, camera.transform.position);
            Vector3 direction = Vector3.Normalize(desiredPosition - camera.transform.position) * (distance * scroll);

            float checkCamDist = Vector3.Distance(ObjCamPos, (camera.transform.position + direction));

            if (checkCamDist > -0.1f && checkCamDist < 0.01f)
            {
                camera.transform.position += direction;
            }
        }
    }

    private void PanGameObject()
    {
        
            // Determine how much to move the camera
            Vector3 offset = camera.ScreenToViewportPoint(_LastPosition - Input.mousePosition);
            Vector3 move = new Vector3(offset.x * panSpeed, offset.y * panSpeed, 0);

            // Perform the movement
            camera.transform.Translate(move, Space.Self);
            // Ensure the camera remains within bounds.
            Vector3 pos = camera.transform.localPosition;
            pos.x = Mathf.Clamp(camera.transform.localPosition.x, panXLimit.min, panXLimit.max);
            pos.y = Mathf.Clamp(camera.transform.localPosition.y, panYLimitCam.min, panYLimitCam.max);
            pos.z = camera.transform.localPosition.z;

            camera.transform.localPosition = pos;
            // Cache the position
            _LastPosition = Input.mousePosition;
        
    }
}
