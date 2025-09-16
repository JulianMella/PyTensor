using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private int _maxInnerDimensions;
    private int _currentLayer = 1;
    private bool _leftMouseIsPressed;
    private bool _radiusMode = false;

    private int _objectSelected;
    
    [Header("Camera Settings")] 
    public float moveSpeed = 10f;
    public float lookSensitivity = 1f;
    
    private Vector3 _moveInput;
    private Vector2 _rotationInput;

    private Vector2 _currentRotation;
    private Vector2 _rotationVelocity;
    private int _screenWidth;
    private int _screenHeight;
    private Vector3 _screenCenter;
    private Transform _objectHit;

    private RaycastHit _hit;
    private Ray _ray;

    private GameObject _matrix;

    [SerializeField] private Camera cam;
    [Header("Sphere Hover Color Settings")]
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material highlightMat;
    [SerializeField] private Material radiusModeHighlightMat;
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        UpdateScreenCenter(); // This must be called in Start because it depends on Screen.width and Screen.height, which is not guaranteed to be initialized in Awake()
    }

    private void UpdateScreenCenter()
    {
        _screenHeight = Screen.height;
        _screenWidth = Screen.width;
        _screenCenter = new Vector3(_screenWidth / 2f, _screenHeight / 2f, 0f);
    }
    
    void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector3>();
    }

    void OnLook(InputValue value)
    {
        _rotationInput = value.Get<Vector2>();
    }
    
    void OnEnable() => MatrixSpawner.OnMatrixSpawned += HandleMatrixSpawned;
    void OnDisable() => MatrixSpawner.OnMatrixSpawned -= HandleMatrixSpawned;
    void HandleMatrixSpawned(GameObject matrix)
    {
        _matrix = matrix;
        _maxInnerDimensions = _matrix.transform.childCount;
    }

    private void OnRightPress(InputValue value)
    {
        _radiusMode = !_radiusMode;
        if (_radiusMode)
        {
            Debug.Log("Radius Mode Activated");
        }
        else
        {
            Debug.Log("Builder Mode Activated");
        }
    }
    
    private void OnLeftPress(InputValue value)
    {
        _leftMouseIsPressed = value.isPressed;
        if (_radiusMode  && _leftMouseIsPressed)
        {
            ShootRay();
        }
        else if (!_radiusMode)
        {
            
            // TODO: Figure out how to allow a short timed left click to only change one cube and only modify multiple if the click last lost enough???
            // The problem here is that press and release are two separate function calls, where release comes after press.
            // Essentially, what I need to do is to add a countdown timer in the press function 
            // Where if _leftMouseIsPressed is still true after that period, then it will allow for multiple edits based 
            // upon that left click
        
            // IDK, unreasonably hard to figure this one out simply, will come back to look at this eventually
        
            if (!_leftMouseIsPressed)
            {
                _objectSelected = 0;
            }
        }
    }

    void OnScroll(InputValue value)
    {
        if (_matrix == null)
        {
            Debug.Log("Matrix is not initialized yet");
            return;
        }
        Vector2 scrollDelta = value.Get<Vector2>();

        if (scrollDelta.y > 0)
        {
            if (_currentLayer > 0)
            {
                _currentLayer--;    
                Debug.Log("Layer decrease, value now:" + _currentLayer);
            }
        }
        else if (scrollDelta.y < 0)
        {
            if (_currentLayer < _maxInnerDimensions)
            {
                _currentLayer++;    
                Debug.Log("Layer increase, value now:" + _currentLayer);
            }
        }

        ShowLayer(_currentLayer);
    }

    private void ShowLayer(int layer)
    {
        if (layer == _maxInnerDimensions)
        {
            for (int i = 0; i < _maxInnerDimensions; i++)
            {
                Transform matrixSlice = _matrix.transform.GetChild(i); 
                ToggleSpheresOnLayer(matrixSlice, false);
            }
        }

        else
        {
            for (int i = 0; i < _maxInnerDimensions; i++)
            {
                Transform matrixSlice = _matrix.transform.GetChild(i); 
                if (i > layer)
                {
                    // Hide everything above
                    matrixSlice.gameObject.SetActive(false);
                }
                
                else if (i < layer)
                {
                    matrixSlice.gameObject.SetActive(true);
                    ToggleSpheresOnLayer(matrixSlice, false);
                }
                else
                {
                    matrixSlice.gameObject.SetActive(true);
                    ToggleSpheresOnLayer(matrixSlice, true);
                }
            }
        }
    }

    void ToggleSpheresOnLayer(Transform matrixSlice, bool toggle)
    {
        foreach (Transform transformChild in matrixSlice)
        {
            if (transformChild.CompareTag("innerBoundarySphere"))
            {
                transformChild.gameObject.SetActive(toggle);
            }
        }
    }
    
    void Update()
    {
        float currDeltaTime = Time.deltaTime;
        HandleLeftClick();
        HandleScreenCenter();
        HandleMovement(currDeltaTime);
        HandleRotation();
        HandleRaycastHover();
    }
    
    private void HandleLeftClick()
    {
        if (_leftMouseIsPressed && !_radiusMode)
        {
            ShootRay();
        }

    }
    
    private void HandleScreenCenter()
    {
        if (_screenWidth != Screen.width || _screenHeight != Screen.height)
        {
            UpdateScreenCenter();
        }
    }

    private void HandleRaycastHover()
    {
        _ray = cam.ScreenPointToRay(_screenCenter);
        
        if (Physics.Raycast(_ray, out _hit))
        {
            if (_objectHit != null && _objectHit != _hit.transform)
            {
                ResetColor(_objectHit);
            }
    
            if (_hit.transform.CompareTag("innerBoundarySphere") || _hit.transform.CompareTag("innerBoundaryCube"))
            {
                _objectHit = _hit.transform;

                if (_hit.transform.CompareTag("innerBoundarySphere"))
                {
                    SetHighlight(_objectHit, _radiusMode);
                }

                if (_hit.transform.CompareTag("innerBoundaryCube"))
                {
                    SetHighlight(_objectHit, false);
                }
            }

        }

        else
        {
            // TODO: Confirm this is necessary
            if (_objectHit != null)
            {
                ResetColor(_objectHit);
                _objectHit = null;
            }
        }
    }

    private void ResetColor(Transform objectHit)
    {
        objectHit.GetComponent<Renderer>().material = defaultMat;
    }

    private void SetHighlight(Transform objectHit, bool radiusMode)
    {
        if (radiusMode)
        {
            objectHit.GetComponent<Renderer>().material = radiusModeHighlightMat;
        }
        else
        {
            objectHit.GetComponent<Renderer>().material = highlightMat;
        }
    }

    private void HandleMovement(float deltaTime)
    {
        Vector3 velocity = _moveInput * moveSpeed;
        Vector3 moveAmount = velocity * deltaTime;
        transform.Translate(moveAmount);
    }

    private void HandleRotation()
    {
        _currentRotation += _rotationInput * lookSensitivity;
        _currentRotation.y = Mathf.Clamp(_currentRotation.y, -90f, 90f); // prevent flipping
        transform.rotation = Quaternion.Euler(-_currentRotation.y, _currentRotation.x, 0);
    }

    private void ShootRay()
    {
        _ray = cam.ScreenPointToRay(_screenCenter);

        if (Physics.Raycast(_ray, out _hit))
        {
            if (_radiusMode && _hit.transform.CompareTag("innerBoundarySphere"))
            {
                
            }

            else
            {
                
                if (_objectSelected == 0)
                {
                    if (_hit.transform.CompareTag("innerBoundarySphere"))
                        _objectSelected = 1;
                    else if (_hit.transform.CompareTag("innerBoundaryCube"))
                        _objectSelected = 2;
                }
                if (_objectSelected == 1 && _hit.transform.CompareTag("innerBoundarySphere"))
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = _hit.transform.position;
                    cube.transform.rotation = _hit.transform.rotation;
                    cube.transform.parent = _hit.transform.parent;
                    cube.transform.localScale = _hit.transform.localScale;
                    cube.transform.tag = "innerBoundaryCube";
                    Destroy(_hit.transform.gameObject);
                }
                
                else if (_objectSelected == 2 && _hit.transform.CompareTag("innerBoundaryCube"))
                {
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = _hit.transform.position;
                    sphere.transform.rotation = _hit.transform.rotation;
                    sphere.transform.parent = _hit.transform.parent;
                    sphere.transform.localScale = _hit.transform.localScale;
                    sphere.transform.tag = "innerBoundarySphere";
                    Destroy(_hit.transform.gameObject);
                }
            }
        }
    }
}
