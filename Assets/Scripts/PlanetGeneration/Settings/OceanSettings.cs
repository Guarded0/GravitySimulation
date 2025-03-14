using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class OceanSettings 
{

    public Color colorA;
    public Color colorB;
    public float depthMultiplier = 10.0f;
    public float alphaMultiplier = 70.0f;
    public float oceanRadius = 5.0f;

}