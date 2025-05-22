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
    //private Material material;
    private OceanRenderPass oceanRenderPass;
    public override void Create()
    {
        if (shader == null)
        {
            Debug.LogWarning("Shader is null");
            return;
        }
        //material = new Material(shader);
        PlanetGenerator[] planetGenerators = FindObjectsByType<PlanetGenerator>(FindObjectsSortMode.InstanceID);

        OceanSettings[] oceanSettings = new OceanSettings[planetGenerators.Length];
        Transform[] planetTransforms = new Transform[planetGenerators.Length];
        for (int i = 0; i < planetGenerators.Length; i++)
        {
            oceanSettings[i] = planetGenerators[i].oceanSettings;
            planetTransforms[i] = planetGenerators[i].transform;//
        }
            
        oceanRenderPass = new OceanRenderPass(shader, oceanSettings, planetTransforms);
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


    }
}

public class OceanRenderPass : ScriptableRenderPass
{
    private OceanSettings defaultOceanSettings;
    private List<OceanSettings> oceanSettingsList;
    private Shader shader;
    private List<Transform> planetTransforms;
    private RenderTextureDescriptor oceanTextureDescriptor;

    private static readonly int colorAID = Shader.PropertyToID("_colorA");
    private static readonly int colorBID = Shader.PropertyToID("_colorB");
    private static readonly int depthMultiplierID = Shader.PropertyToID("_depthMultiplier");
    private static readonly int alphaMultiplierID = Shader.PropertyToID("_alphaMultiplier");
    private static readonly int oceanRadiusID = Shader.PropertyToID("_oceanRadius");
    private static readonly int planetPositionID = Shader.PropertyToID("_planetPosition");
    private static readonly int smoothnessID = Shader.PropertyToID("_smoothness");
    private static readonly int directionToSunID = Shader.PropertyToID("_directionToSun");
    private static readonly int specularColorID = Shader.PropertyToID("_specularColor");
    private const string k_OceanTextureName = "_OceanTexture";
    private const string k_OceanPassName = "OceanPass";

    public OceanRenderPass(Shader shader, OceanSettings[] oceanSettings, Transform[] planetTransforms)
    {
        this.shader = shader;
        this.oceanSettingsList = new List<OceanSettings>(oceanSettings);
        this.planetTransforms = new List<Transform>(planetTransforms);
        oceanTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGBHalf, 0);
    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var volumeComponent = VolumeManager.instance.stack.GetComponent<OceanVolumeComponent>();
        if (volumeComponent == null || !volumeComponent.IsActive())
        {
            // If the volume component is not active, skip rendering
            return;
        }
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
            Material mat = new Material(shader);
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
        if (planets == null) return;
        for (int i = 0; i < planets.Count; i++)
        {
            PlanetGenerator gen;

            if (!planets[i].TryGetComponent<PlanetGenerator>(out gen)) continue;
            if (gen.oceanSettings.oceanRadius == 0) continue;
            transforms.Add(planets[i].transform);//
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

        Transform mainLightsource = PrimaryLightSource.FindMainLightSource(planetTransform.position);
        float sunIntensity = 0.0f;
        Vector3 directionToSun = Vector3.zero;
        if (mainLightsource != null)
        {
            directionToSun = (mainLightsource.position - planetTransform.position);
            sunIntensity = mainLightsource.GetComponent<Light>().intensity / 2;
        }

        material.SetColor(colorAID, colorA);
        material.SetColor(colorBID, colorB);
        material.SetFloat(depthMultiplierID, depthMultiplier);
        material.SetFloat(alphaMultiplierID, alphaMultiplier);
        material.SetFloat(oceanRadiusID, oceanRadius * planetTransform.localScale.x);
        material.SetFloat("_planetScale", planetTransform.localScale.x);
        material.SetVector(planetPositionID, planetTransform.position);
        material.SetFloat(smoothnessID, oceanSettings.oceanPreset.smoothness);
        material.SetVector(directionToSunID, directionToSun);
        material.SetFloat("_sunIntensity", sunIntensity);
        material.SetColor(specularColorID, volumeComponent.specularColor.value);
        material.SetTexture("_waveNormalA", oceanSettings.oceanPreset.waveNormalA);
        material.SetTexture("_waveNormalB", oceanSettings.oceanPreset.waveNormalB);
        material.SetFloat("_waveStrength", oceanSettings.oceanPreset.waveStrength);
        material.SetFloat("_waveNormalScale", oceanSettings.oceanPreset.waveNormalScale);
        material.SetFloat("_waveSpeed", oceanSettings.oceanPreset.waveSpeed);


        
    }

}

