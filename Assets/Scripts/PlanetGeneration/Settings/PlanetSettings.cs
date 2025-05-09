using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class PlanetSettings
{
    public string name;
    public BodyType bodyType;
    public float mass;
    public Vector3 velocity;
    public bool isAnchored;
    public bool hasGravity;
    public float radius;
    public int temperature;

    public SurfaceColor surfaceColor = new SurfaceColor();
    public AtmosphereComposition atmosphereComposition = new AtmosphereComposition();
    public OceanSettings oceanSettings = new OceanSettings();
    public PlanetShapePreset planetShapeSettings;

    public PlanetSettings()
    {
        this.name = "Planet";
        this.bodyType = BodyType.Planet;
        this.mass = 10f;
        this.velocity = Vector3.zero;
        this.radius = 5f;
        this.temperature = 5;
        this.isAnchored = false;
        this.hasGravity = true;
    }
    public PlanetSettings(PlanetSettings other)
    {
        this.name = other.name;
        this.bodyType = other.bodyType;
        this.mass = other.mass;
        this.velocity = other.velocity;
        this.radius = other.radius;
        this.temperature = other.temperature;
        this.isAnchored = other.isAnchored;
        this.hasGravity = other.hasGravity;
        this.surfaceColor = new SurfaceColor(other.surfaceColor);
        this.oceanSettings = new OceanSettings(other.oceanSettings);
        this.planetShapeSettings = other.planetShapeSettings;
        this.atmosphereComposition = new AtmosphereComposition(atmosphereComposition);
            
    }
}

[System.Serializable]
public class SurfaceColor
{
    public Color low, mid, high;

    public SurfaceColor()
    {
        this.low = Color.black;
        this.mid = Color.grey;
        this.high = Color.white;
    }
    public SurfaceColor(SurfaceColor other)
    {
        this.low = other.low;
        this.mid = other.mid;
        this.high = other.high;
    }
}
[System.Serializable]
public class AtmosphereComposition
{
    public float oxygen;
    public float hydrogen;
    public float carbonDioxide;
    public float methane;
    
    public AtmosphereComposition()
    {
        this.oxygen = 0f;
        this.hydrogen = 0f;
        this.carbonDioxide = 0f;
        this.methane = 0f;
    }
    public AtmosphereComposition(AtmosphereComposition other)
    {
        this.oxygen = other.oxygen;
        this.hydrogen = other.hydrogen;
        this.carbonDioxide = other.carbonDioxide;
        this.methane = other.methane;
    }
}