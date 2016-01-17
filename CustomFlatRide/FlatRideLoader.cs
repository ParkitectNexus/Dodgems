using UnityEngine;

namespace BumperCars.CustomFlatRide
{
    public class FlatRidesLoader : CustomFlatRideLoader
    {
        public void LoadBumperCars()
        {
            GameObject asset = LoadAsset<GameObject>("BumperCars");

            asset = Instantiate(asset);

            FlatRideScript.BumperCars bumperCars = asset.AddComponent<FlatRideScript.BumperCars>();
            bumperCars.Tune = LoadAsset<AudioClip>("bc_tune");

            AddColors(asset, new []{ ConvertColor(161, 1, 1), ConvertColor(220, 205, 7), ConvertColor(112, 112, 112), ConvertColor(76, 76, 76) });
            BasicFlatRideSettings(bumperCars, "Dodgems", 800, .75f, .3f, .1f, 6, 6);
            SetWaypoints(asset);
            asset.transform.position = new Vector3(0, 999, 0);
            AddBoundingBox(asset, 6, 6);
        }

        public void LoadDodgem()
        {
            LoadBumperCars();
        }
    }
}


