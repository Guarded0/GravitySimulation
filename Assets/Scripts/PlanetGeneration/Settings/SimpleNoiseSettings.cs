using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class SimpleNoiseSettings
{
    public int numLayers = 4;
    public float lacunarity = 2;
    public float persistence = 0.5f;
    public float noiseScale = 1f;
    public float elevation = 0f;
    public float weight = 1f;
    public Vector3 offset;

    public float[] GetValues()
    {
        float[] noiseParams =
        {
            // [0]
            offset.x,
            offset.y,
            offset.z,
            numLayers,
            // [1]
            persistence,
            lacunarity,
            noiseScale,
            elevation,
            // [2]
            weight
        };
        return noiseParams;
    }
}

[System.Serializable]
public class RidgidNoiseSettings
{  
    public int numLayers = 4;
    public float lacunarity = 2;
    public float persistence = 0.5f;
    public float noiseScale = 1;
    public float elevation = 0f;
    public float weight = 1;
    public float power = 1;
    public float gain = 1;

    public Vector3 offset;
    public float[] GetValues()
    {
        float[] noiseParams =
        {
            // [0]
            offset.x,
            offset.y,
            offset.z,
            numLayers,
            // [1]
            persistence,
            lacunarity,
            noiseScale,
            elevation,
            // [2]
            weight,
            power,
            gain
        };
        return noiseParams;
    }
}
