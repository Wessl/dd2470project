using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


namespace Assets
{
    public class MapCreator : MonoBehaviour
    {
        // Or just reference the terrain, surely this info exists?
        [SerializeField] private Terrain terrain;
        private TerrainData terrainData;
        [SerializeField] private float circleSamplerRadius;
        [Range(1, 20)]
        [SerializeField] private int slopeMapDistSampling = 1;
        
        [Header("Save maps as pictures")] 
        [SerializeField] private bool saveMeanHeightMap;
        [SerializeField] private bool saveRelativeHeightMap;
        [SerializeField] private bool saveSlopeMap;
        private float[,] minAndMaxNeighbourhood;
        private float[,] rawHeights;

        void Start()
        {
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("No Terrain available!", "Please put your terrain into the MapCreator script", "Ok");
                return;
            }
            terrainData = terrain.terrainData;
            rawHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
            float startTime = Time.realtimeSinceStartup;
            Texture2D meanHeightMap = CreateMeanHeightMap();
            CreateRelativeHeightMap(meanHeightMap);
            CreateSlopeMap();
            Debug.Log("Finished creating all maps after " + (Time.realtimeSinceStartup - startTime).ToString("f6") + " seconds.");
        }

        private Texture2D CreateMeanHeightMap()
        {
            Texture2D meanHeightMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
                TextureFormat.ARGB32, false);
            int maxHeight = meanHeightMap.height;
            int maxWidth = meanHeightMap.width;
            minAndMaxNeighbourhood = new float[maxWidth,maxHeight];
        
