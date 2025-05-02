using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.MaxValue, layerMask))
            {
                coordone = hit.point;
                pointeur.position = coordone;
            }
            if (Input.GetMouseButtonDown(0))
            {
                NBodySimulation.Instance.CreatePlanet(coordone, settingPlaneteACree, "New planete");
                activer = false;
                pointeur.gameObject.SetActive(false);
            }

        }
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
