using UnityEngine;

namespace BumperCars.CustomFlatRide
{
    public class Main : IMod
    {
        private GameObject _go;

        public void onEnabled()
        {
            _go = new GameObject();

            _go.AddComponent<FlatRidesLoader>();
            _go.GetComponent<FlatRidesLoader>().Path = Path;
            _go.GetComponent<FlatRidesLoader>().Identifier = Identifier;
            _go.GetComponent<FlatRidesLoader>().LoadDodgem();
        }

        public void onDisabled()
        {
            _go.GetComponent<FlatRidesLoader>().UnloadScenery();

            Object.Destroy(_go);
        }

        public string Name { get { return "Dodgems"; } }
        public string Description { get { return "Dodgems flat ride"; } }
        public string Path { get; set; }
        public string Identifier { get; set; }
    }
}

