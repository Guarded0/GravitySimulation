using UnityEditor;
using UnityEngine;

public class nouveauPreset : MonoBehaviour
{
    public GameObject parent;
    public GameObject buttonPrefab;
    private GameObject nouveauBoutton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ajouterPreset(){
        nouveauBoutton = Instantiate(buttonPrefab, new Vector3(0,0,0), Quaternion.identity);
        nouveauBoutton.transform.parent = parent.transform;
        nouveauBoutton.GetComponent<ButtonPrefab>().settings = Cible.current.GetComponent<CelestialBody>().planetSettings;
    }
}
