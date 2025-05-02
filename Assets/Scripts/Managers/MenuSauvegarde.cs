using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class MenuSauvegardes : MonoBehaviour
{
    public GameObject boutonPrefab; // à assigner dans l’inspecteur
    public Transform conteneurBoutons; // à assigner dans l’inspecteur
    public GameObject panelMenu; // à assigner dans l’inspecteur

    void Start()
    {
        panelMenu.SetActive(false); // Le menu est caché au début
    }

    public void AfficherMenu()
    {
        // Nettoyer l'ancien contenu
        foreach (Transform enfant in conteneurBoutons) {
            Destroy(enfant.gameObject);
        }

        string[] fichiers = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string fichier in fichiers)
        {
            GameObject bouton = Instantiate(boutonPrefab, conteneurBoutons);
            string nomFichier = Path.GetFileNameWithoutExtension(fichier);
            bouton.GetComponentInChildren<TextMeshProUGUI>().text = nomFichier;
            bouton.GetComponent<Button>().onClick.AddListener(() => {
                FindFirstObjectByType<SauveragerSysteme>().ChargerSystemePlanetaireDepuisFichier(nomFichier);
                panelMenu.SetActive(false);
            });
        }

        panelMenu.SetActive(true);
    }

    public void CacherMenu()
    {
        panelMenu.SetActive(false);
    }
}
