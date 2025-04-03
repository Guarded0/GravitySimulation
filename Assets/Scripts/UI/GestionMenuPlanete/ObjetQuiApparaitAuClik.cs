using UnityEngine;

public class ShowOnClick : MonoBehaviour
{
    public GameObject BoiteQuiApparait; 

    private void OnMouseDown()  
    {   
        BoiteQuiApparait.SetActive(!BoiteQuiApparait.activeSelf);
    }
}
