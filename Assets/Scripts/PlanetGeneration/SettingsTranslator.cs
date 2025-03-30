using UnityEngine;

public class SettingsTranslator
{
    // takes variables from settings -> planet generator
    public static void SettingsToVariables(CelestialBody celestialBody, PlanetGenerator planetGenerator)
    {
        PlanetSettings planetSettings = celestialBody.planetSettings;
        Random.InitState(planetSettings.surfaceSettings.seed);
        planetGenerator.planetRadius = planetSettings.radius;
        SimpleNoiseSettings baseNoiseSettings = new SimpleNoiseSettings();
        baseNoiseSettings.numLayers = 4;
        baseNoiseSettings.lacunarity = 6f;
        baseNoiseSettings.persistence = 0.1f;
        baseNoiseSettings.noiseScale = Mathf.Clamp(planetSettings.surfaceSettings.planetScale, 0.1f, 5f);
        baseNoiseSettings.weight = 30f;
        baseNoiseSettings.offset = new Vector3(Random.value, Random.value, Random.value);


        RidgidNoiseSettings ridgidNoiseSettings = new RidgidNoiseSettings();
        ridgidNoiseSettings.numLayers = 4;
        ridgidNoiseSettings.lacunarity = 1.86f;
        ridgidNoiseSettings.persistence = 0.9f;
        ridgidNoiseSettings.noiseScale = Mathf.Clamp(planetSettings.surfaceSettings.mountainFrequency, 0.1f, 5f);
        ridgidNoiseSettings.weight = planetSettings.surfaceSettings.mountainScale * 10f;
        SimpleNoiseSettings ridgidMaskSettings = new SimpleNoiseSettings();
        ridgidMaskSettings.numLayers = 1;
        ridgidMaskSettings.lacunarity = 3.5f;
        ridgidMaskSettings.persistence = 10f;
        ridgidMaskSettings.noiseScale = planetSettings.surfaceSettings.planetScale;
        ridgidMaskSettings.weight = 1;
        ridgidMaskSettings.offset = new Vector3(Random.value, Random.value, Random.value);

        planetGenerator.atmosphereSettings.atmosphereRadius = planetSettings.radius * 1.75f;
        planetGenerator.baseNoiseSettings = baseNoiseSettings;
        planetGenerator.ridgidNoiseSettings = ridgidNoiseSettings;
        planetGenerator.ridgidMaskNoiseSettings = ridgidMaskSettings;
        planetGenerator.needsMeshUpdate = true;

        
        
    }

    // takes variables from planet generator -> settings
    static void VariablesToSettings(CelestialBody celestialBody, PlanetGenerator planetGenerator)
    {

    }
}
