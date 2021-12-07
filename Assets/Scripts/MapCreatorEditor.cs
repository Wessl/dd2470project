using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapCreator))]
public class MapCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapCreator myTarget = (MapCreator)target;
        if(GUILayout.Button("GENERATE MAPS"))
        {
            myTarget.Generate();
        }
    }
}
