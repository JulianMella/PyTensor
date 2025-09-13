using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PyMatrix : EditorWindow
{
    [MenuItem("Window/UI Toolkit/PyMatrix")]
    public static void ShowPyMatrix()
    {
        PyMatrix wnd = GetWindow<PyMatrix>();
        wnd.titleContent = new GUIContent("PyMatrix");
    }

    private const int MaxDimensionValue = 100;

    private readonly List<GameObject> _cubePlaneObjects = new();
    private GameObject _matrixFloor = null;
    private GameObject _innerBoundaries = null;
    private string _path = "";
    private string _filename = "";

    public string Path
    {
        get
        {
            return _path; 
        }
        set
        {
            if (_path != value)
            {
                _path = value + "/";
                ActivateExportButton();
            }
        }
    }
    
    public void CreateGUI()
    {
        var root = rootVisualElement;

        var widthField = new UnsignedIntegerField()
        {
            label = "Enter width of matrix: ",
            name = "widthField",
            value = 1,
        };
        widthField.RegisterValueChangedCallback((v) =>
        {
            if (v.newValue > MaxDimensionValue) widthField.value = MaxDimensionValue;
            else if (v.newValue < 1)  widthField.value = 1;
        });
        
        root.Add(widthField);
        
        var lengthField = new UnsignedIntegerField()
        {
            label = "Enter length of matrix: ",
            name = "lengthField",
            value = 1
        };

        lengthField.RegisterValueChangedCallback((v) =>
        {
            if (v.newValue > MaxDimensionValue) lengthField.value = MaxDimensionValue;
            else if (v.newValue < 1)  lengthField.value = 1;
        });
        
        root.Add(lengthField);

        var heightField = new UnsignedIntegerField()
        {
            label = "Enter height of matrix: ",
            name = "heightField",
            value = 1
        };

        heightField.RegisterValueChangedCallback((v) =>
        {
            if (v.newValue > MaxDimensionValue) heightField.value = MaxDimensionValue;
            else if (v.newValue < 1) heightField.value = 1;
        });
        
        root.Add(heightField);
        
        var createCubePlane = new Button()
        {
            name = "createMatrixBoundary",
            text = "Add matrix boundary"
        };
        root.Add(createCubePlane);
        
        var spacer = new VisualElement
        {
            style =
            {
                height = 20
            }
        };
        
        root.Add(spacer);

        var selectPath = new Button()
        {
            name = "selectPath",
            text = "Select path where matrix will be stored"
        };
        root.Add(selectPath);
        
        var fileName = new TextField()
        {
            name = "fileName",
            label = "Enter filename:"
        };
        fileName.RegisterValueChangedCallback((v) =>
        {
            _filename = v.newValue + ".py";
        });
        fileName.SetEnabled(false);
        root.Add(fileName);
        
        var exportToPythonSet = new Button()
        {
            name = "exportToPythonSet",
            text = "Export to Python set"
        };
        exportToPythonSet.SetEnabled(false);
        root.Add(exportToPythonSet);
        
        SetupButtonHandler();
    }

    private void SetupButtonHandler()
    {
        VisualElement root = rootVisualElement;

        var buttons = root.Query<Button>();
        buttons.ForEach(RegisterHandler);
    }

    private void RegisterHandler(Button button)
    {
        if (button.name == "createMatrixBoundary")
        {
            button.RegisterCallback<ClickEvent>(CreateInnerObstaclePreview);
            button.RegisterCallback<ClickEvent>(CreateMatrixFloor);
        }

        if (button.name == "exportToPythonSet")
        {
            button.RegisterCallback<ClickEvent>(ExportToPythonSet);
        }

        if (button.name == "selectPath")
        {
            button.RegisterCallback<ClickEvent>(SelectPath);
        }
    }

    private void SelectPath(ClickEvent evt)
    {
        Path = EditorUtility.OpenFolderPanel("Select path to store Matrix in", "~/", "");
        Debug.Log(Path);
    }

    private void ExportToPythonSet(ClickEvent evt)
    {

        if (_filename == "")
        {
            Debug.Log("Empty filename");
            return;
        }
        string pythonMatrixSet = "Matrix = {";

        foreach (Transform transformLayerChild in _innerBoundaries.transform)
        {
            foreach (Transform boundaryChild in transformLayerChild)
            {
                if (boundaryChild.CompareTag("innerBoundaryCube"))
                {
                    pythonMatrixSet += "(" + (int)(boundaryChild.position.x / 1.5) + "," +
                                        (int)((boundaryChild.position.y / 1.5) + 1) + "," +
                                        (int)(boundaryChild.position.z / 1.5) + "), ";  
                }
            }
        }
        pythonMatrixSet = pythonMatrixSet.Remove(pythonMatrixSet.Length - 2, 2);
        pythonMatrixSet += "}";
            
        File.WriteAllText(_path + _filename, pythonMatrixSet);
    }

    private void CreateInnerObstaclePreview(ClickEvent evt)
    {
        var root = rootVisualElement;
        
        var width = root.Q<UnsignedIntegerField>("widthField").value;
        var length = root.Q<UnsignedIntegerField>("lengthField").value;
        var height = root.Q<UnsignedIntegerField>("heightField").value;

        if (_matrixFloor != null)
        {
            if (_matrixFloor.GetComponent<Renderer>().bounds.size.x == width * 1.5f && _matrixFloor.GetComponent<Renderer>().bounds.size.z == length * 1.5f)
            {
                Debug.Log("Already created!");
                return;
            }
        }
        
        if (_innerBoundaries != null)
        {
            Destroy(_innerBoundaries);
        }
        
        _innerBoundaries = new GameObject("MatrixContainer");
        for (var y = 1; y < height - 1; y++)
        {
            var layer = new GameObject("Y Matrix Layer " + y);
            
            for (var x = 1; x < width - 1; x++)
            {
                for (var z = 1; z < length - 1; z++)
                {
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position =  new Vector3(x * 1.5f, (y - 1) * 1.5f, z * 1.5f);
                    sphere.transform.SetParent(layer.transform);
                    sphere.tag = "innerBoundarySphere";
                }
            }    
            
            layer.transform.SetParent(_innerBoundaries.transform);
        }
    }
    
    private void CreateMatrixFloor(ClickEvent evt)
    {
        var root = rootVisualElement;
        
        var width = root.Q<UnsignedIntegerField>("widthField").value;
        var length = root.Q<UnsignedIntegerField>("lengthField").value;
        var amountOfSpheres = (x: width - 2, z: length - 2);
        
        // if matrix floor dimensions are different, destroy and create new, otherwise return.
        if (_matrixFloor != null)
        {
            if (_matrixFloor.GetComponent<Renderer>().bounds.size.x == width * 1.5f && _matrixFloor.GetComponent<Renderer>().bounds.size.z == length * 1.5f)
            {
                Debug.Log("Already created!");
                return;
            }

            Destroy(_matrixFloor);
        }
        _matrixFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _matrixFloor.name = "MatrixFloor";
        _matrixFloor.transform.localScale = new Vector3(width * 1.5f, 2, length * 1.5f);
        _matrixFloor.transform.position = new Vector3(((amountOfSpheres.x * 1.5f) + 1.5f) / 2, -2, ((amountOfSpheres.z * 1.5f) + 1.5f) / 2);
    }

    private void ActivateExportButton()
    {
        if (_path == "")
        {
            return;
        }
        
        var root = rootVisualElement;
        var exportButton = root.Q<Button>("exportToPythonSet");
        var filenameField = root.Q<TextField>("fileName");
        exportButton.SetEnabled(true);
        filenameField.SetEnabled(true);
    }
}
