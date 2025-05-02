using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Xml;
//source : https://www.youtube.com/watch?v=HnvPNoU9Wjw 
public class Parametres{
    public Resolution resolution = new Resolution(); 
    public Boolean pleinEcran = false;
    public Boolean fps = false;
    public float sensibiliter = 100; 
    }
public class resolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle togglePleinEcran;
    public Toggle toogleFPS;
    public TMP_Text textFPS;

    private Resolution[] resolutions;

    public static Parametres parametres = new Parametres();


    public float chronometre = 0;
    public int frameCount = 0;

    void Start()
    {

        // Obtenir toutes les résolutions disponibles
        resolutions = Screen.resolutions;

        // enlever les anciennes options
        resolutionDropdown.ClearOptions();

        // Créer une liste de chaînes de texte pour les options
        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "Hz";
            if (!options.Contains(option)) 
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

        togglePleinEcran.onValueChanged.AddListener(SetPleinEcran);
        toogleFPS.onValueChanged.AddListener(ShowFPS);
    }
    void Update()
    {
        if(parametres.fps == true ) {
        updateFPS();
        }
    }

    public void SetResolution(int index)
    {
        parametres.resolution = resolutions[index];
        Screen.SetResolution(parametres.resolution.width, parametres.resolution.height, true);
        Debug.Log("Résolution changée : " + parametres.resolution.width + "x" + parametres.resolution.height);
    }

    public void SetPleinEcran(Boolean boolPleinEcran){
        parametres.pleinEcran = boolPleinEcran; 
        Screen.fullScreen = parametres.pleinEcran; // Met en plein écran
        Debug.Log("Plein Écran: " + parametres.pleinEcran);

    }

    public void ShowFPS(Boolean boolFPS){
        parametres.fps = boolFPS;
        textFPS.gameObject.SetActive(parametres.fps);
        Debug.Log("FPS: " + parametres.fps);
    }
    public void updateFPS(){
        frameCount += 1;
        chronometre += Time.deltaTime;
            
        if(chronometre >= 1){ 
            textFPS.text = "FPS: " + Mathf.Round(frameCount/chronometre);
            chronometre = 0;
            frameCount = 0;
            }
        }
}

