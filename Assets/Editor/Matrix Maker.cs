using System.IO;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PyTensor : EditorWindow
{
    [MenuItem("Window/UI Toolkit/PyTensor")]
    public static void ShowPyTensor()
    {
        PyTensor wnd = GetWindow<PyTensor>();
        wnd.titleContent = new GUIContent("PyTensor");
    }

    private const uint MaxDimensionValue = MatrixConstants.MaxDimensionValue;
    private string _path = "";
    private string _filename = "";
    private GameObject _innerBoundaries = null;

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
            label = "Enter width: ",
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
            label = "Enter length: ",
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
            label = "Enter height: ",
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
            text = "Add boundary"
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
            text = "Select path where boundaries will be stored"
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
            button.RegisterCallback<ClickEvent>(CallMatrixSpawner);
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

    private void CallMatrixSpawner(ClickEvent evt)
    {
        MatrixSpawner spawner = FindFirstObjectByType<MatrixSpawner>();
        if (spawner != null)
        {
            var root = rootVisualElement;
        
            var width = root.Q<UnsignedIntegerField>("widthField").value;
            var length = root.Q<UnsignedIntegerField>("lengthField").value;
            var height = root.Q<UnsignedIntegerField>("heightField").value;
            
            _innerBoundaries = spawner.SpawnMatrix(width, length, height);    
        }
        else
        {
            Debug.Log("No spawner found");
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
