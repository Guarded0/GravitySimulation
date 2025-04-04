using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class AtmosphereRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private AtmosphereSettings defaultSettings;
    [SerializeField] private Shader shader;
    //private Material material;
    private AtmosphereRenderPass atmosphereRenderPass;
    public override void Create()
    {
        if (shader == null)
        {
            Debug.LogWarning("Shader is null");
            return;
        }
        //material = new Material(shader);
        PlanetGenerator[] planetGenerators = FindObjectsByType<PlanetGenerator>(FindObjectsSortMode.None);

        AtmosphereSettings[] atmosphereSettings = new AtmosphereSettings[planetGenerators.Length];
        Transform[] planetTransforms = new Transform[planetGenerators.Length];
        for (int i = 0; i < planetGenerators.Length; i++)
        {
            atmosphereSettings[i] = planetGenerators[i].atmosphereSettings;
            planetTransforms[i] = planetGenerators[i].transform;//
        }

        atmosphereRenderPass = new AtmosphereRenderPass(shader, atmosphereSettings, planetTransforms);
        atmosphereRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        atmosphereRenderPass.ConfigureInput(ScriptableRenderPassInput.None);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (atmosphereRenderPass == null)
        {
            Debug.LogWarning("Render pass gone");
            return;
        }

        renderer.EnqueuePass(atmosphereRenderPass);

    }
    protected override void Dispose(bool disposing)
    {


    }
}

public class AtmosphereRenderPass : ScriptableRenderPass
{
    private AtmosphereSettings defaultAtmosphereSettings;
    private List<AtmosphereSettings> atmosphereSettingsList;
    private List<float> oceanRadiusList;
    private Shader shader;
    private List<Transform> planetTransforms;
    private RenderTextureDescriptor atmosphereTextureDescriptor;

    private static readonly int planetCenterID = Shader.PropertyToID("_planetCenter");
    private static readonly int planetRadiusID = Shader.PropertyToID("_planetRadius");
    private static readonly int atmosphereRadiusID = Shader.PropertyToID("_atmosphereRadius");
    private static readonly int inScatteringPointsID = Shader.PropertyToID("_inScatteringPoints");
    private static readonly int opticalDepthPointsID = Shader.PropertyToID("_opticalDepthPoints");
    private static readonly int densityFalloffID = Shader.PropertyToID("_densityFalloff");
    private static readonly int oceanRadiusID = Shader.PropertyToID("_oceanRadius");
    private static readonly int directionToSunID = Shader.PropertyToID("_directionToSun");
    private const string k_AtmosphereTextureName = "_AtmosphereTexture";
    private const string k_AtmospherePassName = "AtmospherePass";

    public AtmosphereRenderPass(Shader shader, AtmosphereSettings[] atmosphereSettings, Transform[] planetTransforms)
    {
        this.shader = shader;
        this.atmosphereSettingsList = new List<AtmosphereSettings>(atmosphereSettings);
        this.planetTransforms = new List<Transform>(planetTransforms);
        atmosphereTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGBHalf, 0);
    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // where color texture and depth texture is...
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (resourceData.isActiveTargetBackBuffer)
        {
            // Optionally, you can log a warning or handle this case differently
            Debug.LogWarning("AtmosphereRenderPass: Attempting to operate on the back buffer. Skipping.");
            return;
        }
        atmosphereTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
        atmosphereTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
        atmosphereTextureDescriptor.depthBufferBits = 0;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle intermediate1 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, atmosphereTextureDescriptor, k_AtmosphereTextureName, false);
        TextureHandle intermediate2 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, atmosphereTextureDescriptor, k_AtmosphereTextureName, false);

        if (!srcCamColor.IsValid())
            return;

        UpdatePlanets();

        for (int i = 0; i < planetTransforms.Count; i++)
        {
            Material mat = new Material(shader);
            UpdateAtmosphereSettings(mat, i);
            RenderGraphUtils.BlitMaterialParameters paraAtmosphere;
            if (i == 0)
            {
                paraAtmosphere = new(srcCamColor, intermediate1, mat, 0);
            }
            else if (i == planetTransforms.Count - 1 && i != 0)
            {
                if (i % 2 == 0)
                {
                    paraAtmosphere = new(intermediate2, srcCamColor, mat, 0);
                }
                else
                {
                    paraAtmosphere = new(intermediate1, srcCamColor, mat, 0);
                }
            }
            else
            {
                if (i % 2 == 0)
                {
                    paraAtmosphere = new(intermediate2, intermediate1, mat, 0);
                }
                else
                {
                    paraAtmosphere = new(intermediate1, intermediate2, mat, 0);
                }
            }
            renderGraph.AddBlitPass(paraAtmosphere, k_AtmospherePassName);
        }


        if (planetTransforms.Count == 1)
            resourceData.cameraColor = intermediate1;//
    }
    private void UpdatePlanets()
    {
        var planets = NBodySimulation.celestialBodies;

        List<Transform> transforms = new List<Transform>();
        List<AtmosphereSettings> newSettings = new List<AtmosphereSettings>();
        List<float> newOceanRadiusList = new List<float>();
        if (planets == null) return;
        for (int i = 0; i < planets.Count; i++)
        {
            PlanetGenerator gen;
            if (!planets[i]) continue;
            if (!planets[i].TryGetComponent<PlanetGenerator>(out gen)) continue;
            if (gen.atmosphereSettings.atmosphereRadius == 0) continue;
            transforms.Add(planets[i].transform);//
            newSettings.Add(planets[i].GetComponent<PlanetGenerator>().atmosphereSettings);
            newOceanRadiusList.Add(planets[i].GetComponent<PlanetGenerator>().oceanSettings.oceanRadius);
            // TODO: GET PLANETS THAT ARE CURRENTLY ON THIS BITCH

        }
        planetTransforms = transforms;
        atmosphereSettingsList = newSettings;
        oceanRadiusList = newOceanRadiusList;
    }
    private void UpdateAtmosphereSettings(Material material, int index)
    {
        if (material == null) return;

       AtmosphereSettings atmosphereSettings = atmosphereSettingsList[index];
       float oceanRadius = oceanRadiusList[index];
       Transform planetTransform = planetTransforms[index];

       var volumeComponent = VolumeManager.instance.stack.GetComponent<AtmosphereVolumeComponent>();

        material.SetVector(planetCenterID, planetTransform.position);
        material.SetFloat(planetRadiusID, atmosphereSettings.planetRadius);
        material.SetFloat(atmosphereRadiusID, atmosphereSettings.atmosphereRadius);
        material.SetInt(inScatteringPointsID, atmosphereSettings.inScatteringPoints);
        material.SetInt(opticalDepthPointsID, atmosphereSettings.opticalDepthPoints);
        material.SetFloat(densityFalloffID, atmosphereSettings.densityFalloff);
        material.SetFloat(oceanRadiusID, oceanRadius);
        material.SetVector(directionToSunID, (NBodySimulation.Instance.relativeBody.transform.position - planetTransform.position).normalized);
        material.SetVector("_scatteringCoefficients", atmosphereSettings.GetScatteringCoefficient());
        material.SetFloat("_intensity", atmosphereSettings.intensity);
        //
    }

}

