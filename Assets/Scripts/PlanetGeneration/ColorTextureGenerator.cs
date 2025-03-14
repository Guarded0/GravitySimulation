using UnityEngine;

[System.Serializable]
public class ColorTextureGenerator
{

    public Gradient colorGradient;
    public Texture2D texture;
    public int textureResolution = 150;

    public void UpdateTexture()
    {
        texture = new Texture2D(textureResolution, 1);
        Color[] colors = new Color[textureResolution];
        for (int i = 0; i < textureResolution; i++)
        {
            colors[i] = colorGradient.Evaluate(i / (textureResolution - 1f));
        }
        texture.SetPixels(colors);
        texture.Apply();
    }

}
