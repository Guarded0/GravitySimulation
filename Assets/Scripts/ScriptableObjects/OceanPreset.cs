using UnityEngine;

[CreateAssetMenu(fileName = "OceanPreset", menuName = "ScriptableObjects/OceanPreset")]
public class OceanPreset : ScriptableObject
{
    public float smoothness = 0.95f;
    public Texture2D waveNormalA;
    public Texture2D waveNormalB;
    public float waveStrength = 0.4f;
    public float waveNormalScale = 9.75f;
    public float waveSpeed = 0.1f;
}
