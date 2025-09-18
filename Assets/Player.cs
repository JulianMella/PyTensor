using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private int _ySliceCount;
    private int _xSliceCount;
    private int _zSliceCount;
    private int _currentLayer = 1;
    private bool _leftMouseIsPressed;
    private bool _radiusMode;
    private int _currentRadius = 0;
    private int _maxRadius;

    private int _objectSelected;

    private const int NoObjectSelected = 0;
    private const int SphereSelected = 1;
    private const int CubeSelected = 2;

    private bool _freezeMovement;
    private bool _stopTime;
    
    
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
    private GameObject _voxelHSphere;

    private readonly string _cwd = Directory.GetCurrentDirectory();

    [SerializeField] private Camera cam;
    [Header("Sphere Hover Color Settings")]
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material highlightMat;
    [SerializeField] private Material radiusModeHighlightMat;
    [SerializeField] private Material voxelMat;
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
        _xSliceCount = tensor.transform.GetChild(0).childCount;
        _zSliceCount = tensor.transform.GetChild(0).transform.GetChild(0).transform.childCount;
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

    // Maybe it is possible to do this function in a lower level? I don't know yet.
    void OnFreeze(InputValue value)
    {
        // This is cool to do, but should be refactored into its own files.
        if (!_freezeMovement)
        {
            Debug.Log("Frozen movement");
        }
        else if (!_stopTime)
        {
            Debug.Log("Unfrozen movement");
        }
        _freezeMovement = !_freezeMovement; //TODO: Expand this to granular movement tool
    }

    void OnTimeStop(InputValue value)
    {
        if (!_stopTime)
        {
            Debug.Log("Unstopped time");
        }
        else
        {
            Debug.Log("Stopped movement");
        }
        _stopTime = !_stopTime;
        _freezeMovement = false; // quick tip: assignation is faster than if check. 
    }
    
    //TODO: Add Show All Spheres toggle button.
    void OnShowEverything(InputValue value)
    {
        
    }

    void OnScroll(InputValue value)
    {
        if (_stopTime)
        {
            return;
        }
        
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

            if (scrollDelta.y < 0)
            {
                if (_currentRadius < _maxRadius)
                {
                    _currentRadius++;
                }
            }

            ShowHollowVoxelSphere(_currentRadius);
        }
        else
        {
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

    private void ShowHollowVoxelSphere(int radius)
    {
        if (_voxelHSphere != null)
        {
            Destroy(_voxelHSphere);
        }
        
        if (radius <= 0 || radius > _maxRadius)
        {
            return;
        }

        _voxelHSphere = new GameObject("Hollow Voxel Sphere");

        string filePath = _cwd + "/Assets/SphereCoordinates/Sphere" + radius + ".txt";
        string[] split;
        float xCoord;
        float yCoord;
        float zCoord;
        
        if (File.Exists(filePath))
        {
            foreach (string line in File.ReadLines(filePath))
            {
                split = line.Split(' ');
                // I think there are minor optimizations which could be done here to
                // improve amount of clock cycles for invalid positions
                
                xCoord = (float.Parse(split[0]) * 1.5f) + _radiusSphere.transform.position.x;
                yCoord = (float.Parse(split[1]) * 1.5f) +  _radiusSphere.transform.position.y;
                zCoord = (float.Parse(split[2]) * 1.5f) +  _radiusSphere.transform.position.z;

                // Referring to the last comment:
                // I think it should be possible to compare with the parsed values only if its a valid position
                // and only after that the value is multiplied and summed with the position of radiusSphere.
                if (xCoord < 1.5f || yCoord < 1.5f || zCoord < 1.5f)
                {
                    continue;
                }

                if (xCoord > _xSliceCount * 1.5f || yCoord > _ySliceCount * 1.5f || zCoord > _zSliceCount * 1.5f)
                {
                    continue;
                }
                
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position =  new Vector3(xCoord, yCoord, zCoord);
                cube.transform.SetParent(_voxelHSphere.transform);
                cube.tag = "voxelSphereObject";
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
        if (!_stopTime)
        {
            HandleLeftClick();
            HandleMovement(currDeltaTime);
            HandleRotation();
            HandleRaycastHover();
        }
        
        HandleScreenCenter();
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
                    Material mat = _radiusMode ? radiusModeHighlightMat : highlightMat;
                    SetHighlight(_objectHit, mat);
                }

                if (_hit.transform.CompareTag("innerBoundaryCube") && !_radiusMode)
                {
                    SetHighlight(_objectHit, highlightMat);
                }
            }

            if (_hit.transform.CompareTag("voxelSphereObject"))
            {
                _objectHit = _hit.transform;
                SetHighlight(_objectHit, voxelMat);
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

    private void SetHighlight(Transform objectHit, Material material)
    {
        objectHit.GetComponent<Renderer>().material = material;
    }

    private void HandleMovement(float deltaTime)
    {
        if (!_freezeMovement) // Hacky way, this can be expanded to a better suited tool.
        {
            Vector3 velocity = _moveInput * moveSpeed;
            Vector3 moveAmount = velocity * deltaTime;
            transform.Translate(moveAmount);
        }
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
        
        CalculateMaxRadius(_radiusSphere);
        UpdateRadiusSpherePosition();
        ShowAllObstacles();
    }

    private void CalculateMaxRadius(Transform selectedSphere)
    {
        int xPos = _xSliceCount - (int) (selectedSphere.position.x / 1.5);
        int xNeg = _xSliceCount - xPos - 1;
        int yPos = _ySliceCount - (int) (selectedSphere.position.y / 1.5);
        int yNeg = _ySliceCount - yPos - 1;
        int zPos  = _zSliceCount - (int) (selectedSphere.position.z / 1.5);
        int zNeg = _zSliceCount - zPos - 1;
        
        _maxRadius = Mathf.Max(xPos, xNeg, yPos, yNeg, zPos, zNeg);
    }

    private void UpdateRadiusSpherePosition()
    {
        if (_currentRadius > _maxRadius)
        {
            _currentRadius = _maxRadius;
        }

        ShowHollowVoxelSphere(_currentRadius);
    }
    
    private void ShowAllObstacles()
    {
        
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
