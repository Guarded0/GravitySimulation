using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PlanetSettings
{
    public float mass;
    public Vector3 velocity;
    public float radius;
    public float temperature;
    public PlanetComposition planetComposition;
    public AtmosphereComposition atmosphereComposition;
    public SurfaceSettings surfaceSettings;

    public PlanetSettings(float mass)
    {
        this.mass = 10f;
        this.velocity = Vector3.zero;
        this.radius = 5f;
        this.temperature = 5f;
        this.planetComposition = new PlanetComposition(0);
        this.atmosphereComposition = new AtmosphereComposition(0);
        this.surfaceSettings = new SurfaceSettings(0);
    }
}

[System.Serializable]
public struct PlanetComposition
{
    public float iron;
    public float silicate;
    public float water;
    public PlanetComposition(float _)
    {
        this.iron = 0;
        this.silicate = 0;
        this.water = 0;
    }
}

[System.Serializable]
public struct AtmosphereComposition
{
    public float oxygen;
    public float hydrogen;
    public float carbonDioxide;
    public float methane;
    
    public AtmosphereComposition(float _)
    {
        this.oxygen = 0f;
        this.hydrogen = 0f;
        this.carbonDioxide = 0f;
        this.methane = 0f;
    }
}

[System.Serializable]
public struct SurfaceSettings
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
    public float roughness;
    // ocean
    public bool hasOcean;
    public float oceanRadius;

    public SurfaceSettings(float _)
    {
        this.seed = 0;
        this.complexity = 1.0f;
        this.mountainScale = 0.1f;
        this.mountainFrequency = 0.1f;
        this.mountainSharpness = 0.5f;
        this.planetScale = 2f;
        this.roughness = 2f;

        this.hasOcean = false;
        this.oceanRadius = 1f;
    }
}