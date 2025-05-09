using UnityEngine;
using System.Collections.Generic;


public class SauveragerSysteme : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/data.json";
    }

    void SauveragerSystemePlanetaire(string nomFichier) {
        DoneesScene doneesScene = new DoneesScene();

        foreach (CelestialBody planete in NBodySimulation.celestialBodies) {
            doneesScene.listeDonneesPlanetes.Add(planete.planetSettings);
            doneesScene.listeCoordonnees.Add(planete.transform.position);
        }

        doneesScene.gravConstant = NBodySimulation.Instance.gravConstant;
        doneesScene.planetGravity = NBodySimulation.Instance.planetGravity;

        if (NBodySimulation.Instance.referenceBody != null) {
        doneesScene.relativeBody = NBodySimulation.Instance.referenceBody.name;
        }

        string donneesSysteme = JsonUtility.ToJson(doneesScene, true);
        string cheminComplet = Application.persistentDataPath + "/" + nomFichier + ".json";
        System.IO.File.WriteAllText(cheminComplet, donneesSysteme);
 
    }

    void ChargerSystemePlanetaire() {
        foreach(CelestialBody planete in NBodySimulation.celestialBodies) {
            Destroy(planete.gameObject);
        }

        string donneesSysteme = System.IO.File.ReadAllText(filePath);
        DoneesScene donneesCharger = JsonUtility.FromJson<DoneesScene>(donneesSysteme);

        for (int i = 0; i < donneesCharger.listeDonneesPlanetes.Count; i++) {
            GameObject newPlanet = NBodySimulation.Instance.CreatePlanet(donneesCharger.listeCoordonnees[i], donneesCharger.listeDonneesPlanetes[i]);
            
            if (donneesCharger.listeDonneesPlanetes[i].name == donneesCharger.relativeBody) {
                NBodySimulation.Instance.referenceBody = newPlanet.GetComponent<CelestialBody>();
            }
        }

        NBodySimulation.Instance.gravConstant = donneesCharger.gravConstant;
        NBodySimulation.Instance.planetGravity = donneesCharger.planetGravity;
    }

    public void ChargerSystemePlanetaireDepuisFichier(string nomFichier) {
        string chemin = Application.persistentDataPath + "/" + nomFichier + ".json";

        if (!System.IO.File.Exists(chemin))
        return;

        string donneesSysteme = System.IO.File.ReadAllText(chemin);
        DoneesScene donneesCharger = JsonUtility.FromJson<DoneesScene>(donneesSysteme);

        foreach (CelestialBody planete in NBodySimulation.celestialBodies) {
            Destroy(planete.gameObject);
        }

        for (int i = 0; i < donneesCharger.listeDonneesPlanetes.Count; i++) {
            GameObject newPlanet = NBodySimulation.Instance.CreatePlanet(donneesCharger.listeCoordonnees[i], donneesCharger.listeDonneesPlanetes[i]);

            if (donneesCharger.listeDonneesPlanetes[i].name == donneesCharger.relativeBody) {
                NBodySimulation.Instance.referenceBody = newPlanet.GetComponent<CelestialBody>();
            }
        }

        NBodySimulation.Instance.gravConstant = donneesCharger.gravConstant;
        NBodySimulation.Instance.planetGravity = donneesCharger.planetGravity;
    }

    [System.Serializable]
    public class DoneesScene {
        public List<PlanetSettings> listeDonneesPlanetes = new List<PlanetSettings>{};
        public List<Vector3> listeCoordonnees = new List<Vector3>{};

        public float gravConstant;
        public bool planetGravity;
        public string relativeBody;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            string horodatage = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            SauveragerSystemePlanetaire("sauvegarde_" + horodatage);
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            FindFirstObjectByType<MenuSauvegardes>().AfficherMenu();
        }
    }
}