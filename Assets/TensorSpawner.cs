using System;
using UnityEngine;

public class TensorSpawner : MonoBehaviour
{
    public static event Action<GameObject> OnTensorSpawned;
    private GameObject _tensorFloor = null;
    private GameObject _innerBoundaries = null;
    private const float SpacingBetweenSpheres = 1.5f;
    public GameObject SpawnTensor(uint width, uint length, uint height)
    {
        Debug.Log("Spawning Tensor");

        if (_tensorFloor != null)
        {
            Renderer tensorFloorComponent = _tensorFloor.GetComponent<Renderer>();
            if (tensorFloorComponent.bounds.size.x == width * SpacingBetweenSpheres && 
                tensorFloorComponent.bounds.size.z == length * SpacingBetweenSpheres && 
                _innerBoundaries.transform.childCount == height)
            {
                Debug.Log("Already exists with given parameters!");
                return null;
            }
        }
        
        SpawnFloorBelowTensor(width, length);
        SpawnSphereTensor(width, length, height);
        OnTensorSpawned?.Invoke(_innerBoundaries);
        return _innerBoundaries;
    }

    private void SpawnFloorBelowTensor(uint width, uint length)
    {
        if (_tensorFloor != null)
        {
            Destroy(_tensorFloor);
        }
        
        var amountOfSpheres = (x: width - 2, z: length - 2);
        
        _tensorFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _tensorFloor.name = "TensorFloor";
        _tensorFloor.transform.localScale = new Vector3(width * SpacingBetweenSpheres, 2, length * SpacingBetweenSpheres);
        _tensorFloor.transform.position = new Vector3(((amountOfSpheres.x * SpacingBetweenSpheres) + SpacingBetweenSpheres) / 2, -2, ((amountOfSpheres.z * SpacingBetweenSpheres) + SpacingBetweenSpheres) / 2);
    }

    private void SpawnSphereTensor(uint width, uint length, uint height)
    {
        if (_innerBoundaries != null)
        {
            Destroy(_innerBoundaries);
        }
   
        _innerBoundaries = new GameObject("TensorContainer");
        for (var y = 1; y < height - 1; y++)
        {
            var yLayer = new GameObject("Y Tensor Layer " + y);
       
            for (var x = 1; x < width - 1; x++)
            {
                var xLayer = new GameObject("X Tensor Layer " + x);
                for (var z = 1; z < length - 1; z++)
                {
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position =  new Vector3(x * SpacingBetweenSpheres, (y - 1) * SpacingBetweenSpheres, z * SpacingBetweenSpheres);
                    sphere.transform.SetParent(xLayer.transform);
                    sphere.tag = "innerBoundarySphere";
                    sphere.name = "Z Sphere " + z;
                }
                xLayer.transform.SetParent(yLayer.transform);
            }    
       
            yLayer.transform.SetParent(_innerBoundaries.transform);
        }
    }
}