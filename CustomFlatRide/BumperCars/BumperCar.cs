using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BumperCar : MonoBehaviour
{
    private GameObject _physicsCar;

    private BumperCars _bumperCars;

    void Awake()
    {
        _bumperCars = GetComponentInParent<BumperCars>();

        _physicsCar = new GameObject();
        _physicsCar.AddComponent<BumperCarAI>();
        _physicsCar.GetComponent<BumperCarAI>().BumperCars = _bumperCars;
        _physicsCar.transform.position = transform.position;
        _physicsCar.transform.rotation = transform.rotation;
    }

    void FixedUpdate()
    {
        _physicsCar.SetActive(_bumperCars.currentState == BumperCars.State.RUNNING);

        transform.position = _physicsCar.transform.position;
        transform.rotation = _physicsCar.transform.rotation;
    }

    void OnDestroy()
    {
        Destroy(_physicsCar.gameObject);
    }
}
