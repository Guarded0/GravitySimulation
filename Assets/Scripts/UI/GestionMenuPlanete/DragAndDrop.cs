using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DragAndDrop : MonoBehaviour
{
    public Transform pointeur;
    private Boolean activer = false;
    private Vector3 coordone;
    private PlanetSettings settingPlaneteACree;
    public LayerMask layerMask;
    public Transform listBoutonPlanet;
    public Transform listBoutonEtoile;
    public GameObject prefabBouton;
    public TMP_InputField nomsNouveauBouton;
    public GameObject boutonCreationBouton;
    private Vector3 vitesse;
    public GameObject prefabAxeVitesse;
    private string cheminPresetBouton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cheminPresetBouton = Application.persistentDataPath + "/" + "Preset" + ".json";
        chargerBouton();
        afficherBoutonPlanet(); 
    
    }
    void Awake()
    {
       boutonCreationBouton.GetComponent<Button>().onClick.AddListener(() => creeBouton(Cible.current.GetComponent<CelestialBody>().planetSettings, nomsNouveauBouton.text ));
    }
    // Update is called once per frame
    void Update()
    {

        if(Input.GetKey(KeyCode.H)){
            sauvegarderBouton();
        }
        if(Input.GetKey(KeyCode.J)){
            chargerBouton();
        }
        if (activer)
        {
            RaycastHit hit;
            if (lancerRayonSurPlan(out hit))
            {
                coordone = hit.point;
                pointeur.position = coordone;
            }
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(creerPlanete());
            }
        }
    }
    public bool lancerRayonSurPlan(out RaycastHit raycastHit)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out raycastHit, float.MaxValue, layerMask);
    }
    
    public IEnumerator creerPlanete()
    {
        activer = false;
        yield return new WaitForEndOfFrame();
        yield return null;

        RaycastHit raycastHit;
        // SPEED
        PlanetSettings newPlanetSettings = new PlanetSettings(settingPlaneteACree);
        prefabAxeVitesse.SetActive(true);
        while (true)
        {

            lancerRayonSurPlan(out raycastHit);
            vitesse = raycastHit.point - coordone;
            newPlanetSettings.velocity = vitesse;
            OrbitPathRenderer.CreateTemporaryVirtualBody(coordone, newPlanetSettings);
            prefabAxeVitesse.transform.position = coordone;
            prefabAxeVitesse.transform.up = vitesse.normalized;
            prefabAxeVitesse.transform.localScale = new Vector3(0.5f, vitesse.magnitude / 8f, 0.5f);
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }
        NBodySimulation.Instance.CreatePlanet(coordone, newPlanetSettings);
        OrbitPathRenderer.ClearTemporaryVirtualBodies();
        pointeur.gameObject.SetActive(false);
        prefabAxeVitesse.SetActive(false);
    }
    public void demarerConstruction(ButtonPrefab buttonPrefab)
    {
        activer = true;
        settingPlaneteACree = buttonPrefab.settings;
        pointeur.transform.localScale = new Vector3(settingPlaneteACree.radius, settingPlaneteACree.radius, settingPlaneteACree.radius) * 2;
        pointeur.gameObject.SetActive(true);
    }
    


    public void creeBouton(PlanetSettings settings, String noms)
    {
        if (settings != null )
        {
            GameObject nouveauBouton = Instantiate(prefabBouton, new Vector3(0, 0, 0), Quaternion.identity);
            nouveauBouton.GetComponent<ButtonPrefab>().settings = settings;
            nouveauBouton.GetComponentInChildren<TMP_Text>().text = noms; 

            if(nouveauBouton.GetComponent<ButtonPrefab>().settings.bodyType == BodyType.Planet){
            nouveauBouton.transform.SetParent(listBoutonPlanet); 
            } else{
               nouveauBouton.transform.SetParent(listBoutonEtoile); 
            }

            nouveauBouton.GetComponent<Button>().onClick.AddListener(() => demarerConstruction(nouveauBouton.GetComponent<ButtonPrefab>()));
        }

    }
    public void afficherBoutonPlanet(){
        listBoutonEtoile.gameObject.SetActive(false);
        listBoutonPlanet.gameObject.SetActive(true);
    }
    public void afficherBoutonEtoile(){
        listBoutonPlanet.gameObject.SetActive(false);
        listBoutonEtoile.gameObject.SetActive(true);
    }

    public void sauvegarderBouton(){
        DoneesBouton doneesBouton = new DoneesBouton();
        foreach (Transform bouton in listBoutonPlanet){
            ajouterDoneesList(bouton.gameObject, doneesBouton);
        }
        foreach (GameObject bouton in listBoutonEtoile){
            ajouterDoneesList(bouton.gameObject, doneesBouton);
        }
 
       
        string donneesListBouton = JsonUtility.ToJson(doneesBouton, true);
    
        System.IO.File.WriteAllText(cheminPresetBouton, donneesListBouton);
    
    }
    public void chargerBouton(){
        
        foreach (Transform bouton in listBoutonPlanet){
            Destroy(bouton.gameObject);
        }
       
        foreach (Transform bouton in listBoutonEtoile){
            Destroy(bouton.gameObject);
        }
       

        string donneesBouton = System.IO.File.ReadAllText(cheminPresetBouton);

        DoneesBouton doneesBoutonCharger = JsonUtility.FromJson<DoneesBouton>(donneesBouton);
        Debug.Log(doneesBoutonCharger.listNoms[0]);
        for (int i = 0; i < doneesBoutonCharger.listeDonneesPlanetes.Count; i++){
            creeBouton(doneesBoutonCharger.listeDonneesPlanetes[i], doneesBoutonCharger.listNoms[i]);
        }
        
    }

    private void ajouterDoneesList(GameObject bouton, DoneesBouton doneesBouton)
    {
        doneesBouton.listeDonneesPlanetes.Add(bouton.GetComponent<ButtonPrefab>().settings);
        doneesBouton.listNoms.Add(bouton.GetComponentInChildren<TMP_Text>().text);
    }

    [System.Serializable]
    public class DoneesBouton {
        public List<PlanetSettings> listeDonneesPlanetes = new List<PlanetSettings>{};
        public List<String> listNoms = new List<string>{};
    }
}
