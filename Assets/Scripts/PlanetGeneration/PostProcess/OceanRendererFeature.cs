using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class OceanRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private OceanSettings defaultSettings;
    [SerializeField] private Shader shader;
    private Material material;
    private OceanRenderPass oceanRenderPass;
    public override void Create()
    {
        if (shader == null)
        {
            Debug.LogWarning("Shader is null");
            return;
        }
        
        material = new Material(shader);
        PlanetGenerator[] planetGenerators = FindObjectsByType<PlanetGenerator>(FindObjectsSortMode.InstanceID);

        OceanSettings[] oceanSettings = new OceanSettings[planetGenerators.Length];
        Transform[] planetTransforms = new Transform[planetGenerators.Length];
        for (int i = 0; i < planetGenerators.Length; i++)
        {
            oceanSettings[i] = planetGenerators[i].oceanSettings;
            planetTransforms[i] = planetGenerators[i].transform;
        }
            
        oceanRenderPass = new OceanRenderPass(material, oceanSettings, planetTransforms);
        oceanRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        oceanRenderPass.ConfigureInput(ScriptableRenderPassInput.None);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (oceanRenderPass == null)
        {
            Debug.LogWarning("Render pass gone");
            return;
        }

        renderer.EnqueuePass(oceanRenderPass);

    }
    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }

    }
}


public class OceanRenderPass : ScriptableRenderPass
{
    private OceanSettings defaultOceanSettings;
    private List<OceanSettings> oceanSettingsList;
    private Material material;
    private List<Transform> planetTransforms;
    private RenderTextureDescriptor oceanTextureDescriptor;

    private static readonly int colorAID = Shader.PropertyToID("_colorA");
    private static readonly int colorBID = Shader.PropertyToID("_colorB");
    private static readonly int depthMultiplierID = Shader.PropertyToID("_depthMultiplier");
    private static readonly int alphaMultiplierID = Shader.PropertyToID("_alphaMultiplier");
    private static readonly int oceanRadiusID = Shader.PropertyToID("_oceanRadius");
    private static readonly int planetPositionID = Shader.PropertyToID("_planetPosition");
    private const string k_OceanTextureName = "_OceanTexture";
    private const string k_OceanPassName = "OceanPass";

    public OceanRenderPass(Material material, OceanSettings[] oceanSettings, Transform[] planetTransforms)
    {
        this.material = material;
        this.oceanSettingsList = new List<OceanSettings>(oceanSettings);
        this.planetTransforms = new List<Transform>(planetTransforms);
        oceanTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // where color texture and depth texture is...
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (resourceData.isActiveTargetBackBuffer)
        {
            // Optionally, you can log a warning or handle this case differently
            Debug.LogWarning("OceanRenderPass: Attempting to operate on the back buffer. Skipping.");
            return;
        }
        oceanTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
        oceanTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
        oceanTextureDescriptor.depthBufferBits = 0;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle intermediate1 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, oceanTextureDescriptor, k_OceanTextureName, true);
        TextureHandle intermediate2 = UniversalRenderer.CreateRenderGraphTexture(renderGraph, oceanTextureDescriptor, k_OceanTextureName, false);

        if (!srcCamColor.IsValid())
            return;

        UpdatePlanets();
        for (int i = 0; i < planetTransforms.Count; i++)
        { 
            
            Material mat = new Material(material);
            UpdateOceanSettings(mat, oceanSettingsList[i], planetTransforms[i]);
            RenderGraphUtils.BlitMaterialParameters paraOcean;
            if (i == 0)
            {
                paraOcean = new(srcCamColor, intermediate1, mat, 0);
            }
            else if(i == planetTransforms.Count-1 && i != 0) 
            {
                if (i % 2 == 0)
                {
                    paraOcean = new(intermediate2, srcCamColor, mat, 0);
                }
                else
                {
                    paraOcean = new(intermediate1, srcCamColor, mat, 0);
                }
            }
            else
            {
                if (i % 2 == 0)
                {
                    paraOcean = new(intermediate2, intermediate1, mat, 0);
                }
                else
                {
                    paraOcean = new(intermediate1, intermediate2, mat, 0);
                }
            }
            renderGraph.AddBlitPass(paraOcean, k_OceanPassName);
        }
        
        
        if(planetTransforms.Count == 1)
            resourceData.cameraColor = intermediate1;
    }
    private void UpdatePlanets()
    {
        var planets = NBodySimulation.celestialBodies;
        List<Transform> transforms = new List<Transform>();
        List<OceanSettings> newSettings = new List<OceanSettings>();
        for (int i = 0; i < planets.Count; i++)
        {
            transforms.Add(planets[i].transform);
            newSettings.Add(planets[i].GetComponent<PlanetGenerator>().oceanSettings);
            // TODO: GET PLANETS THAT ARE CURRENTLY ON THIS BITCH
            
        }
        planetTransforms = transforms;
        oceanSettingsList = newSettings;
    }
    private void UpdateOceanSettings(Material material, OceanSettings oceanSettings, Transform planetTransform)
    {
        if (material == null) return;

        var volumeComponent = VolumeManager.instance.stack.GetComponent<OceanVolumeComponent>();
        Color colorA = oceanSettings.colorA;
        Color colorB = oceanSettings.colorB;
        float depthMultiplier = oceanSettings.depthMultiplier;
        float alphaMultiplier = oceanSettings.alphaMultiplier;
        float oceanRadius = oceanSettings.oceanRadius;

        material.SetColor(colorAID, colorA);
        material.SetColor(colorBID, colorB);
        material.SetFloat(depthMultiplierID, depthMultiplier);
        material.SetFloat(alphaMultiplierID, alphaMultiplier);
        material.SetFloat(oceanRadiusID, oceanRadius);
        material.SetVector(planetPositionID, planetTransform.position);
    }

}

