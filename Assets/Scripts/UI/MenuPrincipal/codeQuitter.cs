using UnityEngine;

public class codeQuitter : MonoBehaviour
{

    public GameObject menuBoutonPlanete; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }
    


    // Update is called once per frame
    void Update()
    {
        
    }
    public void QuitGame()
{
    menuBoutonPlanete.GetComponent<DragAndDrop>().sauvegarderBouton();
    //Application.Quit();
}
}
