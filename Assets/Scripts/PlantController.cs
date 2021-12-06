using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantController : MonoBehaviour
{
    [SerializeField] private MapCreator mapCreator;
    [Tooltip("How many samples should the Poisson Disc Distribution use per node before giving up and going to the next one? Good default is ~30 ish, higher values affect runtime quite a bit.")]
    [SerializeField] private int pddSamples;
    [Tooltip("The threshold that which determines if a given plant should be placed after running placement algorithm. Globally affects all plants equally. 1 = Strictest possible, place almost nothing. 0 = Very lenient, place almost anything.")]
    [Range(0f,1.1f)]
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
        // Step 0. Clear away any old placed plants
        RemoveOldPlants();
        // Step 0,5. Get color per pixel values for each texture map
        PopulateColorArrays();
        // Step 1. Generate PDD for whole terrain
        terrainSize = new Vector2(mapCreator.TerrainData.heightmapResolution-1, mapCreator.TerrainData.heightmapResolution-1);
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(L1Radius, terrainSize, pddSamples);
        // Step 2. evaluate positions to determine which plant to place in each position on the terrain
        foreach (var point in points)
        {
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
        // Pixel starts at top left corner, so if we floor to int we end up at correct pixel position definition
        int x = Mathf.FloorToInt(xRaw);
        int y = Mathf.FloorToInt(yRaw);
        int maxHeight = (int)terrainSize.y-1;
        
        float p = 1;
        p = p * (1-waterMapColors[y * maxHeight + x].a);

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
            PlaceTreeOnTerrain(xRaw,yRaw,plant);
        }
    }

    private void PlaceTreeOnTerrain(float x, float y, Plant plant)
    {
        
        Vector3 terrainSize = mapCreator.TerrainData.size;
        // placement variables
        float diffX = x / mapCreator.TerrainData.heightmapResolution;
        float diffY = y / mapCreator.TerrainData.heightmapResolution;
        float newX = diffX * terrainSize.x;
        float newY = diffY * terrainSize.x;
        float height = mapCreator.TerrainData.GetInterpolatedHeight(diffX, diffY);
        // randomly rotate around y axis so that not every tree has exact same rot
        Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        var newPlant = Instantiate(plant.plantObject, new Vector3(newX,height, newY), rot);
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
