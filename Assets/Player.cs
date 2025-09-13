using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class Player : MonoBehaviour
{
    private const int MaxInnerDimensions = MatrixConstants.MaxDimensionValue - 2; // -2 because inner boundaries remove outer walls 
    private int _currentLayer = 1;
    
    [Header("Camera Settings")] 
    public float moveSpeed = 10f;
    public float lookSensitivity = 1f;
    
    private Vector3 _moveInput;
    private Vector2 _rotationInput;

    private Vector2 _currentRotation;
    private Vector2 _rotationVelocity;
    
    private readonly Vector3 _screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
    private Transform _objectHit;

    private RaycastHit _hit;
    private Ray _ray;
    
    private GameObject _matrix;
    
    [SerializeField] private Camera cam;
    [Header("Sphere Hover Color Settings")]
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material highlightMat;
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponent<Camera>();
    }

    void OnEnable() => MatrixSpawner.OnMatrixSpawned += HandleMatrixSpawned;
    void OnDisable() => MatrixSpawner.OnMatrixSpawned -= HandleMatrixSpawned;

    void HandleMatrixSpawned(GameObject matrix)
    {
        _matrix = matrix;
        Debug.Log("Got matrix!");
        Debug.Log(_matrix.transform.childCount);
    }
    
    void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector3>();
    }

    void OnLook(InputValue value)
    {
        _rotationInput = value.Get<Vector2>();
    }

    void OnScroll(InputValue value)
    {
        Vector2 scrollDelta = value.Get<Vector2>();

        if (scrollDelta.y > 0)
        {
            _currentLayer--;
        }
        else if (scrollDelta.y < 0)
        {
            _currentLayer++;
        }

        if (_currentLayer < 1)
        {
            _currentLayer = MaxInnerDimensions;
        }

        if (_currentLayer > MaxInnerDimensions)
        {
            _currentLayer = 1;
        }

        ShowLayer(_currentLayer);
    }

    private void ShowLayer(int layer)
    {
    }
    

    void OnLeftClick(InputValue value)
    {
        _ray = cam.ScreenPointToRay(_screenCenter);

        if (Physics.Raycast(_ray, out _hit))
        {
            if (_hit.transform.CompareTag("innerBoundarySphere"))
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = _hit.transform.position;
                cube.transform.rotation = _hit.transform.rotation;
                cube.transform.parent = _hit.transform.parent;
                cube.transform.localScale = _hit.transform.localScale;
                cube.transform.tag = "innerBoundaryCube";
                Destroy(_hit.transform.gameObject);
            }
            
            else if (_hit.transform.CompareTag("innerBoundaryCube"))
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
    
    void Update()
    {
        float currDeltaTime = Time.deltaTime;
        HandleMovement(currDeltaTime);
        HandleRotation();
        HandleRaycastHover();
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
                SetHighlight(_objectHit);
            }

        }

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
        objectHit.GetComponent<Renderer>().material = defaultMat;
    }

    private void SetHighlight(Transform objectHit)
    {
        objectHit.GetComponent<Renderer>().material = highlightMat;
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
}
