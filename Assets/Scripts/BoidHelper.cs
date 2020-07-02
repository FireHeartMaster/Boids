using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoidHelper
{
    public static Vector3[] rayDirections;
    static int numPoints = 100;
    static float turnFraction = 1.61f;

    public static void Initialize(int numberOfPoints, float fraction)
    {
        numPoints = numberOfPoints;
        turnFraction = fraction;
        rayDirections = new Vector3[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (numPoints - 1f);
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = 2 * Mathf.PI * turnFraction * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            rayDirections[i] = (new Vector3(x, y, z));
        }
    }
}
