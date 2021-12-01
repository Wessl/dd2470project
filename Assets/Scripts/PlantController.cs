using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    [Header("Layer 1 (Large)")] 
    [SerializeField] private Plant[] L1Plants;
    [Header("Layer 2 (Medium)")] 
    [SerializeField] private Plant[] L2Plants;
    [Header("Layer 3 (Small)")] 
    [SerializeField] private Plant[] L3Plants;
}
