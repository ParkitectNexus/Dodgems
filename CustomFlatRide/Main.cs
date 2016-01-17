using UnityEngine;


public class Main : IMod
{
    public string Name { get { return "Dodgems"; } }
    public string Description { get { return "Dodgems flat ride"; } }
    public string Path { get; set; }
    public string Identifier { get; set; }

    private GameObject _go;

    public void onEnabled()
    {
        _go = new GameObject();

        _go.AddComponent<BumperCarsLoader>();
        _go.GetComponent<BumperCarsLoader>().Path = Path;
        _go.GetComponent<BumperCarsLoader>().Identifier = Identifier;
        _go.GetComponent<BumperCarsLoader>().LoadBumperCars();
    }

    public void onDisabled()
    {
        _go.GetComponent<BumperCarsLoader>().UnloadScenery();

        Object.Destroy(_go);
    }
}

