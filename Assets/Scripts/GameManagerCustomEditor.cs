using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpheresHandler))]
public class GameManagerCustomEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpheresHandler spheresHandler = (SpheresHandler)target; //access the object being inspected
        if (GUILayout.Button("Start Test"))
        {
            spheresHandler.StopTutorialOnClick();
        }
    }
}
