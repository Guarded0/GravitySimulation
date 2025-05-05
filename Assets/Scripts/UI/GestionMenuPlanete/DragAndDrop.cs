using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

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
    private Vector3 vitesse;
    public GameObject prefabAxeVitesse;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        afficherBoutonPlanet(); 
    }
    void Awake()
    {
    }
    // Update is called once per frame
    void Update()
    {
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

        RaycastHit raycastHit;
        // SPEED
        PlanetSettings newPlanetSettings = new PlanetSettings(settingPlaneteACree);
        prefabAxeVitesse.SetActive(true);
        while (true)
        {

            lancerRayonSurPlan(out raycastHit);
            vitesse = raycastHit.point - coordone;
            newPlanetSettings.velocity = vitesse;
            OrbitDebugDisplay.CreateTemporaryVirtualBody(coordone, newPlanetSettings);
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

    public void creeBouton()
    {
        if (Cible.current != null)
        {
            GameObject nouveauBouton = Instantiate(prefabBouton, new Vector3(0, 0, 0), Quaternion.identity);
            nouveauBouton.GetComponent<ButtonPrefab>().settings = Cible.current.GetComponent<CelestialBody>().planetSettings;
            nouveauBouton.GetComponentInChildren<TMP_Text>().text = nomsNouveauBouton.text;

            if(Cible.current.GetComponent<CelestialBody>().planetSettings.bodyType == BodyType.Planet){
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
}
