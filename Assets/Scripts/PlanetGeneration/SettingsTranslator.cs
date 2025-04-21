using UnityEngine;

public class SettingsTranslator
{
    // takes variables from settings -> planet generator
    public static void SettingsToVariables(CelestialBody celestialBody, PlanetGenerator planetGenerator)
    {
        planetGenerator.bodyType = celestialBody.planetSettings.bodyType;
        planetGenerator.planetRadius = celestialBody.planetSettings.radius;
        planetGenerator.temperature = celestialBody.planetSettings.temperature;
        if (celestialBody.planetSettings.bodyType == BodyType.Star)
        {
            
            planetGenerator.oceanSettings.oceanRadius = 0;
            planetGenerator.atmosphereSettings.atmosphereRadius = 0;
        }

        if (celestialBody.planetSettings.planetShapeSettings != null)
        {
            planetGenerator.baseNoiseSettings = celestialBody.planetSettings.planetShapeSettings.baseNoiseSettings;
            planetGenerator.ridgidNoiseSettings = celestialBody.planetSettings.planetShapeSettings.ridgidNoiseSettings;
            planetGenerator.ridgidMaskNoiseSettings = celestialBody.planetSettings.planetShapeSettings.ridgidMaskNoiseSettings;
        }//

        planetGenerator.needsMeshUpdate = true;
    }

    // takes variables from planet generator -> settings
    static void VariablesToSettings(CelestialBody celestialBody, PlanetGenerator planetGenerator)
    {

    }
}
