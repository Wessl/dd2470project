using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("General info parameters")]
    [Tooltip("Should include a mesh somewhere")]
    public GameObject plantObject;
    [Tooltip("Radius")] 
    public float zoneOfInfluence;
    [Header("Adaptability parameters")] 
    public AnimationCurve height;
    public AnimationCurve slope;
    public AnimationCurve moisture;
    public AnimationCurve interaction;
    
}
