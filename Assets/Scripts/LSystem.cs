using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystem : MonoBehaviour
{
    public int iterations;
    public float angle;
    public float width;
    public float minLeafLength;
    public float maxLeafLength;
    public float minBranchLength;
    public float maxBranchLength;
    public float variance;

    public GameObject tree;
    public GameObject branch;
    public GameObject leaf;

    private const string axiom = "X";

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
            randomRotations[i] = Random.Range(-1f, 1f);
        }

        rules.Add('X', "[-FX][+FX][FX]");
        rules.Add('F', "FF");

        Generate();
    }

    private void Generate()
    {
        currentPath = axiom;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < iterations; i++)
        {
            foreach (char ch in currentPath.ToCharArray())
            {
                sb.Append(rules.ContainsKey(ch)
                    ? rules[ch] : ch.ToString());
            }
            currentPath = sb.ToString();
            sb.Clear();
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

                    currentTreeElement.lineRenderer.startWidth = currentTreeElement.lineRenderer.startWidth * width;
                    currentTreeElement.lineRenderer.endWidth = currentTreeElement.lineRenderer.endWidth * width;
                    currentTreeElement.lineRenderer.sharedMaterial = currentTreeElement.material;
                    currentTreeElement.lineRenderer.SetPosition(1, new Vector3(0, length, 0));
                    break;
                case 'X':
                    break;
                case'+':
                    transform.Rotate(Vector3.forward * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;
                case'-':
                    transform.Rotate(Vector3.back * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;
                case'*':
                    transform.Rotate(Vector3.up * 120f * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;
                case'/':
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
