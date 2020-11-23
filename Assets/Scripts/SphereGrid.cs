using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class SphereGrid : MonoBehaviour {

    //Game object to represent sound stimuli
    public GameObject prefab;
    public Material mainMaterial;
    public Material frontMaterial;

    //Spatial representation settings
    [Range(1,24)]
    public int nRings = 1;

    [Range(1, 24)]
    public int nStepsHorizontal = 24;

    //sphere radius
    public float radius = 100;

    //set range of view for the horizontal plane and elevation
    [Range(0,360)]
    public int maxAzimuth = 360;
    [Range(0, 180)]
    public int minAzimuth = 0;

    [Range(0, 180)]
    public int maxElevation = 180;

    [Range(0, 90)]
    public int minElevation = 0;


    /// <summary>
    /// Clear existing points
    /// </summary>
    public void ClearPoints()
    {    
        foreach (var item in GameObject.FindGameObjectsWithTag("SoundSource"))
        {
            DestroyImmediate(item);
        }
    }

    /// <summary>
    /// Generate all points of the sphere
    /// </summary>
    public void GeneratePoints()
    {
        ClearPoints();
        float phi = 0;
        for (int i = 1; i <= nRings; i++)
        {
            phi = (maxElevation - minElevation) / nRings * i;
            for (int j = 0; j <= nStepsHorizontal-1; j++)
            {
                //for 1 ring create only spheres on the horizontal plane
                if (nRings == 1)
                {
                    phi = 90;
                }
                float theta = ((maxAzimuth - minAzimuth) / nStepsHorizontal * j)+ gameObject.transform.localRotation.y;
                Vector3 pos = CalculatePoints(radius, theta, phi) + transform.localPosition;
                GameObject  obj = Instantiate(prefab, pos, Quaternion.identity);
                obj.name = "Sound Source "+ j;
                obj.transform.parent = gameObject.transform;
                //assign materials
                if (theta > 1)
                    obj.GetComponent<Renderer>().material = frontMaterial;
                else
                    obj.GetComponent<Renderer>().material = mainMaterial;
                if (phi == 0 || phi == 180)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Calculate any point in a sphere given a spherical coordinate
    /// </summary>
    /// <param name="r">Sphere's radius</param>
    /// <param name="theta">Azimuth angle</param>
    /// <param name="phi">Elevation angle</param>
    Vector3 CalculatePoints(float r, float theta, float phi)
    {
        theta = theta * Mathf.PI / 180;
        phi = phi * Mathf.PI / 180;
        Vector3 ans = new Vector3(r * Mathf.Cos(theta) * Mathf.Sin(phi), r * Mathf.Cos(phi), r * Mathf.Sin(theta) * Mathf.Sin(phi));
        return ans;
    }
}
