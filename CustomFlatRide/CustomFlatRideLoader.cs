using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BumperCars.CustomFlatRide
{
    public class CustomFlatRideLoader : MonoBehaviour
    {
        private List<BuildableObject> _sceneryObjects = new List<BuildableObject>();

        public string Path;
        public string Identifier;
        public FlatRide FlatRideComponent;
    
        public T  LoadAsset<T>(string prefabName) where T : Object
        {
            try
            {
                T asset;

                char dsc = System.IO.Path.DirectorySeparatorChar;

                using (WWW www = new WWW("file://" + Path + dsc + "assetbundle" + dsc + "bumpercar"))
                {
                    if (www.error != null)
                        throw new Exception("Loading had an error:" + www.error);

                    AssetBundle bundle = www.assetBundle;
                
                    try
                    {
                        asset = bundle.LoadAsset<T>(prefabName);

                        bundle.Unload(false);

                        return asset;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);

                        LogException(e);
                        bundle.Unload(false);
                        return null;
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
                    Waypoint wp = new Waypoint {localPosition = new Vector3(x, 0, y)};

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

        public void BasicFlatRideSettings(FlatRide flatRideScript, string displayName, float price, float excitement, float intensity, float nausea, int x, int z)
        {
            _sceneryObjects.Add(flatRideScript);
            AssetManager.Instance.registerObject(flatRideScript);
            flatRideScript.fenceGO = AssetManager.Instance.rideFenceGO;
            flatRideScript.entranceGO = AssetManager.Instance.rideEntranceGO;
            flatRideScript.exitGO = AssetManager.Instance.rideExitGO;
            typeof(FlatRide).GetField("entranceExitBuilderGO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(flatRideScript, AssetManager.Instance.flatRideEntranceExitBuilderGO);
            flatRideScript.price = price;
            flatRideScript.excitementRating = excitement;
            flatRideScript.intensityRating = intensity;
            flatRideScript.nauseaRating = nausea;
            flatRideScript.setDisplayName(displayName);
            flatRideScript.xSize = x;
            flatRideScript.zSize = z;
        }

        public void AddColors(GameObject asset, Color[] c)
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

        public IEnumerator AddBoundingBox(GameObject asset , float x, float z)
        {
            while (!GameController.Instance.isLoadingGame)
            {
                BoundingBox bb = asset.AddComponent<BoundingBox>();
                bb.layers = BoundingVolume.Layers.Buildvolume;
                Bounds b = new Bounds();
                b.center = new Vector3(0, 1, 0);
                b.size = new Vector3(x - .01f, 2, z - .01f);
                bb.setBounds(b);

                yield return null;
            }
        }
    
    }
}

