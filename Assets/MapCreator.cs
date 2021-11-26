using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class MapCreator : MonoBehaviour
{
    // Or just reference the terrain, surely this info exists?
    public Terrain terrain;
    private TerrainData terrainData;
    public float circleSamplerRadius;
    public float perPixelSamplePoints;

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
        // Create mean height map
        for (int y = 4; y < 5; y++)
        {
            for (int x = 4; x < 5; x++)
            {
                Debug.Log("ok about to enter getsurroundingcolor");
                // For each pixel... sample the values in a circle with some radius around you.
                var color = new Vector4(rawHeights[x, y], rawHeights[x, y], rawHeights[x, y], 1);
                GetSurroundingColors(x, y, maxHeight, maxWidth, rawHeights);
                meanHeightMap.SetPixel(x,y, color);
                index++;
            }
        }
    }

    void GetSurroundingColors(int x, int y, int maxY, int maxX, float[,] rawHeights )
    {
        // First get bounding rectangle
        var r = circleSamplerRadius;
        // top, right, bottom, left (clockwise)
        float x_mid = x + 0.5f;
        float y_mid = y + 0.5f;
        float[,] rectangle = new float[,] { { x_mid, Mathf.Ceil(y_mid+r) }, { Mathf.Ceil(x_mid+r), y_mid }, { x_mid, Mathf.Floor(y_mid-r) }, { Mathf.Floor(x_mid-r), y_mid } };
        
        // Make sure rectangle doesn't go outside of image borders
        if (rectangle[0, 1] < 0) { rectangle[0, 1] = 0; }

        if (rectangle[1, 0] > maxX) { rectangle[1, 0] = maxX; }

        if (rectangle[2, 1] > maxY) { rectangle[2, 1] = maxY; }
        
        if (rectangle[3, 0] < 0) { rectangle[3, 0] = 0; }

        Debug.Log("rectangle is prepared");
        // Now get the indices of the pixels we are interested in
        List<Tuple<int, int>> indices = new List<Tuple<int, int>>();
        List<float> surrHeightMapVals = new List<float>();
        List<double> distances = new List<double>();
        var rr = r * r;
        for (int x_cur = (int)rectangle[3,0]; x_cur < rectangle[1,0]; x_cur++)
        {
            for (int y_cur = (int)rectangle[0,1]; y_cur > rectangle[2,1]; y_cur--)
            {
                Debug.Log("now we are in the for loop");
                double dx = x_cur + 0.5f - x_mid;
                double dy = y_cur + 0.5f - y_mid;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= rr)
                {
                    // This means we are inside the circle
                    indices.Add(new Tuple<int, int>(x_cur,y_cur));
                    surrHeightMapVals.Add(rawHeights[x_cur,y_cur]);
                    if (distanceSquared == 0)
                    {
                        distances.Add(1);
                    }
                    else
                    {
                        distances.Add(distanceSquared);
                    }
                    
                }
            }
        }
        // Now we know which pixels are within our range - let's calculate how much of them are surrounded by circle
        double colorSum = 0;
        int pixelsTouched = indices.Count;
        for(int i = 0; i < pixelsTouched; i++ )
        {
            // Sample random points within, how many are within circle?
            int amountInside = 0;
            for (int j = 0; j < perPixelSamplePoints; j++)
            {
                var pixelX = indices[i].Item1;
                var pixelY = indices[i].Item2;
                Vector2 resultPoint = new Vector2(Random.Range(pixelX, 1f), Random.Range(pixelY, 1f));
                // Is it inside circle?
                double dx = resultPoint.x - x_mid;
                double dy = resultPoint.y - y_mid;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= rr)
                {
                    amountInside++;
                }
            }

            float coverPercent = amountInside / perPixelSamplePoints;
            colorSum += rawHeights[indices[i].Item1, indices[i].Item2] * coverPercent / (distances[i] * pixelsTouched);
        }
        Debug.Log("idk if this is gonna work but whatever. color: " + colorSum);
        
    }

}
