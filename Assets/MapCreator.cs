using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    void Start()
    {
        CreateMeanHeightMap();
    }

    public void CreateMeanHeightMap()
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
        Debug.Log("rawheihgts size: " + rawHeights.Length);
        // Run through the array row by row
        int maxHeight = meanHeightMap.height;
        int maxWidth = meanHeightMap.width;
        Debug.Log("uuh maxwidth "+ maxWidth);
        // Create mean height map
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                // For each pixel... sample the values in a circle with some radius around you.
                //var color = new Vector4(rawHeights[x, y], rawHeights[x, y], rawHeights[x, y], 1);
                Vector4 color = GetSurroundingColors(x, y, maxHeight, maxWidth, rawHeights);
                meanHeightMap.SetPixel(x,y, color);
                index++;
            }
        }
        // Apply all SetPixel calls
        meanHeightMap.Apply();
 
        string path = EditorUtility.SaveFilePanel(
            "Save texture as",
            "",
            "MeanHeightMap",
            "png, jpg");
 
        var extension = Path.GetExtension(path);
        byte[] pngData = null;// duplicateHeightMap.EncodeToPNG();
 
        switch(extension)
        {
            case ".jpg":
                pngData = meanHeightMap.EncodeToJPG();
                break;
 
            case ".png":
                pngData = meanHeightMap.EncodeToPNG();
                break;
        }
 
        if (pngData != null)
        {
            File.WriteAllBytes(path, pngData);
            EditorUtility.DisplayDialog("Heightmap Duplicated", "Saved as" + extension + " in " + path, "Pog");
        }else
        {
            EditorUtility.DisplayDialog("Failed to duplicate height map", "eh something happen hu? lol", "Check Script");
        }
 
        AssetDatabase.Refresh();
    }

    Vector4 GetSurroundingColors(int x, int y, int maxY, int maxX, float[,] rawHeights )
    {
        // First get bounding rectangle
        var r = circleSamplerRadius;
        // top, right, bottom, left (clockwise)
        float x_mid = x + 0.5f;
        float y_mid = y + 0.5f;
        float[,] rectangle = new float[,] { { x, y-r }, { x+r, y }, { x, y+r }, { x-r, y } };
        // Make sure rectangle doesn't go outside of image borders
        if (rectangle[0, 1] < 0) { rectangle[0, 1] = 0; }
        
        if (rectangle[1, 0] > maxX) { rectangle[1, 0] = maxX; }

        if (rectangle[2, 1] > maxY) { rectangle[2, 1] = maxY; }
        
        if (rectangle[3, 0] < 0) { rectangle[3, 0] = 0; }
       
        // Now get the indices of the pixels we are interested in
        List<Vector2> indices = new List<Vector2>();
        List<float> distances = new List<float>();
        var rr = r * r;
        for (int x_cur = (int)rectangle[3,0]; x_cur < rectangle[1,0]; x_cur++)
        {
            for (int y_cur = (int)rectangle[0,1]; y_cur < rectangle[2,1]; y_cur++)
            {
                double dx = x_cur + 0.5f - x_mid;
                double dy = y_cur + 0.5f - y_mid;
                double distanceSquared = dx * dx + dy * dy;

                if (distanceSquared <= rr)
                {
                    // This means we are inside the circle
                    indices.Add(new Vector2(x_cur,y_cur));
                    if (distanceSquared == 0)
                    {
                        distances.Add(1);
                    }
                    else
                    {
                        distances.Add((float)distanceSquared);
                    }
                    
                }
            }
        }
        // Now we know which pixels are within our range - let's calculate how much of them are surrounded by circle
        float colorSum = 0;
        int pixelsTouched = indices.Count;
        
        for(int i = 0; i < pixelsTouched; i++ )
        {
            // Okay, don't sample random points, we dont care about cover percent just distance
            colorSum += rawHeights[(int)indices[i].x, (int)indices[i].y] / (pixelsTouched);
        }
        return new Vector4(colorSum, colorSum, colorSum, 1);
    }

}
