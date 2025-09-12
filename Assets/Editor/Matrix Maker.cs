using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PyMatrix : EditorWindow
{
    [MenuItem("Window/UI Toolkit/PyMatrix")]
    public static void ShowPyGrid()
    {
        PyMatrix wnd = GetWindow<PyMatrix>();
        wnd.titleContent = new GUIContent("PyGrid");
    }

    private const int MaxDimensionValue = 100;

    private readonly List<GameObject> _cubePlaneObjects = new();
    private GameObject _boundaryEdges = null;
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

        var createCubePlane = new Button()
        {
            name = "createGridBoundary",
            text = "Add grid boundary"
        };
        root.Add(createCubePlane);
        
        var spacer = new VisualElement();
        spacer.style.height = 20;
        root.Add(spacer);

        var selectPath = new Button()
        {
            name = "selectPath",
            text = "Select path where grid will be stored"
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
        if (button.name == "createGridBoundary")
        {
            button.RegisterCallback<ClickEvent>(CreateGridBoundary);
            button.RegisterCallback<ClickEvent>(CreateInnerObstaclePreview);
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
        Path = EditorUtility.OpenFolderPanel("Select path to store Grid in", "~/", "");
        Debug.Log(Path);
    }

    private void ExportToPythonSet(ClickEvent evt)
    {

        if (_filename == "")
        {
            Debug.Log("Empty filename");
            return;
        }
        string pythonGridSet = "Grid = {";
        
        foreach (Transform childTransform in _innerBoundaries.transform)
        {
            if (childTransform.CompareTag("innerBoundaryCube"))
            {
                pythonGridSet += "(" + (int)(childTransform.position.x / 1.5) + "," +
                                    (int)(childTransform.position.y / 1.5) + "," +
                                    (int)(childTransform.position.z / 1.5) + "), ";  
            }
            
        }
        pythonGridSet = pythonGridSet.Remove(pythonGridSet.Length - 2, 2);
        pythonGridSet += "}";
            
        File.WriteAllText(_path + _filename, pythonGridSet);
    }

    private void CreateInnerObstaclePreview(ClickEvent evt)
    {
        var root = rootVisualElement;
        
        var width = root.Q<UnsignedIntegerField>("widthField").value;
        var length = root.Q<UnsignedIntegerField>("lengthField").value;
        
        if (_innerBoundaries != null)
        {
            Destroy(_innerBoundaries);
        }
        
        _innerBoundaries = new GameObject("InnerBoundaryGroup");

        for (var x = 1; x < width - 1; x++)
        {
            for (var z = 1; z < length - 1; z++)
            {
                var innerBoundary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                innerBoundary.transform.position =  new Vector3(x * 1.5f, 0, z * 1.5f);
                innerBoundary.transform.SetParent(_innerBoundaries.transform);
                innerBoundary.tag = "innerBoundarySphere";
            }
        }
        
    }
    
    private void CreateGridBoundary(ClickEvent evt)
    {
        var root = rootVisualElement;
        
        var width = root.Q<UnsignedIntegerField>("widthField").value;
        var length = root.Q<UnsignedIntegerField>("lengthField").value;
        
        if (_boundaryEdges != null)
        {   
            Destroy(_boundaryEdges);
        }
        
        _boundaryEdges = new GameObject("boundaryGroup");
        
        for (int x = 0; x < width; x++)
        {
            var topEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topEdge.transform.position = new Vector3(x * 1.5f, 0, 0);
            topEdge.transform.SetParent(_boundaryEdges.transform);
            
            var bottomEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottomEdge.transform.position = new Vector3(x * 1.5f, 0, (length - 1) * 1.5f);
            bottomEdge.transform.SetParent(_boundaryEdges.transform);
        }
        
        for (int z = 1; z < length - 1; z++)
        {
            var leftEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftEdge.transform.position = new Vector3(0, 0, z * 1.5f);
            leftEdge.transform.SetParent(_boundaryEdges.transform);
            
            var rightEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightEdge.transform.position = new Vector3((width - 1) * 1.5f, 0, z * 1.5f);
            rightEdge.transform.SetParent(_boundaryEdges.transform);
        }

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