            // Create mean height map, Run through the array row by row
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    // For each pixel... sample the values in a circle with some radius around you.
                    Vector4 color = GetSurroundingColors(x, y, maxHeight, maxWidth);
                    meanHeightMap.SetPixel(x,y, color);
                }
            }

            if (saveMeanHeightMap)
            {
                CreatePicture(meanHeightMap, "meanHeightMap");
            }
        
            return meanHeightMap;
        }
        
        /*
         *  https://www.researchgate.net/publication/261550103_Semantic_calibration_of_digital_terrain_analysis_scale?enrichId=rgreq-cb88bb3ef20c7892033eb9a83eafba72-XXX&enrichSource=Y292ZXJQYWdlOzI2MTU1MDEwMztBUzoxMDIzNjU3NDcyMjA0OTBAMTQwMTQxNzMwNDgzMg%3D%3D&el=1_x_3&_esc=publicationCoverPdf
         *  Method: Using the sum of the min and max values of the local neighbourhood (area from mean height map),
         *          Calculate inverse = (min + max) - originalHeightMap
         *          Calculate relativeHeightMap = originalHeightMap - inverse
         *  So basically, they (aka the OG authors) was wrong as hell, research destroyed :sunglasses:
         */
        private void CreateRelativeHeightMap(Texture2D meanHeightMap)
        {
            Texture2D relativeHeightMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
                TextureFormat.ARGB32, false);
            if (meanHeightMap == null)
            {
                Debug.Log("There is no mean height map data available.");
                return;
            }
            
            int maxHeight = meanHeightMap.height;
            int maxWidth = meanHeightMap.width;
            float[,] inverse = new float[maxWidth, maxHeight];
            float avgHeight = Utility.JaggedArrAvg(rawHeights);
            Debug.Log("Average height: " + avgHeight);
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    // For each pixel... calculate inverse, then subtract inverse value from rawHeights
                    inverse[x, y] = minAndMaxNeighbourhood[x, y] - rawHeights[x, y];
                    var colorVal = rawHeights[x, y] - inverse[x,y] + avgHeight;
                    var color = new Vector4(colorVal, colorVal, colorVal, 1);
                    relativeHeightMap.SetPixel(x,y, color);
                }
            }
            
            if (saveRelativeHeightMap)
            {
                CreatePicture(relativeHeightMap, "relativeHeightMap");
            }
        }
        
        // Calculate the slope at each position of the map in the X and Y directions
        // Formula, from Hammes' Modeling of ecosystems as a data source for real-time terrain rendering, 2001
        // deltaX[x,y] = (rawHeights[x+1,y] - rawHeights[x-1,y])/2
        // deltaY[x,y] = (rawHeights[x,y+1] - rawHeights[x,y-1])/2
        // Slope = sqrt(deltaX^2 + deltaY^2)
        private void CreateSlopeMap()
        {
            int maxWidth = terrainData.heightmapResolution;
            int maxHeight = terrainData.heightmapResolution;
            float[,] deltaX = new float[maxWidth, maxHeight];
            float[,] deltaY = new float[maxWidth, maxHeight];
            float[,] slope = new float[maxWidth, maxHeight];
            int d = slopeMapDistSampling;
            Texture2D slopeMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
                TextureFormat.ARGB32, false);
            // variables for normalizing
            float min = float.MaxValue;
            float max = 0;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    deltaX[x, y] = (rawHeights[( x + d) % maxWidth, y] - rawHeights[Mathf.Max((x - d),0), y]) / 2;
                    deltaY[x, y] = (rawHeights[x, (y + d) % maxHeight] - rawHeights[x, Mathf.Max((y - d),0)]) / 2;
                    slope[x, y] = Mathf.Sqrt(Mathf.Pow(deltaX[x, y], 2) + Mathf.Pow(deltaY[x, y], 2));
                    if (slope[x, y] > max) max = slope[x, y];
                    if (slope[x, y] < min) min = slope[x, y];
                }
            }
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    // Normalize before getting color
                    slope[x, y] = (slope[x, y] - min) / (max + min);
                    var c = new Color(slope[x, y], slope[x, y], slope[x, y], 1);
                    slopeMap.SetPixel(x,y,c);
                }
            }
            
            if (saveSlopeMap)
            {
                CreatePicture(slopeMap, "slopeMap");
            }
        }

        void CreatePicture(Texture2D mapTexture, string pictureName)
        {
            // Apply all SetPixel calls
            mapTexture.Apply();
 
            string path = EditorUtility.SaveFilePanel(
                "Save texture as",
                "",
                pictureName,
                "png, jpg");
 
            var extension = Path.GetExtension(path);
            byte[] pngData = null;// duplicateHeightMap.EncodeToPNG();
 
            switch(extension)
            {
                case ".jpg":
                    pngData = mapTexture.EncodeToJPG();
                    break;
 
                case ".png":
                    pngData = mapTexture.EncodeToPNG();
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

        Vector4 GetSurroundingColors(int x, int y, int maxY, int maxX)
        {
            // First get bounding rectangle
            float r = circleSamplerRadius;
            float rr = r * r;
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
        
            for (int xCur = (int)rectangle[3,0]; xCur < rectangle[1,0]; xCur++)
            {
                for (int yCur = (int)rectangle[0,1]; yCur < rectangle[2,1]; yCur++)
                {
                    double dx = xCur + 0.5f - x_mid;
                    double dy = yCur + 0.5f - y_mid;
                    double distanceSquared = dx * dx + dy * dy;
                    if (distanceSquared <= rr)
                    {
                        // This means we are inside the circle
                        indices.Add(new Vector2(xCur,yCur));
                    }
                }
            }
            // Now we know which pixels are within our range - let's calculate how much of them are surrounded by circle
            float colorSum = 0;
            int pixelsTouched = indices.Count;
            // Also calculate the sum of the min and max of this neighbourhood
            float min = float.MaxValue;
            float max = 0;

            for( int i = 0; i < pixelsTouched; i++ )
            {
                // For each pixel, what is the height at that pixel? Divide that by the total amount,
                // to get the percentage of contribution to the total color value
                var heightAtPixel = rawHeights[(int) indices[i].x, (int) indices[i].y];
                colorSum += heightAtPixel / (pixelsTouched);
                if (heightAtPixel > max) max = heightAtPixel;
                if (heightAtPixel < min) min = heightAtPixel;
            }

            minAndMaxNeighbourhood[x, y] = min + max;
            return new Vector4(colorSum, colorSum, colorSum, 1);
        }
    }
}
