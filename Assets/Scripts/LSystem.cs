using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;
using Random = UnityEngine.Random;

public class LSystem : MonoBehaviour
{
    [Header("L System parameters")] 
    public int iterations;
    public float angle;
    public float width;
    public float variance;
    public bool thinnerOverLength;
    [Header("Leaf and Branch lengths")] 
    public float minLeafLength;
    public float maxLeafLength;
    public float minBranchLength;
    public float maxBranchLength;
    [Header("Prefabs")] 
    public GameObject tree;
    public GameObject branch;
    public GameObject leaf;
    
    [Tooltip("Do not edit directly, select from dropdown")]
    public string selectedRule;
    
    [Tooltip("Be careful about changing this, good default is just 'X'")]
    public string axiom = "X";

    private Dictionary<char, string> rules = new Dictionary<char, string>();
    private Stack<SavedTransform> savedTransforms = new Stack<SavedTransform>();
    private Vector3 initialPosition;

    private string currentPath = "";
    private float[] randomRotations;

    private void Awake()
    {
        randomRotations = new float[1000];
        for (int i = 0; i < randomRotations.Length; i++)
        {
            randomRotations[i] = Random.Range(-1.0f, 1.0f);
        }

        rules.Add('X', selectedRule);
        rules.Add('F', "FF");
        transform.position = tree.GetComponent<LineRenderer>().GetPosition(1);

        Generate();
    }

    private void Generate()
    {
        currentPath = axiom;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < iterations; i++)
        {
            var currentPathChars = currentPath.ToCharArray();
            for (int j = 0; j < currentPathChars.Length; j++)
            {
                sb.Append(rules.ContainsKey(currentPath[j])
                    ? rules[currentPathChars[j]]
                    : currentPathChars[j].ToString());
            }
            currentPath = sb.ToString();
            sb = new StringBuilder();
        }

        for (int i = 0; i < currentPath.Length; i++)
        {
            switch (currentPath[i])
            {
                case 'F':
                    initialPosition = transform.position;
                    bool isLeaf = false;

                    GameObject currentElement;
                    if (currentPath[i + 1] % currentPath.Length == 'X' ||
                        currentPath[i + 3] % currentPath.Length == 'F' &&
                        currentPath[i + 4] % currentPath.Length == 'X')
                    {
                        currentElement = Instantiate(leaf, transform.position, transform.rotation);
                        isLeaf = true;
                    }
                    else
                    {
                        currentElement = Instantiate(branch, transform.position, transform.rotation);
                        Debug.Log("branch start width: " + currentElement.GetComponent<LineRenderer>().startWidth);
                        Debug.Log("branch end width: " + currentElement.GetComponent<LineRenderer>().endWidth);

                    }

                    currentElement.transform.SetParent(tree.transform);
                    TreeElement currentTreeElement = currentElement.GetComponent<TreeElement>();
                    float length;
                    if (isLeaf)
                    {
                        length = UnityEngine.Random.Range(minLeafLength, maxLeafLength);
                        transform.Translate(Vector3.up * length);
                        
                    }
                    else // Branch
                    {
                        length = UnityEngine.Random.Range(minBranchLength, maxBranchLength);
                        transform.Translate(Vector3.up * length);
                    }
                    Debug.Log("length: " + length);
                    currentTreeElement.lineRenderer.SetPosition(1, new Vector3(0, length, 0));
                    double[] widthDecreaser = new double[]{1,1};
                    if (thinnerOverLength)
                    {
                        widthDecreaser[0] = Math.Log(i);
                        widthDecreaser[1] = Math.Log(i + 1);
                    }
                    currentTreeElement.lineRenderer.startWidth = currentTreeElement.lineRenderer.startWidth * width / (float)widthDecreaser[0];
                    currentTreeElement.lineRenderer.endWidth = currentTreeElement.lineRenderer.endWidth * width / (float)widthDecreaser[1];
                    currentTreeElement.lineRenderer.sharedMaterial = currentTreeElement.material;

                    break;
                case 'X':
                    break;
                case '+':
                    transform.Rotate(Vector3.forward * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                case '-':
                    transform.Rotate(Vector3.back * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));

                    break;

                case '*':
                    transform.Rotate(Vector3.up * 120f * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                case '/':
                    transform.Rotate(Vector3.down * 120f * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                
                case '[':
                    savedTransforms.Push(new SavedTransform()
                    {
                        position = transform.position,
                        rotation = transform.rotation
                    });
                    break;
                case ']':
                    SavedTransform savedTransform = savedTransforms.Pop();

                    transform.position = savedTransform.position;
                    transform.rotation = savedTransform.rotation;
                    break;
            }
        }
    }
}

