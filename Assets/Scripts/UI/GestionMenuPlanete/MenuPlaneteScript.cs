using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuPlaneteScript : MonoBehaviour
{
    //Tranform du gameObject qui montre la position ou un nouvelle planète seras crée 
    public Transform pointeur;
    //true si la création d'un célestialBody est en cours 
    private Boolean activer = false;
    //Les coordoné de la souris 
    private Vector3 coordone;
    private PlanetSettings settingPlaneteACree;
    //Limite le layer sur le quelle le raycast peut hit 
    public LayerMask layerMask;
    public Transform listBoutonPlanet;
    public Transform listBoutonEtoile;
    public GameObject prefabBouton;
    public TMP_InputField nomsNouveauBouton;
    public GameObject boutonCreationBouton;
    private Vector3 vitesse;
    public GameObject prefabAxeVitesse;
    //Chemin ou sauvegarder les preset de bouton
    private string cheminPresetBouton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Définition du chemin 
        cheminPresetBouton = Application.persistentDataPath + "/" + "Preset" + ".json";
        chargerBouton();
        //afficher list de bouton par défault 
        afficherBoutonPlanet(); 
    
    }
    void Awake()
    { 
       boutonCreationBouton.GetComponent<Button>().onClick.AddListener(() => creeBouton(Cible.current.GetComponent<CelestialBody>().planetSettings, nomsNouveauBouton.text ));
    }
    // Update is called once per frame
    void Update()
    {
        if (activer)
        {
            RaycastHit hit;
            if (lancerRayonSurPlan(out hit))
            {
                //sauvegarde les coordoné de la souris et place le pointeur a cette position 
                coordone = hit.point;
                pointeur.position = coordone;
            }
            if (Input.GetMouseButtonDown(0))
            {
                //Lancement la création de la planète 
                StartCoroutine(creerPlanete());
            }
            if (Input.GetMouseButton(1))
            {
                //Annule la création de planète 
                activer = false;
                pointeur.gameObject.SetActive(false);
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
    /// <summary>
    /// Fonction appeler lorsque un des bouton est appuyer. 
    /// Définit settingPLaneteACree, ajuste la taille du transform de pointeur et active le gameObject de pointeur 
    /// </summary>
    /// <param name="buttonPrefab"></param> Cette objet contient les setting de la planète a crée
    public void demarerConstruction(ButtonPrefab buttonPrefab)
    {
        activer = true;
        settingPlaneteACree = buttonPrefab.settings;
        pointeur.transform.localScale = new Vector3(settingPlaneteACree.radius, settingPlaneteACree.radius, settingPlaneteACree.radius) * 2;
        pointeur.gameObject.SetActive(true);
    }
    

    /// <summary>
    /// Crée une instance de ButtonPrefab, définit ces settings et place le bouton soit dans listBoutonPlanet ou 
    /// dans listBoutonEtoile dépendament du BodyType de settings
    /// </summary>
    /// <param name="settings"></param> le PlanetSettings du bouton à crée 
    /// <param name="noms"></param> le text du bouton à crée 
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
    /// <summary>
    /// active listBoutonPlanet et désactive listBoutonEtoile
    /// </summary>
    public void afficherBoutonPlanet()
    {
        listBoutonEtoile.gameObject.SetActive(false);
        listBoutonPlanet.gameObject.SetActive(true);
    }
    /// <summary>
    /// active listBoutonEtoile et désactive listBoutonPlanet 
    /// </summary>
    public void afficherBoutonEtoile()
    {
        listBoutonPlanet.gameObject.SetActive(false);
        listBoutonEtoile.gameObject.SetActive(true);
    }
    /// <summary>
    /// sauvergarde chaque bouton une instance de la classe DoneesBouton et sauvegarde cette classe sous format jason 
    /// </summary>
    public void sauvegarderBouton()
    {
        DoneesBouton doneesBouton = new DoneesBouton();
        foreach (Transform bouton in listBoutonPlanet)
        {
            ajouterDoneesList(bouton.gameObject, doneesBouton);
        }
        foreach (GameObject bouton in listBoutonEtoile)
        {
            ajouterDoneesList(bouton.gameObject, doneesBouton);
        }


        string donneesListBouton = JsonUtility.ToJson(doneesBouton, true);

        System.IO.File.WriteAllText(cheminPresetBouton, donneesListBouton);

    }
    /// <summary>
    /// supprime les bouton qui pourrait être présent dans listBoutonPlanet et listBoutonEtoile 
    /// crée une instance de DoneesBouton a partir des donnée Json à la position cheminPresetBouton 
    /// crée chaque bouton grace à creeBouton 
    /// </summary>
    public void chargerBouton()
    {
        if (System.IO.File.Exists(cheminPresetBouton))
        {
            foreach (Transform bouton in listBoutonPlanet)
            {
                Destroy(bouton.gameObject);
            }

            foreach (Transform bouton in listBoutonEtoile)
            {
                Destroy(bouton.gameObject);
            }


            string donneesBouton = System.IO.File.ReadAllText(cheminPresetBouton);

            DoneesBouton doneesBoutonCharger = JsonUtility.FromJson<DoneesBouton>(donneesBouton);

            for (int i = 0; i < doneesBoutonCharger.listeDonneesPlanetes.Count; i++)
            {
                creeBouton(doneesBoutonCharger.listeDonneesPlanetes[i], doneesBoutonCharger.listNoms[i]);
            }
        }
    }
    
    /// <summary>
    /// ajoute le bouton a listeDoneesPlanetes et le text du bouton à listNoms 
    /// </summary>
    /// <param name="bouton"></param> gameObject du bouton à ajouter au liste 
    /// <param name="doneesBouton"></param> instance de la classe DoneesBouton ou il faut ajouter les donnes du bouton 
    private void ajouterDoneesList(GameObject bouton, DoneesBouton doneesBouton)
    {
        doneesBouton.listeDonneesPlanetes.Add(bouton.GetComponent<ButtonPrefab>().settings);
        doneesBouton.listNoms.Add(bouton.GetComponentInChildren<TMP_Text>().text);
    }

    /// <summary>
    /// Classe permettant de sauvegarder les information en liens avec les bouton qui serve à la création de planète 
    /// </summary>
    [System.Serializable]
    public class DoneesBouton {
        public List<PlanetSettings> listeDonneesPlanetes = new List<PlanetSettings>{};
        public List<String> listNoms = new List<string>{};
    }
}
