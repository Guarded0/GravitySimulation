using UnityEngine;

public class Cible : MonoBehaviour
{
    public static Transform current;
    public LayerMask layerMask;
    public KeyCode deselectKey;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            choisirCible();
        }
        if (Input.GetKey(deselectKey))
        {
            current = null;
        }
    }
    void choisirCible()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, float.MaxValue, layerMask))
        {
            current = hit.transform;
        }
    }
}
