using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class PlanetSettings
{
    public float mass = 10f;
    public float radius = 5f;
    public float temperature;
    public PlanetComposition planetComposition;
    public AtmosphereComposition atmosphereComposition;
    public SurfaceSettings surfaceSettings;
}

[System.Serializable]
public class PlanetComposition
{
    public float iron;
    public float silicate;
    public float water;
}

[System.Serializable]
public class AtmosphereComposition
{
    public float oxygen;
    public float hydrogen;
    public float carbonDioxide;
    public float methane;
    
}

[System.Serializable]
public class SurfaceSettings
{
    public int seed;
    // surfance
    [Range(1.0f, 10.0f)]
    public float complexity;
    [Range(0.1f, 5f)]
    public float mountainScale;
    [Range(0.1f, 5f)]
    public float mountainFrequency;
    public float mountainSharpness;
    [Range(0.1f, 5f)]
    public float planetScale;
    public float Roughness;
    // ocean
    public bool hasOcean = false;
    public float oceanRadius = 1f;
}