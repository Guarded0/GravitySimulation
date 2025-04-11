using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


public class SauveragerSysteme : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/data.json";
    }

    void SauveragerSystemePlanetaire() {
        List<PlanetSettings> listeDonnesPlanetes = new List<PlanetSettings>{};
        DonneesPlanetes donneesPlanetes = new DonneesPlanetes();

        foreach (CelestialBody planete in NBodySimulation.celestialBodies) {
            listeDonnesPlanetes.Add(planete.planetSettings);
        }
        donneesPlanetes.listeDonneesPlanetes = listeDonnesPlanetes;

        string donneesSysteme = JsonUtility.ToJson(donneesPlanetes, true);
        System.IO.File.WriteAllText(filePath, donneesSysteme);
        Debug.Log(filePath);
    }

    
    public struct DonneesPlanetes {
        public List<PlanetSettings> listeDonneesPlanetes;
    } 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            SauveragerSystemePlanetaire();
        }
    }
}
