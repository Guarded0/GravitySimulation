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

        while (true)
        {

            lancerRayonSurPlan(out raycastHit);
            vitesse = raycastHit.point - coordone;
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }

        settingPlaneteACree.velocity = vitesse;
        NBodySimulation.Instance.CreatePlanet(coordone, settingPlaneteACree);
        pointeur.gameObject.SetActive(false);
       

    }
    public void demarerConstruction(ButtonPrefab buttonPrefab)
    {
        activer = true;
        settingPlaneteACree = buttonPrefab.settings;
        pointeur.gameObject.SetActive(true);
    }

    public void creeBouton()
    {
        if (Cible.current != null)
        {
            Debug.Log("star");
            GameObject nouveauBouton = Instantiate(prefabBouton, new Vector3(0, 0, 0), Quaternion.identity);
            Debug.Log("Instant");
            nouveauBouton.GetComponent<ButtonPrefab>().settings = Cible.current.GetComponent<CelestialBody>().planetSettings;
            nouveauBouton.GetComponentInChildren<TMP_Text>().text = nomsNouveauBouton.text;
            if(Cible.current.GetComponent<CelestialBody>().planetSettings.bodyType == BodyType.Planet){
            nouveauBouton.transform.SetParent(listBoutonPlanet); 
            } else{
               nouveauBouton.transform.SetParent(listBoutonEtoile); 
            }
            Debug.Log("3");
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
