using UnityEngine;

public class Supression : MonoBehaviour
{
    private GameObject bouton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void Awake()
    {
        bouton = this.gameObject;   
    }
   public void autodestruction(){
    Destroy(bouton);
   }

}
