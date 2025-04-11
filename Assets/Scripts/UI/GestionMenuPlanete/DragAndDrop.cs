using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour
{
    public Transform pointeur;
    private Boolean activer = false;
    private Vector3 coordone;
    private PlanetSettings planeteACree;
    public LayerMask layerMask;
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
        if(activer){
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, float.MaxValue, layerMask)){
                coordone = hit.point; 
                pointeur.position = coordone; 
            }
            if(Input.GetMouseButtonDown(0)){
                CelestialBody nouvellePlanet = new CelestialBody();
            
                Instantiate(nouvellePlanet, coordone, Quaternion.identity);   
                nouvellePlanet.planetSettings = planeteACree;             
                activer = false;            
            }
            
        }
    }
    
    public void demarerConstruction(ButtonPrefab buttonPrefab){
        activer = true;
        planeteACree = buttonPrefab.settings;
    }
}
