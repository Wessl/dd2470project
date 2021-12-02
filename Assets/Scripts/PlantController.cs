using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    [SerializeField] private MapCreator mapCreator;
    [SerializeField] private int pddSamples;
    [SerializeField] private bool regenerateMapsBeforePlacement;

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
    [SerializeField] private float L1Radius;
    [Header("Layer 2 (Medium)")] 
    [SerializeField] private Plant[] L2Plants;
    [SerializeField] private float L2Radius;

    [Header("Layer 3 (Small)")] 
    [SerializeField] private Plant[] L3Plants;
    [SerializeField] private float L3Radius;

    
    public void PlacePlants()
    {
        // I'm thinking that I'll have one big poisson disc for the whole terrain with different layers?
        // Step 0. Regenerate maps if user wants to (not really necessary)
        if (regenerateMapsBeforePlacement)
        {
            mapCreator.Generate();
        }
        // Step 0,5. Get color per pixel values for each texture map
        PopulateColorArrays();
        // Step 1. Generate PDD for whole terrain
        Vector2 terrainSize = mapCreator.Dimensions;
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(L1Radius, terrainSize, pddSamples);
        // Step 2. evaluate position to determine which plant to place in each position on the terrain
        foreach (var point in points)
        {
            EvaluatePosition(point.x, point.y);
        }
    }

    /*
     * As described in paper 1, for details refer to it please. 
     */
    private void EvaluatePosition(float x, float y)
    {
        float p = 1;
        // continue here...
    }

    private void PopulateColorArrays()
    {
        heightMapColors = heightMap.GetPixels();
        densityMapColors = densityMap.GetPixels();
        moistureMapColors = moistureMap.GetPixels();
        slopeMapColors = slopeMap.GetPixels();
        waterMapColors = waterMap.GetPixels();
        waterSpreadMapColors = waterSpreadMap.GetPixels();
    }
}
