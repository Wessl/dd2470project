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

[CustomEditor(typeof(PlantController))]
public class PlantControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlantController myTarget = (PlantController)target;
        if(GUILayout.Button("PLACE PLANTS"))
        {
            myTarget.PlacePlants();
        }
    }
}
