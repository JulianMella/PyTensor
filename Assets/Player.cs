using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
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

    [SerializeField] private Camera cam;
    [Header("Sphere Hover Color Settings")]
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material highlightMat;
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponent<Camera>();
    }
    
    void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector3>();
    }

    void OnLook(InputValue value)
    {
        _rotationInput = value.Get<Vector2>();
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
