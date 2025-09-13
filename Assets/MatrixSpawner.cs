using System;
using UnityEngine;

public class MatrixSpawner : MonoBehaviour
{
    public static event Action<GameObject> OnMatrixSpawned;
    private GameObject _matrixFloor = null;
    private GameObject _innerBoundaries = null;
    private const float SpacingBetweenSpheres = 1.5f;
    public GameObject SpawnMatrix(uint width, uint length, uint height)
    {
        Debug.Log("Spawning Matrix");

        if (_matrixFloor != null)
        {
            Renderer matrixFloorComponent = _matrixFloor.GetComponent<Renderer>();
            if (matrixFloorComponent.bounds.size.x == width * SpacingBetweenSpheres && 
                matrixFloorComponent.bounds.size.z == length * SpacingBetweenSpheres && 
                _innerBoundaries.transform.childCount == height)
            {
                Debug.Log("Already exists with given parameters!");
                return null;
            }
        }
        
        SpawnFloorBelowMatrix(width, length);
        SpawnSphereMatrix(width, length, height);
        OnMatrixSpawned?.Invoke(_innerBoundaries);
        return _innerBoundaries;
    }

    private void SpawnFloorBelowMatrix(uint width, uint length)
    {
        if (_matrixFloor != null)
        {
            Destroy(_matrixFloor);
        }
        
        var amountOfSpheres = (x: width - 2, z: length - 2);
        
        _matrixFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _matrixFloor.name = "MatrixFloor";
        _matrixFloor.transform.localScale = new Vector3(width * SpacingBetweenSpheres, 2, length * SpacingBetweenSpheres);
        _matrixFloor.transform.position = new Vector3(((amountOfSpheres.x * SpacingBetweenSpheres) + SpacingBetweenSpheres) / 2, -2, ((amountOfSpheres.z * SpacingBetweenSpheres) + SpacingBetweenSpheres) / 2);
    }

    private void SpawnSphereMatrix(uint width, uint length, uint height)
    {
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
                    sphere.transform.position =  new Vector3(x * SpacingBetweenSpheres, (y - 1) * SpacingBetweenSpheres, z * SpacingBetweenSpheres);
                    sphere.transform.SetParent(layer.transform);
                    sphere.tag = "innerBoundarySphere";
                }
            }    
       
            layer.transform.SetParent(_innerBoundaries.transform);
        }
    }
}