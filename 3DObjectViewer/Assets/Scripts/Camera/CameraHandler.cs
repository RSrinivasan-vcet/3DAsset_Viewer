using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraHandler : MonoBehaviour
{
    [System.Serializable]
    public struct MinMaxLimits
    {
        public float min;
        public float max;
    }
    public float panSpeed = 20f;
    public float zoomSpeedMouse = 5f;
    public float zoomSpeedScroll = 10f;
    public MinMaxLimits panXLimit = new MinMaxLimits { min = -10, max = 10 };
    public MinMaxLimits panYLimit = new MinMaxLimits { min = 0, max = 5 };
    public MinMaxLimits zoomLimit = new MinMaxLimits { min = 10, max = 60 };
    public GameObject targetTransform;

    [SerializeField]
    private Camera _LabelCamera = null;
    [SerializeField]
    private Camera _Cam = null;

    private float _Startpoint = 0;
    private Vector3 _LastMouseCoordinate = Vector3.zero;
    private Vector3 _LastPanPosition = Vector3.zero;
    private Vector3 _LastPosition = Vector3.zero;

    private bool _IsZoom = true;
    private bool _ZoomByMouseButtonActive = false;
    private bool _PanActive = false;
    private bool _RotationActive = false;

    [SerializeField]
    private Texture2D _PanCursor = null,
                      _ZoomCursor = null,
                      _RotateCursor = null,
                      _DefaultCursor = null;
    private void Awake()
    {
        if (_Cam == null)
            _Cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        Cursor.SetCursor(_DefaultCursor, Vector2.zero, CursorMode.ForceSoftware);
    }

    private void OnDisable()
    {

    }

    private void Start()
    {
        _Cam.transform.parent.localPosition = new Vector3(-3.9f, 0, 37.7f);
        _Cam.transform.localPosition = new Vector3(-5.53f, 1.5f, 0);
        _Cam.transform.parent.localRotation = Quaternion.Euler(5, -176.9f, 0);
        _Cam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _Cam.transform.localScale = new Vector3(1, 1, 1);
    }

    private void ResetNavigationBooleans()
    {
        _PanActive = false;
        _ZoomByMouseButtonActive = false;
        _RotationActive = false;
    }

    private void FrontView()
    {
        _Cam.transform.parent.localPosition = new Vector3(-5.4f, 0, 37.4f);
        _Cam.transform.localPosition = new Vector3(-6.52f, 1.36f, 0);
        _Cam.transform.parent.localRotation = Quaternion.Euler(5, 180, 0);
        _Cam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _Cam.transform.localScale = new Vector3(1, 1, 1);
        _Cam.fieldOfView = 13;
        _LabelCamera.fieldOfView = _Cam.fieldOfView;
    }

    private void RearView()
    {
        _Cam.transform.parent.localPosition = new Vector3(4.88f, 0, -17.5f);
        _Cam.transform.localPosition = new Vector3(-6.35f, 1.07f, 0);
        _Cam.transform.parent.localRotation = Quaternion.Euler(5, 1.07f, 0);
        _Cam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _Cam.transform.localScale = new Vector3(1, 1, 1);
        _Cam.fieldOfView = 11;
        _LabelCamera.fieldOfView = _Cam.fieldOfView;
    }

    private void SetZoom(bool isZoomAvailable)
    {
        _IsZoom = isZoomAvailable;
    }

    private void Update()
    {
        
            if (Input.GetMouseButtonDown(1))
                _LastPosition = Input.mousePosition;

            if (Input.GetMouseButtonUp(1))
                _LastMouseCoordinate = Vector3.zero;

            HandleMouse();

            if (!EventSystem.current.IsPointerOverGameObject())
        {     
            float scroll = Input.GetAxis("Mouse ScrollWheel");
                    //Mouse Scroll Zoom
                    if (scroll != 0)                 
                        ZoomCameraScroll(scroll, zoomSpeedScroll);
                }
        
    }

    private void HandleMouse()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButton(1))
        {
            //Camera Pan
            if (_PanActive)
                PanCamera();

            //Mouse button by Zoom
            if (_ZoomByMouseButtonActive)
                ZoomByMouseButtonDrag();

            //Mouse Rotate
            if (_RotationActive)
                RotateCamera(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(1))
        {
            _IsCursorChanged = false;
            Cursor.SetCursor(_DefaultCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
            

    }
    private bool _IsCursorChanged;

    private void PanCamera()
    {
        if(!_IsCursorChanged)
        {
            _IsCursorChanged = true;
            Cursor.SetCursor(_PanCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        Vector3 delta = Input.mousePosition - _LastPosition;
        if (delta.magnitude > 1)
        {
            transform.Translate(-delta.x * panSpeed, -delta.y * panSpeed, 0, Space.Self);

            transform.localPosition = new Vector3(Mathf.Clamp(
                transform.localPosition.x, panXLimit.min, panXLimit.max),
                Mathf.Clamp(transform.localPosition.y, panYLimit.min, panYLimit.max),
                0);
        }
        _LastPosition = Input.mousePosition;
    }

    private void RotateCamera(Vector3 newPosition)
    {
        //print("LAST = " + _LastPosition.x + "NEW = " + newPosition.x);
        if (!_IsCursorChanged)
        {
            _IsCursorChanged = true;
            Cursor.SetCursor(_RotateCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        if (_LastPosition.x < newPosition.x)
            transform.parent.RotateAround(targetTransform.transform.position, Vector3.up, 60 * Time.deltaTime);
        else if (_LastPosition.x > newPosition.x)
            transform.parent.RotateAround(targetTransform.transform.position, -Vector3.up, 60 * Time.deltaTime);

        _LastPosition = newPosition;
    }

    private void ZoomByMouseButtonDrag()
    {
        if (!_IsCursorChanged)
        {
            _IsCursorChanged = true;
            Cursor.SetCursor(_ZoomCursor, Vector2.zero, CursorMode.ForceSoftware);
        }
        Vector3 mouseDelta = Input.mousePosition - _LastMouseCoordinate;

        if (mouseDelta.magnitude > 1)
        {
            Vector3 direction = mouseDelta.normalized;
            if (_LastMouseCoordinate == Vector3.zero)
            {
                _LastMouseCoordinate = Input.mousePosition;
                _Startpoint = Vector3.Dot(direction, Vector3.up);
            }
            float dot = Vector3.Dot(direction, Vector3.up);
            if (_Startpoint > dot)
            {
                if (_Cam.fieldOfView < zoomLimit.max)
                {
                    _Cam.fieldOfView += 0.5f;
                    _LabelCamera.fieldOfView = _Cam.fieldOfView;
                }
            }
            else if (_Startpoint < dot)
            {
                if (_Cam.fieldOfView > zoomLimit.min)
                {
                    _Cam.fieldOfView -= 0.5f;
                    _LabelCamera.fieldOfView = _Cam.fieldOfView;
                }
            }
        }
    }

    private void ZoomCameraScroll(float offset, float speed)
    {
        if (_ZoomByMouseButtonActive && offset == 0)
            return;

        _Cam.fieldOfView = Mathf.Clamp(_Cam.fieldOfView - (offset * speed), zoomLimit.min, zoomLimit.max);
        _LabelCamera.fieldOfView = _Cam.fieldOfView;
    }
}

