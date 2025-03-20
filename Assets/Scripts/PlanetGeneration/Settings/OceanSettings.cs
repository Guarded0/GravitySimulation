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
    public float depthMultiplier = 10.0f;
    public float alphaMultiplier = 70.0f;
    public float oceanRadius = 5.0f;
    public float smoothness = 1.0f;


    public Texture2D waveNormalA;
    public Texture2D waveNormalB;
    public float waveStrength = 1f;
    public float waveNormalScale = 1f;
    public float waveSpeed = 1f;
}