using UnityEngine;

[CreateAssetMenu(fileName = "PlanetShapePreset", menuName = "ScriptableObjects/PlanetShapePreset", order = 1)]
public class PlanetShapePreset : ScriptableObject
{
    public SimpleNoiseSettings baseNoiseSettings;
    public RidgidNoiseSettings ridgidNoiseSettings;
    public SimpleNoiseSettings ridgidMaskNoiseSettings;
}
