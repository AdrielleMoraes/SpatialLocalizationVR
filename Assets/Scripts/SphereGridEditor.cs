using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SphereGrid))]
public class SphereGridEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SphereGrid sphereGrid = (SphereGrid)target;
        if (GUILayout.Button("Clear objects"))
        {
            sphereGrid.ClearPoints();
        }
        if (GUILayout.Button("Build object"))
        {
            sphereGrid.GeneratePoints();
        }
    }
}
