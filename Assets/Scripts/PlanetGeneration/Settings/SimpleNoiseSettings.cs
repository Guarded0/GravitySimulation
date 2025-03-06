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
    public float scale = 1;
    public float elevation = 1;
    public Vector3 offset;
}

[System.Serializable]
public class RidgesNoiseSettings
{
    public Vector3 offset;
    public int numLayers = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2;
    public float noiseScale = 1;
    public float weight = 1;
    public float power = 1;
    public float gain = 1;
    public float elevation = 4;
}
