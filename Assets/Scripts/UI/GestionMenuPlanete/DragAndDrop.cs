using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour
{
    public Transform pointeur;
    private Boolean activer = false;
    private Vector3 coordone;
    private PlanetSettings settingPlaneteACree;
    public LayerMask layerMask;
    public Transform listBouton;
    public GameObject prefabBouton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
            GameObject nouveauBouton = Instantiate(prefabBouton, new Vector3(0, 0, 0), Quaternion.identity);
            nouveauBouton.GetComponent<ButtonPrefab>().settings = Cible.current.GetComponent<CelestialBody>().planetSettings;
            nouveauBouton.transform.SetParent(listBouton, false);
        }
    }
}
