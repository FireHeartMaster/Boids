using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public Boid[] allBoids;
    public ComputeShader computeShader;
    public BoidSettings settings;

    const int threadGroupSize = 1024;

    void Update()
    {
        if(allBoids != null)
        {
            int numberOfBoids = allBoids.Length;
            BoidData[] boidData = new BoidData[numberOfBoids];

            for (int i = 0; i < numberOfBoids; i++)
            {
                boidData[i].position = allBoids[i].transform.position;
                boidData[i].direction = allBoids[i].transform.forward;
            }

            ComputeBuffer boidBuffer = new ComputeBuffer(numberOfBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            computeShader.SetBuffer(0, "boids", boidBuffer);
            computeShader.SetInt("totalNumberOfBoids", allBoids.Length);
            computeShader.SetFloat("visionRadius", settings.visionRadius);
            computeShader.SetFloat("avoidRadius", settings.visionRadius * settings.avoidRadiusPercent);

            int threadGroups = Mathf.CeilToInt(numberOfBoids / (float)threadGroupSize);
            computeShader.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < numberOfBoids; i++)
            {
                allBoids[i].avoidOthersDirection = boidData[i].avoidOthersDirection;
                allBoids[i].alignmentDirection = boidData[i].alignmentDirection;
                allBoids[i].followCenterDirection = boidData[i].groupCenter / boidData[i].numberNearBoids;
                allBoids[i].numberOfNearBoids = boidData[i].numberNearBoids;
            }

            boidBuffer.Release();
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 avoidOthersDirection;
        public Vector3 alignmentDirection;
        public Vector3 groupCenter;
        public int numberNearBoids;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    };
}
