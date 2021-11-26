using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LSystem))]
public class LSysEditor : Editor
{
    [SerializeField]
    [HideInInspector]
    private string[] _rules = new [] { "F+[[X]-X]-F[-FX]+X", "F-F++F-F", "F-X-F", "FF" };

    public override void OnInspectorGUI ()
    {
        //do this first to make sure you have the latest version
        serializedObject.Update();
        
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Update the selected choice in the underlying object
        if (target is LSystem lSys)   // holy shit c# is fucking amazing
        {
            lSys._choiceIndices[0] = EditorGUILayout.Popup(lSys._choiceIndices[0], _rules);
            lSys._choiceIndices[1] = EditorGUILayout.Popup(lSys._choiceIndices[1], _rules);
            lSys.selectedRule1 = _rules[lSys._choiceIndices[0]];
            lSys.selectedRule2 = _rules[lSys._choiceIndices[1]];
        }

        //for each property you want to draw ....
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_choiceIndices"));

        //do this last!  it will loop over the properties on your object and apply any it needs to, no if necessary!
        serializedObject.ApplyModifiedProperties();
        
    }
}