using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


public class MapCreator : MonoBehaviour
{
    [Tooltip("How far out should the mean height map stretch when sampling a single pixel. Larger values create a smoother map, but takes longer to compute.")]
    [SerializeField] private float circleSamplerRadius;
    [Range(1, 20)]
    [SerializeField] private int slopeMapDistSampling;

    [Header("Manually created maps")] 
    [SerializeField] private Texture2D waterMap;
    [SerializeField] private Texture2D waterSpreadMap;
    
    [Header("Save maps as pictures")] 
    [SerializeField] private bool saveMeanHeightMap;
    [SerializeField] private bool saveRelativeHeightMap;
    [SerializeField] private bool saveSlopeMap;
    [SerializeField] private bool saveMoistureMap;
    
    [Header("Map weights")]
    [SerializeField] [Range(0f, 2f)] private float heightMapWeight;
    [SerializeField] [Range(0f, 2f)] private float slopeMapWeight;
    [SerializeField] [Range(0f, 2f)] private float relativeHeightMapWeight;
    [SerializeField] [Range(0f, 1f)] 
    [Tooltip("Attenuates the direct contribution of the relative height on moisture map")] private float omega;

    [Header("Influence curves")] 
    [SerializeField] private AnimationCurve heightMapCurve;
    [SerializeField] private AnimationCurve slopeMapCurve;
    [SerializeField] private AnimationCurve relativeHeightMapCurve;
    [SerializeField] private AnimationCurve verticalWaterSpreadCurve;
    
    private float[,] minAndMaxNeighbourhood;
    private float[,] rawHeights;
    // global maps
    private Texture2D slopeMap;
    private float[,] rawSlopes;
    private Texture2D relativeHeightMap;
    private float[,] rawRelativeHeights;
    private float[,] rawWaterSpread;
    private float[,] rawWater;
    private float[,] rawMoisture;

    // global vars
    // terrain map dimensions
    private int maxWidth;
    private int maxHeight;

    // the terrain itself (tremendously important)
    [SerializeField] private Terrain terrain;
    private TerrainData terrainData;
    public TerrainData TerrainData => terrainData;


    public void Generate()
    {
        if (terrain == null)
        {
            EditorUtility.DisplayDialog("No Terrain available!", "Please put your terrain into the MapCreator script", "Ok");
            return;
        }
        terrainData = terrain.terrainData;
        maxHeight = terrainData.heightmapResolution;
        maxWidth = terrainData.heightmapResolution;
        rawHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float startTime = Time.realtimeSinceStartup;
        // Start by reading in water and water spread map since those are created separately/manually for now
        ReadWaterMaps();
        CreateMeanHeightMap();
        CreateRelativeHeightMap();
        CreateSlopeMap();
        CreateMoistureMap();
        Debug.Log("Finished creating all maps after " + (Time.realtimeSinceStartup - startTime).ToString("f6") + " seconds.");
    }

