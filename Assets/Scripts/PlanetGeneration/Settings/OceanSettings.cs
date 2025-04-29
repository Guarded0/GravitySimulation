using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;

[Serializable]
public class OceanSettings 
{

    public Color colorA;
    public Color colorB;
    public float depthMultiplier = 75.0f;
    public float alphaMultiplier = 298.0f;
    [Range(0f, 2f)]
    public float oceanRadius = 1.0f;
    public float smoothness = 1.0f;


    public Texture2D waveNormalA;
    public Texture2D waveNormalB;
    public float waveStrength = 0.4f;
    public float waveNormalScale = 9.75f;
    public float waveSpeed = 0.1f;
}