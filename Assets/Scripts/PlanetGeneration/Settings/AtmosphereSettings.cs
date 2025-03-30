using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;

[Serializable]
public class AtmosphereSettings
{
    public float planetRadius = 1.0f;
    public float atmosphereRadius = 1.0f;
    public int inScatteringPoints = 9;
    public int opticalDepthPoints = 20;
    public float densityFalloff = 12f;
    public float intensity = 0.5f;

    public float scatteringStrength = 23f;
    public Vector3 wavelengths = new Vector3(700, 530, 440);

    public Vector3 GetScatteringCoefficient()
    {
        float redScatter = Mathf.Pow(400 / wavelengths.x, 4) * scatteringStrength;
        float greenScatter = Mathf.Pow(400 / wavelengths.y, 4) * scatteringStrength;
        float blueScatter = Mathf.Pow(400 / wavelengths.z, 4) * scatteringStrength;
        // scattering coefficients
        return new Vector3(redScatter, greenScatter, blueScatter);
    }
}
    