using UnityEngine;

public class CameraMouvement : MonoBehaviour
{
    
    private Camera cam;
    public float vitessetAvant = 50f;

    void Awake()
    {
        cam = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W)){
        }
    }
}
