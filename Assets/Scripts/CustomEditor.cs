using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LSystem))]
public class CustomeEditor : Editor
{
    string[] _rules = new [] { "F+[[X]-X]-F[-FX]+X", "F-F++F-F" };
    int _choiceIndex = 0;
 
    public override void OnInspectorGUI ()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        _choiceIndex = EditorGUILayout.Popup(_choiceIndex, _rules);
        // Update the selected choice in the underlying object
        if (target is LSystem lSys)   // holy shit c# is fucking amazing
        {
            lSys.selectedRule = _rules[_choiceIndex];
        }
        // Save the changes back to the object
        EditorUtility.SetDirty(target);
    }
}