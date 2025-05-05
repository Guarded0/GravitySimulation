using System;
using UnityEngine;

[Serializable]
public class OceanSettings 
{
    public Color colorA;
    public Color colorB;
    public float depthMultiplier = 75.0f;
    public float alphaMultiplier = 298.0f;
    [Range(0f, 2f)]
    public float oceanRadius = 1.0f;


    public OceanPreset oceanPreset;
    public OceanSettings()
    {
        this.colorA = Color.blue;
        this.colorB = Color.cyan;
        this.depthMultiplier = 75.0f;
        this.alphaMultiplier = 298.0f;
        this.oceanRadius = 1.0f;
        this.oceanPreset = null; // Set to null or assign a default preset if needed
    }
    public OceanSettings(OceanSettings other)
    {
        this.colorA = other.colorA;
        this.colorB = other.colorB;
        this.depthMultiplier = other.depthMultiplier;
        this.alphaMultiplier = other.alphaMultiplier;
        this.oceanRadius = other.oceanRadius;
        this.oceanPreset = other.oceanPreset;
    }
}