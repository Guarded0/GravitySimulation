using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Xml;
using Unity.VisualScripting;
//source : https://www.youtube.com/watch?v=HnvPNoU9Wjw 
public class Parametres{
    public Resolution resolution = new Resolution(); // Résolution actuelle de l'écran
    public bool pleinEcran = false; // Mode plein écran activé ou désactivé
    public bool fps = false; // Affichage des FPS activé ou désactivé
    public float sensibiliter = 100; //sensibilité de la camera
    }

// résolution gère la résolution de l'écran, l'option plein écran, le FPS et la sensibilité de la camera
public class resolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown; // choisir la résolution de l'écran
    public Toggle togglePleinEcran; // activer et désactiver le plein écran
    public Toggle toogleFPS; // afficher et masquer les FPS
    public TMP_Text textFPS; // Texte pour afficher les FPS
    public Slider sliderSensibiliter; // Slider pour ajuster la sensibilité de la camera
    public TMP_InputField inputFieldSensibiliter; // boite pour écrire et afficher la sensibilité de la camera

    // menu déroulant des résolutions
    private Resolution[] resolutions;

    public static Parametres parametres = new Parametres();

    // Variables utilisées pour calculer les FPS
    private float chronometre = 0;
    private int frameCount = 0;

    void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        // Remplir la liste de résolutions 
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "Hz";
            if (!options.Contains(option)) 
                options.Add(option);

            // Déterminer la résolution actuelle
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        // Appliquer les options au Dropdown et définir la valeur
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex); // éviter d’appeler l’événement
        resolutionDropdown.RefreshShownValue();

        // Lier les événements UI aux méthodes correspondantes
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        togglePleinEcran.onValueChanged.AddListener(SetPleinEcran);
        toogleFPS.onValueChanged.AddListener(ShowFPS);
        sliderSensibiliter.onValueChanged.AddListener(setSensibiliteAvecSlider);
        inputFieldSensibiliter.onValueChanged.AddListener(setSensibiliteAvecInputField);
    }

    void Update()
    {
        // Mettre à jour l’affichage des FPS si activé
        if (parametres.fps)
        {
            updateFPS();
        }
    }


    // Met à jour la résolution de l'écran selon l'option sélectionnée
    public void SetResolution(int index)
    {
        parametres.resolution = resolutions[index];
        Screen.SetResolution(parametres.resolution.width, parametres.resolution.height, true);
    }

    // Active ou désactive le plein écran
    public void SetPleinEcran(bool boolPleinEcran)
    {
        parametres.pleinEcran = boolPleinEcran;
        Screen.fullScreen = parametres.pleinEcran;
    }

    // Définit la sensibilité de la camera à partir d'un champ texte
    public void setSensibiliteAvecInputField(string StringValeur)
    {
        float valeur = float.Parse(StringValeur);
        valeur = Math.Max(valeur, 0);
        valeur = Math.Min(valeur, 200);
        parametres.sensibiliter = valeur;

        // Synchroniser le slider
        sliderSensibiliter.value = valeur;
    }

    // Définit la sensibilité de la camera à partir du slider
    public void setSensibiliteAvecSlider(float valeur)
    {
        parametres.sensibiliter = valeur;

        // Mettre à jour le texte
        inputFieldSensibiliter.text = valeur.ToString("F1");
    }

    // Active ou désactive l'affichage des FPS
    public void ShowFPS(bool boolFPS)
    {
        parametres.fps = boolFPS;
        textFPS.gameObject.SetActive(parametres.fps);
        Debug.Log("FPS: " + parametres.fps);
    }

    // Calcule et affiche les FPS
    public void updateFPS()
    {
        frameCount += 1;
        chronometre += Time.deltaTime;

        if (chronometre >= 1)
        {
            textFPS.text = "FPS: " + Mathf.Round(frameCount / chronometre);
            chronometre = 0;
            frameCount = 0;
        }
    }
}


