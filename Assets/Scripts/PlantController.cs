using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantController : MonoBehaviour
{
    [SerializeField] private MapCreator mapCreator;
    [SerializeField] private int pddSamples;
    [SerializeField] private float threshold;

    [Header("Map input information")] 
    [SerializeField] private Texture2D heightMap;
    private Color[] heightMapColors;
    [SerializeField] private Texture2D moistureMap;
    private Color[] moistureMapColors;
    [SerializeField] private Texture2D densityMap;
    private Color[] densityMapColors;
    [SerializeField] private Texture2D slopeMap;
    private Color[] slopeMapColors;
    [SerializeField] private Texture2D waterMap;
    private Color[] waterMapColors;
    [SerializeField] private Texture2D waterSpreadMap;
    private Color[] waterSpreadMapColors;
    
    [Header("Layer 1 (Large)")] 
    [SerializeField] private Plant[] L1Plants;
    [Tooltip("The index of this should correspond to the plant in the same list. Should be increasing, where the difference between preceeding divided by 1 is the percent chance. Ex: 0.4, 0.75, 1")]
    [SerializeField] private float[] L1CorrespondingPredominanceValue;
    [SerializeField] private float L1Radius;
    
    [Header("Layer 2 (Medium)")] 
    [SerializeField] private Plant[] L2Plants;
    [Tooltip("The index of this should correspond to the plant in the same list. Should be increasing, where the difference between preceeding divided by 1 is the percent chance. Ex: 0.4, 0.75, 1")]
    [SerializeField] private float[] L2CorrespondingPredominanceValue;
    [SerializeField] private float L2Radius;

    [Header("Layer 3 (Small)")] 
    [SerializeField] private Plant[] L3Plants;
    [Tooltip("The index of this should correspond to the plant in the same list. Should be increasing, where the difference between preceeding divided by 1 is the percent chance. Ex: 0.4, 0.75, 1")]
    [SerializeField] private float[] L3CorrespondingPredominanceValue;
    [SerializeField] private float L3Radius;
    
    // some private global vars
    private Vector2 terrainSize;


    public void PlacePlants()
    {
        Debug.Log("beginning plant placement procedure");
        // I'm thinking that I'll have one big poisson disc for the whole terrain with different layers?
        // Step 0. Clear away any old placed plants
        RemoveOldPlants();
        // Step 0,5. Get color per pixel values for each texture map
        PopulateColorArrays();
        // Step 1. Generate PDD for whole terrain
        terrainSize = new Vector2(mapCreator.TerrainData.heightmapResolution-1, mapCreator.TerrainData.heightmapResolution-1);
        Debug.Log("terrainsize? " + terrainSize.ToString());
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(L1Radius, terrainSize, pddSamples);
        Debug.Log("we got how many points " + points.Count);
        // Step 2. evaluate position to determine which plant to place in each position on the terrain
        foreach (var point in points)
        {
            // Pixel starts at top left corner, so if we floor to int we end up at correct pixel position definition
            EvaluatePosition(point.x, point.y, 1);
        }
    }

    private void RemoveOldPlants()
    {
        var oldPlants = this.GetComponentsInChildren<Transform>();
        for (int i = 0; i < oldPlants.Length; i++)
        {
            if (oldPlants[i] == null) continue;
            if (oldPlants[i].CompareTag("Plant"))
            {
                DestroyImmediate(oldPlants[i].gameObject);
            }
        }
    }

    /*
     * As described in paper 1, for details refer to it please. 
     */
    private void EvaluatePosition(float xRaw, float yRaw, int layerIndex)
    {
        // Save some variables
        int x = Mathf.FloorToInt(xRaw);
        int y = Mathf.FloorToInt(yRaw);
        int maxHeight = (int)terrainSize.y-2;
        float p = 1;
        try
        {
            p = p * waterMapColors[y * maxHeight + x].r;
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log("index " + x + "," + y);
        }
        Plant plant = GetPlant();
        // Curve ordering: height, slope, moisture, interaction
        AnimationCurve[] curves = plant.GetCurves();
        if (layerIndex > 1)
        {
            // density map calculation here
            // another density map calculation here
        }

        p = p * curves[0].Evaluate(heightMapColors[y * maxHeight + x].r);
        p = p * curves[1].Evaluate(slopeMapColors[y * maxHeight + x].r);
        p = p * curves[2].Evaluate(moistureMapColors[y * maxHeight + x].r);
        if (p >= threshold)
        {
            // god damn it was actually placed. I think here we just physically plop down a tree at this pos
            Debug.Log("Placed down a " + plant.transform.name);// + " at " + x + "," + y);
            PlaceTreeOnTerrain(xRaw,yRaw,plant);
        }
    }

    private void PlaceTreeOnTerrain(float x, float y, Plant plant)
    {
        
        Vector3 terrainSize = mapCreator.TerrainData.size;
        Debug.Log(terrainSize);
        float diffX = x / mapCreator.TerrainData.heightmapResolution;
        float diffY = y / mapCreator.TerrainData.heightmapResolution;
        float newX = x * terrainSize.x / mapCreator.TerrainData.heightmapResolution;
        float newY = y * terrainSize.x / mapCreator.TerrainData.heightmapResolution;
        float height = mapCreator.TerrainData.GetInterpolatedHeight(diffX, diffY);
        var newPlant = Instantiate(plant.plantObject, new Vector3(newX,height, newY), Quaternion.identity);
        newPlant.transform.parent = this.transform;

    }

    private void PopulateColorArrays()
    {
        heightMapColors = heightMap.GetPixels();
        //densityMapColors = densityMap.GetPixels();
        moistureMapColors = moistureMap.GetPixels();
        slopeMapColors = slopeMap.GetPixels();
        waterMapColors = waterMap.GetPixels();
        waterSpreadMapColors = waterSpreadMap.GetPixels();
    }

    private Plant GetPlant()
    {
        // Stochastically sample plant, depending on predominance value
        float val = Random.Range(0f, 1f);
        for (int i = 0; i < L1CorrespondingPredominanceValue.Length; i++)
        {
            if (val < L1CorrespondingPredominanceValue[i])
            {
                return L1Plants[i];
            }
        }
        Debug.Log("Error: No plant was correctly sampled. Returning first in array.");
        return L1Plants[0];
    }
}
