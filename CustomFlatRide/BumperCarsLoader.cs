using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class BumperCarsLoader : MonoBehaviour
{
    private List<BuildableObject> _sceneryObjects = new List<BuildableObject>();

    public string Path;
    public string Identifier;
    public FlatRide FlatRideComponent;
    
    public void LoadBumperCars()
    {
        GameObject asset = LoadAsset("BumperCars");

        BumperCars bumperCars = asset.AddComponent<BumperCars>();

        SetColors(asset, new[]
        {
            ConvertColor(161, 1, 1),
            ConvertColor(220, 205, 7),
            ConvertColor(112, 112, 112),
            ConvertColor(76, 76, 76)
        });

        BasicFlatRideSettings(bumperCars, "Dodgems", 800, .7f, .4f, .7f, 6, 6);
        SetWaypoints(asset);
        asset.transform.position = new Vector3(0, 999, 0);

        BuildableObject buildableObject = asset.GetComponent<BuildableObject>();

        buildableObject.dontSerialize = true;
        buildableObject.isPreview = true;

        AssetManager.Instance.registerObject(asset.GetComponent<FlatRide>());
    }

    public void BasicFlatRideSettings(FlatRide flatRide, string name, float price, float excitement, float intensity, float nausea, int x, int Z)
    {
        _sceneryObjects.Add(flatRide);
        flatRide.fenceGO = AssetManager.Instance.rideFenceGO;
        flatRide.entranceGO = AssetManager.Instance.rideEntranceGO;
        flatRide.exitGO = AssetManager.Instance.rideExitGO;
        flatRide.entranceExitBuilderGO = AssetManager.Instance.flatRideEntranceExitBuilderGO;
        flatRide.price = price;
        flatRide.excitementRating = excitement;
        flatRide.intensityRating = intensity;
        flatRide.nauseaRating = nausea;
        flatRide.setDisplayName(name);
        flatRide.xSize = x;
        flatRide.zSize = Z;
    }

    public GameObject LoadAsset(string PrefabName)
    {
        try
        {
            GameObject asset = new GameObject();

            char dsc = System.IO.Path.DirectorySeparatorChar;

            using (WWW www = new WWW("file://" + Path + dsc + "assetbundle" + dsc + "bumpercar"))
            {
                if (www.error != null)
                    throw new Exception("Loading had an error:" + www.error);

                AssetBundle bundle = www.assetBundle;

                try
                {
                    asset = Instantiate(bundle.LoadAsset(PrefabName)) as GameObject;

                    return asset;
                }
                catch (Exception e)
                {
                    LogException(e);
                    return null;
                }
                finally
                {
                    bundle.Unload(false);
                }
            }
        }
        catch (Exception e)
        {
            LogException(e);
            return null;
        }
    }

    public void SetWaypoints(GameObject asset)
    {
        Waypoints points = asset.GetComponent<Waypoints>();

        Dictionary<KeyValuePair<float, float>, Waypoint> waypoints = new Dictionary<KeyValuePair<float, float>, Waypoint>();

        for (float x = -2.5f; x <= 2.5f; x += 0.25f)
        {
            for (float y = -2.5f; y <= 2.5f; y += 0.25f)
            {
                Waypoint wp = new Waypoint() {localPosition = new Vector3(x, 0, y)};

                waypoints.Add(new KeyValuePair<float, float>(x, y), wp);

                points.waypoints.Add(wp);
            }
        }

        foreach (KeyValuePair<KeyValuePair<float, float>, Waypoint> pair in waypoints)
        {
            bool outer = false;
            for (float x = -0.25f; x <= 0.25f; x += 0.25f)
            {
                for (float y = -0.25f; y <= 0.25f; y += 0.25f)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    Waypoint otherWp;

                    waypoints.TryGetValue(
                        new KeyValuePair<float, float>(pair.Value.localPosition.x + x, pair.Value.localPosition.z + y),
                        out otherWp);

                    if (otherWp != null)
                    {
                        pair.Value.connectedTo.Add(points.waypoints.FindIndex(a => a == otherWp));
                    }
                    else
                    {
                        outer = true;
                    }
                }
            }

            pair.Value.isOuter = outer;
        }
    }

    public void SetColors(GameObject asset, Color[] c)
    {
        CustomColors cc = asset.AddComponent<CustomColors>();
        cc.setColors(c);

        foreach (Material material in AssetManager.Instance.objectMaterials)
        {
            if (material.name == "CustomColorsDiffuse")
            {
                asset.GetComponentInChildren<Renderer>().sharedMaterial = material;

                // Go through all child objects and recolor		
                Renderer[] renderCollection;
                renderCollection = asset.GetComponentsInChildren<Renderer>();

                foreach (Renderer render in renderCollection)
                {
                    render.sharedMaterial = material;
                }

                break;
            }
        }
    }

    private void LogException(Exception e)
    {
        StreamWriter sw = File.AppendText(Path + @"/mod.log");

        sw.WriteLine(e);

        sw.Flush();

        sw.Close();
    }

    public void UnloadScenery()
    {
        foreach (BuildableObject deco in _sceneryObjects)
        {
            AssetManager.Instance.unregisterObject(deco);
            DestroyImmediate(deco.gameObject);
        }
    }

    public Color ConvertColor(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
}

