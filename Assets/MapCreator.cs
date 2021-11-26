using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MapCreator : MonoBehaviour
{
    [Header("Spatial terrain info")] 
    public int top;
    public int bottom;
    public int left;
    public int right;
    // Or just reference the terrain, surely this info exists?
    public Terrain terrain;
    private TerrainData terrainData;
    public float circleSamplerRadius;

    [Header("Map output")] 
    public int mapDimensionSize;

    void Start()
    {
        Debug.Log("start...");
        CreateMap();
    }

    public void CreateMap()
    {
        Debug.Log("Going into createMap");
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("No Terrain available!", "Please put your terrain into the MapCreator script", "Ok");
            return;
        }
        terrainData = terrain.terrainData;
        byte[] bytes;
        int index = 0;
        Texture2D meanHeightMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
            TextureFormat.ARGB32, false);
        float[,] rawHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        
        // Run through the array row by row
        int maxHeight = meanHeightMap.height;
        int maxWidth = meanHeightMap.width;
        for (int y = 4; y < 5; y++)
        {
            for (int x = 4; x < 5; x++)
            {
                Debug.Log("ok about to enter getsurroundingcolor");
                // For each pixel... sample the values in a circle with some radius around you.
                var color = new Vector4(rawHeights[x, y], rawHeights[x, y], rawHeights[x, y], 1);
                GetSurroundingColors(x, y, maxHeight, maxWidth);
                meanHeightMap.SetPixel(x,y, color);
                index++;
            }
        }
    }

    void GetSurroundingColors(int x, int y, int maxY, int maxX )
    {
        // First get bounding rectangle
        var r = circleSamplerRadius;
        // top, right, bottom, left (clockwise)
        float[,] rectangle = new float[,] { { x, Mathf.Ceil(y+r) }, { Mathf.Ceil(x+r), y }, { x, Mathf.Ceil(y-r) }, { Mathf.Ceil(x-r), y } };
        // Make sure rectangle doesn't go outside of image borders
        if (rectangle[0, 1] < 0) { rectangle[0, 1] = 0; }

        if (rectangle[1, 0] > maxX) { rectangle[1, 0] = maxX; }

        if (rectangle[2, 1] > maxY) { rectangle[2, 1] = maxY; }
        
        if (rectangle[3, 0] < 0) { rectangle[3, 0] = 0; }
        
        Debug.Log("rectangle is prepared");
        // Now get the indices of the pixels we are interested in
        List<Tuple<int, int>> indices = new List<Tuple<int, int>>();
        for (int x_cur = (int)rectangle[3,0]; x_cur < rectangle[1,0]; x++)
        {
            for (int y_cur = (int)rectangle[0,1]; y_cur < rectangle[2,1]; y++)
            {
                Debug.Log("now we are in the for loop");
                double dx = x_cur - x;
                double dy = y_cur - y;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= r*r)
                {
                    indices.Add(new Tuple<int, int>(x,y));
                    Debug.Log("Just added " + x + " and " + y);
                }
            }
        }
        
    }

}
