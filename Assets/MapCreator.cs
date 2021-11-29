using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Header("Relative height map settings")] 
        [SerializeField] private float magicAddedNumber;
        [Header("Save maps as pictures")] 
        [SerializeField] private bool saveMeanHeightMap;
        [SerializeField] private bool saveRelativeHeightMap;
        private float[,] minAndMaxNeighbourhood;

        void Start()
        {
            float startTime = Time.realtimeSinceStartup;
            Texture2D meanHeightMap = CreateMeanHeightMap();
            CreateRelativeHeightMap(meanHeightMap);
            Debug.Log("Finished creating all maps after " + (Time.realtimeSinceStartup - startTime).ToString("f6") + " seconds.");
        }

        public Texture2D CreateMeanHeightMap()
        {
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("No Terrain available!", "Please put your terrain into the MapCreator script", "Ok");
                return null;
            }
            terrainData = terrain.terrainData;

            Texture2D meanHeightMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
                TextureFormat.ARGB32, false);
            float[,] rawHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
            int maxHeight = meanHeightMap.height;
            int maxWidth = meanHeightMap.width;
            minAndMaxNeighbourhood = new float[maxWidth,maxHeight];
        
            // Create mean height map, Run through the array row by row
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    // For each pixel... sample the values in a circle with some radius around you.
                    Vector4 color = GetSurroundingColors(x, y, maxHeight, maxWidth, rawHeights);
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
            float[,] rawHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
            
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
            
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    // For each pixel... calculate inverse, then subtract inverse value from rawHeights
                    inverse[x, y] = minAndMaxNeighbourhood[x, y] - rawHeights[x, y];
                    var colorVal = rawHeights[x, y] - inverse[x,y] + magicAddedNumber;
                    var color = new Vector4(colorVal, colorVal, colorVal, 1);
                    relativeHeightMap.SetPixel(x,y, color);
                }
            }
            
            if (saveRelativeHeightMap)
            {
                CreatePicture(relativeHeightMap, "relativeHeightMap");
            }
        }

        void CreatePicture(Texture2D meanHeightMap, string pictureName)
        {
            // Apply all SetPixel calls
            meanHeightMap.Apply();
 
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