    private void CreateMoistureMap()
    {
        Texture2D moistureMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
            TextureFormat.ARGB32, false);
        int maxHeight = moistureMap.height;
        int maxWidth = moistureMap.width;
        rawMoisture = new float[maxWidth, maxHeight];
        // Use all other generated maps and influence curves to create moisture map
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                // 1. Base moisture value. Evaluate height value at coordinate (x,y) on the influence curve of the height and its associated weight
                float baseMoisture = heightMapCurve.Evaluate(rawHeights[x, y]) * heightMapWeight;
                // 2 & 3. Slope influence value and relative height influence value 
                float slopeInfluence = slopeMapCurve.Evaluate(rawSlopes[x, y]) * slopeMapWeight;
                float relativeHeight = relativeHeightMapCurve.Evaluate(rawRelativeHeights[x, y]) * relativeHeightMapWeight;
                // 4. Relative moisture. Based upon topographic characteristics of the terrain represented by relative height and slope maps. 
                // This value acts as an indicator of regions where soil moisture naturally accumulates or declines
                float relativeMoisture = relativeHeight - slopeInfluence + 1;
                // 5. Water spread. Evaluates influence of the Relative Height over the vertical water spread from a water body
                float waterSpread = rawWater[x, y] + rawWaterSpread[x, y] * verticalWaterSpreadCurve.Evaluate(rawRelativeHeights[x, y]);
                // 6. Moisture map is finally computed by compiling values from other maps.
                float moisture = Utility.Saturate((baseMoisture + waterSpread) * relativeMoisture + (relativeHeight * omega)) + waterSpread;
                rawMoisture[x, y] = moisture;
                Vector4 color = new Vector4(moisture, moisture, moisture, 1);
                moistureMap.SetPixel(x,y,color);
            }
        }

        if (saveMoistureMap)
        {
            CreatePicture(moistureMap, "moistureMap");
        }
       
    }

    private void CreateMeanHeightMap()
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
    }
    
    /*
     *  https://www.researchgate.net/publication/261550103_Semantic_calibration_of_digital_terrain_analysis_scale?enrichId=rgreq-cb88bb3ef20c7892033eb9a83eafba72-XXX&enrichSource=Y292ZXJQYWdlOzI2MTU1MDEwMztBUzoxMDIzNjU3NDcyMjA0OTBAMTQwMTQxNzMwNDgzMg%3D%3D&el=1_x_3&_esc=publicationCoverPdf
     *  Method: Using the sum of the min and max values of the local neighbourhood (area from mean height map),
     *          Calculate inverse = (min + max) - originalHeightMap
     *          Calculate relativeHeightMap = originalHeightMap - inverse
     *  So basically, they (aka the OG authors) was wrong as hell, research destroyed :sunglasses:
     */
    private void CreateRelativeHeightMap()
    {
        relativeHeightMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
            TextureFormat.ARGB32, false);

        rawRelativeHeights = new float[maxWidth, maxHeight];
        float[,] inverse = new float[maxWidth, maxHeight];
        float avgHeight = Utility.JaggedArrAvg(rawHeights);
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                // For each pixel... calculate inverse, then subtract inverse value from rawHeights
                inverse[x, y] = minAndMaxNeighbourhood[x, y] - rawHeights[x, y];
                var colorVal = rawHeights[x, y] - inverse[x,y] + avgHeight;
                rawRelativeHeights[x, y] = colorVal;
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
        float[,] deltaX = new float[maxWidth, maxHeight];
        float[,] deltaY = new float[maxWidth, maxHeight];
        rawSlopes = new float[maxWidth, maxHeight];
        int d = slopeMapDistSampling;
        slopeMap = new Texture2D(terrainData.heightmapResolution, terrainData.heightmapResolution,
            TextureFormat.ARGB32, false);
        // variables for normalizing
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                deltaX[x, y] = (rawHeights[Mathf.Min((x + d),maxWidth-1), y] - rawHeights[Mathf.Max((x - d),0), y]) / 2;
                deltaY[x, y] = (rawHeights[x, Mathf.Min((y + d),maxHeight-1)] - rawHeights[x, Mathf.Max((y - d),0)]) / 2;
                rawSlopes[x, y] = Mathf.Sqrt(Mathf.Pow(deltaX[x, y], 2) + Mathf.Pow(deltaY[x, y], 2));
                if (rawSlopes[x, y] > max) max = rawSlopes[x, y];
                if (rawSlopes[x, y] < min) min = rawSlopes[x, y];
            }
        }
        for (int y = 0; y < maxHeight; y++)
        {
            for (int x = 0; x < maxWidth; x++)
            {
                // Normalize before getting color
                rawSlopes[x, y] = (rawSlopes[x, y] - min) / (max + min);
                var c = new Color(rawSlopes[x, y], rawSlopes[x, y], rawSlopes[x, y], 1);
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
        // Apply all SetPixel calls (update mipMaps, dont set to make it unreadable
        mapTexture.Apply(true, false);
        
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
            EditorUtility.DisplayDialog("Failed to duplicate height map", "聖夜のスペシャルシューティングスター", "やばい");
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

    private void ReadWaterMaps()
    {
        // Go over the maps and store the values into raw jagged value arrays
        var waterMapPixels = waterMap.GetPixels();
        var waterSpreadMapPixels = waterSpreadMap.GetPixels();
        rawWater = new float[maxWidth, maxHeight];
        rawWaterSpread = new float[maxWidth, maxHeight];
        int sqrLen = (int)Mathf.Sqrt(waterMapPixels.Length);
        for (int y = 0; y < sqrLen; y++)
        {
            for (int x = 0; x < sqrLen; x++)
            {
                // Get the value at this pixel. Since it's grayscale, R==G==B so it doesn't matter which we use
                rawWater[x, y] = waterMapPixels[y * sqrLen + x].r;
                rawWaterSpread[x, y] = waterSpreadMapPixels[y * sqrLen + x].a;  // ninja note: this one uses alpha instead because the generated water spread map stores the spread on the alpha channel, nothing we can do about it
            }
        }
    }
}
