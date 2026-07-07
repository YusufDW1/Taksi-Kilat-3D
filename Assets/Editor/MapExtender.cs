using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MapExtender
{
    public static string ProcessCurrentScene()
    {
        int deletedDuplicates = 0;
        int deletedWalls = 0;
        int generatedObjects = 0;
        
        // 1. Delete Duplicates
        var renderers = Object.FindObjectsOfType<MeshRenderer>();
        var posMap = new Dictionary<string, List<GameObject>>();
        
        foreach(var r in renderers) {
            if(r.gameObject.activeInHierarchy) {
                var meshFilter = r.GetComponent<MeshFilter>();
                if(meshFilter != null && meshFilter.sharedMesh != null) {
                    Vector3 p = r.transform.position;
                    string key = meshFilter.sharedMesh.name + "_" + System.Math.Round(p.x, 2) + "_" + System.Math.Round(p.y, 2) + "_" + System.Math.Round(p.z, 2);
                    if(!posMap.ContainsKey(key)) {
                        posMap[key] = new List<GameObject>();
                    }
                    posMap[key].Add(r.gameObject);
                }
            }
        }
        foreach(var kv in posMap) {
            if(kv.Value.Count > 1) {
                for(int i = 1; i < kv.Value.Count; i++) {
                    Object.DestroyImmediate(kv.Value[i]);
                    deletedDuplicates++;
                }
            }
        }
        
        // 2. Delete Walls and find anchor points
        var allObjs = Object.FindObjectsOfType<GameObject>();
        List<Vector3> wallPositions = new List<Vector3>();
        foreach(var obj in allObjs) {
            string n = obj.name.ToLower();
            if(n.Contains("wall 1") || n.Contains("wall 2") || n.Contains("wall 3") || n.Contains("wall 5")) {
                wallPositions.Add(obj.transform.position);
                Object.DestroyImmediate(obj);
                deletedWalls++;
            }
        }
        
        // 3. Procedural Map Extension
        if (wallPositions.Count > 0)
        {
            Vector3 anchor = wallPositions[0]; // simplistic anchor
            // Load prefabs
            GameObject roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Floor/Street 1 Prefab.prefab");
            GameObject floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/POLYGON city pack/Prefabs/Floor/Grass_stone_1_prefab.prefab");
            
            string[] bldgGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGON city pack/Prefabs/Buildings" });
            List<GameObject> buildings = new List<GameObject>();
            foreach(var g in bldgGuids) {
                buildings.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)));
            }
            
            string[] propGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGON city pack/Prefabs/Props" });
            List<GameObject> props = new List<GameObject>();
            foreach(var g in propGuids) {
                props.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)));
            }
            
            if (roadPrefab != null && buildings.Count > 0)
            {
                float gridSpacing = 8.5f; // Estimated spacing for Polygon City road
                GameObject parent = new GameObject("Procedural_Map_Extension");
                
                // 4x4 Grid extension
                for(int x = 0; x < 4; x++) {
                    for(int z = 0; z < 4; z++) {
                        Vector3 pos = anchor + new Vector3(x * gridSpacing, 0, z * gridSpacing);
                        
                        // Roads in a cross pattern
                        if (x == 1 || z == 1) {
                            var road = PrefabUtility.InstantiatePrefab(roadPrefab) as GameObject;
                            road.transform.position = pos;
                            road.transform.parent = parent.transform;
                            generatedObjects++;
                        }
                        else {
                            if (floorPrefab != null) {
                                var floor = PrefabUtility.InstantiatePrefab(floorPrefab) as GameObject;
                                floor.transform.position = pos;
                                floor.transform.parent = parent.transform;
                                generatedObjects++;
                            }
                            
                            if (Random.value > 0.3f && buildings.Count > 0) {
                                var bldg = PrefabUtility.InstantiatePrefab(buildings[Random.Range(0, buildings.Count)]) as GameObject;
                                bldg.transform.position = pos;
                                bldg.transform.parent = parent.transform;
                                generatedObjects++;
                            }
                            
                            if (Random.value > 0.4f && props.Count > 0) {
                                var prop = PrefabUtility.InstantiatePrefab(props[Random.Range(0, props.Count)]) as GameObject;
                                prop.transform.position = pos + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                                prop.transform.parent = parent.transform;
                                generatedObjects++;
                            }
                        }
                    }
                }
            }
        }
        
        return "Deleted Duplicates: " + deletedDuplicates + ". Deleted Walls: " + deletedWalls + ". Generated Objects: " + generatedObjects;
    }
}
