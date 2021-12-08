using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("General info parameters")]
    [Tooltip("Should include a mesh somewhere")]
    public GameObject plantObjectWithMesh;
    [Tooltip("Radius")] 
    public float zoneOfInfluence;
    [Header("Adaptability parameters")] 
    public AnimationCurve height;
    public AnimationCurve slope;
    public AnimationCurve moisture;
    public AnimationCurve interaction;

    public AnimationCurve[] GetCurves()
    {
        return new AnimationCurve[] {height, slope, moisture, interaction};
    }
}

