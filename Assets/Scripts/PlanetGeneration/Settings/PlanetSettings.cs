using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PlanetSettings
{
    public string name;
    public BodyType bodyType;
    public float mass;
    public Vector3 velocity;
    public float radius;
    public int temperature;

    public SurfaceColor surfaceColor;
    public AtmosphereComposition atmosphereComposition;
    public OceanSettings oceanSettings;
    public PlanetShapePreset planetShapeSettings;

    public PlanetSettings(float _)
    {
        this.name = "Planet";
        this.bodyType = BodyType.Planet;
        this.mass = 10f;
        this.velocity = Vector3.zero;
        this.radius = 5f;
        this.temperature = 5;
        this.surfaceColor = new SurfaceColor();
        this.atmosphereComposition = new AtmosphereComposition(0);
        this.oceanSettings = new OceanSettings();
        this.planetShapeSettings = PlanetShapePreset.CreateInstance<PlanetShapePreset>();
    }
}

[System.Serializable]
public struct SurfaceColor
{
    public Color low, mid, high;

    public SurfaceColor(float _)
    {
        this.low = Color.black;
        this.mid = Color.grey;
        this.high = Color.white;
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