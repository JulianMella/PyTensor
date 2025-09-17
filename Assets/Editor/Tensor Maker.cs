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

    private const uint MaxDimensionValue = TensorConstants.MaxDimensionValue;
    private const uint MinDimensionValue = TensorConstants.MinDimensionValue;
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
            value = MinDimensionValue,
        };
        widthField.RegisterValueChangedCallback((v) =>
        {
            if (v.newValue > MaxDimensionValue) widthField.value = MaxDimensionValue;
            else if (v.newValue < MinDimensionValue)  widthField.value = MinDimensionValue;
        });
        
        root.Add(widthField);
        
        var lengthField = new UnsignedIntegerField()
        {
            label = "Enter length: ",
            name = "lengthField",
            value = MinDimensionValue
        };

        lengthField.RegisterValueChangedCallback((v) =>
        {
            if (v.newValue > MaxDimensionValue) lengthField.value = MaxDimensionValue;
            else if (v.newValue < MinDimensionValue)  lengthField.value = MinDimensionValue;
        });
        
        root.Add(lengthField);

        var heightField = new UnsignedIntegerField()
        {
            label = "Enter height: ",
            name = "heightField",
            value = MinDimensionValue
        };

        heightField.RegisterValueChangedCallback((v) =>
        {
            if (v.newValue > MaxDimensionValue) heightField.value = MaxDimensionValue;
            else if (v.newValue < MinDimensionValue) heightField.value = MinDimensionValue;
        });
        
        root.Add(heightField);
        
        var createCubePlane = new Button()
        {
            name = "createTensorBoundary",
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
        if (button.name == "createTensorBoundary")
        {
            button.RegisterCallback<ClickEvent>(CallTensorSpawner);
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
        Path = EditorUtility.OpenFolderPanel("Select path to store Tensor in", "~/", "");
        Debug.Log(Path);
    }

    private void ExportToPythonSet(ClickEvent evt)
    {

        if (_filename == "")
        {
            Debug.Log("Empty filename");
            return;
        }
        string pythonTensorSet = "Tensor = {";

        foreach (Transform transformLayerChild in _innerBoundaries.transform)
        {
            foreach (Transform boundaryChild in transformLayerChild)
            {
                if (boundaryChild.CompareTag("innerBoundaryCube"))
                {
                    pythonTensorSet += "(" + (int)(boundaryChild.position.x / 1.5) + "," +
                                       (int)((boundaryChild.position.y / 1.5) + 1) + "," +
                                       (int)(boundaryChild.position.z / 1.5) + "), ";  
                }
            }
        }
        pythonTensorSet = pythonTensorSet.Remove(pythonTensorSet.Length - 2, 2);
        pythonTensorSet += "}";
            
        File.WriteAllText(_path + _filename, pythonTensorSet);
    }

    private void CallTensorSpawner(ClickEvent evt)
    {
        TensorSpawner spawner = FindFirstObjectByType<TensorSpawner>();
        if (spawner != null)
        {
            var root = rootVisualElement;
        
            var width = root.Q<UnsignedIntegerField>("widthField").value;
            var length = root.Q<UnsignedIntegerField>("lengthField").value;
            var height = root.Q<UnsignedIntegerField>("heightField").value;
            
            _innerBoundaries = spawner.SpawnTensor(width, length, height);    
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
