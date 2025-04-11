using UnityEngine;
using TMPro;
//source : https://www.youtube.com/watch?v=HnvPNoU9Wjw 
public class resolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    void Start()
    {
        // Obtenir toutes les résolutions disponibles
        resolutions = Screen.resolutions;

        // Nettoyer les anciennes options
        resolutionDropdown.ClearOptions();

        // Créer une liste de chaînes de texte pour les options
        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            if (!options.Contains(option)) // éviter les doublons
                options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        // Ajouter les options au Dropdown
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Ajouter l'écouteur d'événement
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index)
    {
        Resolution selected = resolutions[index];
        Screen.SetResolution(selected.width, selected.height, true);
        Debug.Log("Résolution changée : " + selected.width + "x" + selected.height);
    }
}
