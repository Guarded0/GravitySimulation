using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public class SauveragerSysteme : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/data.json";
    }

    void SauveragerSystemePlanetaire() {
        DoneesScene doneesScene = new DoneesScene();

        foreach (CelestialBody planete in NBodySimulation.celestialBodies) {
            doneesScene.listeDonneesPlanetes.Add(planete.planetSettings);
            doneesScene.listeCoordonnees.Add(planete.transform.position);
        }

        doneesScene.gravConstant = NBodySimulation.Instance.gravConstant;
        doneesScene.planetGravity = NBodySimulation.Instance.planetGravity;

        if (NBodySimulation.Instance.relativeBody != null) {
        doneesScene.relativeBody = NBodySimulation.Instance.relativeBody.name;
        }

        string donneesSysteme = JsonUtility.ToJson(doneesScene, true);
        System.IO.File.WriteAllText(filePath, donneesSysteme);
        Debug.Log(filePath);
    }

    void ChargerSystemePlanetaire() {
        foreach(CelestialBody planete in NBodySimulation.celestialBodies) {
            Destroy(planete.gameObject);
        }

        string donneesSysteme = System.IO.File.ReadAllText(filePath);
        DoneesScene donneesCharger = JsonUtility.FromJson<DoneesScene>(donneesSysteme);

        for (int i = 0; i < donneesCharger.listeDonneesPlanetes.Count; i++) {
            NBodySimulation.Instance.CreatePlanet(donneesCharger.listeCoordonnees[i], donneesCharger.listeDonneesPlanetes[i]);
            
            if (donneesCharger.relativeBody != null) {
                NBodySimulation.Instance.relativeBody.name = donneesCharger.relativeBody;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            SauveragerSystemePlanetaire();
        } if (Input.GetKeyDown(KeyCode.L)) {
            ChargerSystemePlanetaire();
        }
    }
}
