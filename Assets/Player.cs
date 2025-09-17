using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private int _ySliceCount;
    private int _currentLayer = 1;
    private bool _leftMouseIsPressed;
    private bool _radiusMode = false;
    private int _currentRadius = 0;
    private int _maxRadius;

    private int _objectSelected;

    private const int NoObjectSelected = 0;
    private const int SphereSelected = 1;
    private const int CubeSelected = 2;
    
    
    
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
    private Transform _radiusSphere;

    private RaycastHit _hit;
    private Ray _ray;

    private GameObject _tensor;

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
    
    void OnEnable() => TensorSpawner.OnTensorSpawned += HandleTensorSpawned;
    void OnDisable() => TensorSpawner.OnTensorSpawned -= HandleTensorSpawned;
    void HandleTensorSpawned(GameObject tensor)
    {
        _tensor = tensor;
        _ySliceCount = tensor.transform.childCount;
    }

    private void OnRightPress(InputValue value)
    {
        _radiusMode = !_radiusMode;
    }
    
    private void OnLeftPress(InputValue value)
    {
        _leftMouseIsPressed = value.isPressed;
        if (!_radiusMode)
        {
            
            // TODO: Figure out how to allow a short timed left click to only change one cube and only modify multiple if the click last lost enough???
            // The problem here is that press and release are two separate function calls, where release comes after press.
            // Essentially, what I need to do is to add a countdown timer in the press function 
            // Where if _leftMouseIsPressed is still true after that period, then it will allow for multiple edits based 
            // upon that left click
        
            // IDK, unreasonably hard to figure this one out simply, will come back to look at this eventually
        
            if (!_leftMouseIsPressed)
            {
                _objectSelected = NoObjectSelected;
            }
        }
    }

    void OnScroll(InputValue value)
    {
        if (_tensor == null)
        {
            Debug.Log("Tensor is not initialized yet");
            return;
        }
        Vector2 scrollDelta = value.Get<Vector2>();
        if (_radiusMode)
        {
            if (_radiusSphere == null)
            {
                Debug.Log("Radius sphere is not selected yet");
                return;
            }

            if (scrollDelta.y > 0)
            {
                _currentRadius--;
            }

            if (scrollDelta.y < _ySliceCount) // TODO: Check if there is a better value to utilize here...?
            {                                       // Might require some significant computation to calculate max radius of sphere from selecetd point
                _currentRadius++;
            }
        }
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
            if (_currentLayer < _ySliceCount)
            {
                _currentLayer++;    
                Debug.Log("Layer increase, value now:" + _currentLayer);
            }
        }

        ShowLayer(_currentLayer);
    }

    private void ShowLayer(int layer)
    {
        if (layer == _ySliceCount)
        {
            for (int i = 0; i < _ySliceCount; i++)
            {
                Transform tensorSlice = _tensor.transform.GetChild(i);
                ToggleSpheresOnLayer(tensorSlice, false);
            }
        }

        else
        {
            for (int i = 0; i < _ySliceCount; i++)
            {
                Transform tensorSlice = _tensor.transform.GetChild(i);
                if (i > layer)
                {
                    // Hide everything above
                    tensorSlice.gameObject.SetActive(false);
                }
                
                else if (i < layer)
                {
                    tensorSlice.gameObject.SetActive(true);
                    ToggleSpheresOnLayer(tensorSlice, false);
                }
                else
                {
                    tensorSlice.gameObject.SetActive(true);
                    ToggleSpheresOnLayer(tensorSlice, true);
                }
            }
        }
    }

    void ToggleSpheresOnLayer(Transform tensorSlice, bool toggle)
    {
        foreach (Transform transformChildX in tensorSlice)
        {
            foreach (Transform transformChildZ in transformChildX)
            {
                if (transformChildZ.CompareTag("innerBoundarySphere"))
                {
                    transformChildZ.gameObject.SetActive(toggle);
                }
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
        if (_leftMouseIsPressed)
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

            if (_hit.transform.CompareTag("innerBoundaryCube") && _radiusMode)
            {
                ResetColor(_hit.transform);
            }
    
            if (_hit.transform.CompareTag("innerBoundarySphere") || _hit.transform.CompareTag("innerBoundaryCube"))
            {
                _objectHit = _hit.transform;

                if (_hit.transform.CompareTag("innerBoundarySphere"))
                {
                    SetHighlight(_objectHit, _radiusMode);
                }

                if (_hit.transform.CompareTag("innerBoundaryCube") && !_radiusMode)
                {
                    SetHighlight(_objectHit, false);
                }
            }

        }
        // If ray hits nothing, remove color of last selected
        else
        {
            if (_objectHit != null)
            {
                ResetColor(_objectHit);
                _objectHit = null;
            }
        }
    }

    private void ResetColor(Transform objectHit)
    {
        if (_radiusMode && objectHit.GetComponent<Renderer>().sharedMaterial == radiusModeHighlightMat && objectHit.transform.CompareTag("radiusSphere"))
        {
            return;
        }
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
                HandleRadiusManipulation(_hit);
            }

            else
            {
                HandleBoundaryManipulation(_hit);
            }
        }
    }

    private void HandleRadiusManipulation(RaycastHit hit)
    {
        if (_radiusSphere != null && hit.transform != _radiusSphere)
        {
            _radiusSphere.GetComponent<Renderer>().material = defaultMat;
            _radiusSphere.tag = "innerBoundarySphere";
        }
        
        hit.transform.GetComponent<Renderer>().material = radiusModeHighlightMat;
        hit.transform.tag = "radiusSphere";
        
        _radiusSphere = hit.transform;
    }

    private void HandleBoundaryManipulation(RaycastHit hit)
    {
        if (_objectSelected == NoObjectSelected)
        {
            if (hit.transform.CompareTag("innerBoundarySphere"))
                _objectSelected = SphereSelected;
            else if (_hit.transform.CompareTag("innerBoundaryCube"))
                _objectSelected = CubeSelected;
        }
        if (_objectSelected == SphereSelected && hit.transform.CompareTag("innerBoundarySphere"))
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = hit.transform.position;
            cube.transform.rotation = hit.transform.rotation;
            cube.transform.parent = hit.transform.parent;
            cube.transform.localScale = hit.transform.localScale;
            cube.transform.tag = "innerBoundaryCube";
            Destroy(hit.transform.gameObject);
        }
                
        else if (_objectSelected == CubeSelected && _hit.transform.CompareTag("innerBoundaryCube"))
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = hit.transform.position;
            sphere.transform.rotation = hit.transform.rotation;
            sphere.transform.parent = hit.transform.parent;
            sphere.transform.localScale = hit.transform.localScale;
            sphere.transform.tag = "innerBoundarySphere";
            Destroy(hit.transform.gameObject);
        }
    }
}
