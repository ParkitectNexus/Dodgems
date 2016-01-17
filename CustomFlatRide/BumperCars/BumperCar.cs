using UnityEngine;

namespace BumperCars.CustomFlatRide.BumperCars
{
    public class BumperCar : MonoBehaviour
    {
        private GameObject _physicsCar;

        private BumperCars _bumperCars;

        void Awake()
        {
            _bumperCars = GetComponentInParent<BumperCars>();

            _physicsCar = new GameObject();
            _physicsCar.AddComponent<BumperCarAi>();
            _physicsCar.GetComponent<BumperCarAi>().BumperCars = _bumperCars;
            _physicsCar.transform.position = transform.position;
            _physicsCar.transform.rotation = transform.rotation;
        }

        void Update()
        {
            _physicsCar.SetActive(_bumperCars.CurrentState == BumperCars.State.Running && transform.FindChild("seat").childCount > 0);

            transform.position = _physicsCar.transform.position;
            transform.rotation = _physicsCar.transform.rotation;
        }

        void OnDestroy()
        {
            Destroy(_physicsCar.gameObject);
        }
    }
}
