using Unity.VisualScripting;
using UnityEngine;
[ExecuteAlways]
public class LagrangeDebugDisplay : MonoBehaviour
{
    public CelestialBody body1;
    public CelestialBody body2;
    public bool displayPoints = true;
    public float radius = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    float calculateHillSphere(float mass1, float mass2, float distance)
    {
        return distance * Mathf.Pow(mass2/(3 *mass1), 1f / 3f);
    }
    // MASS 1 LARGEST
    // MASS 2 SMALLEST
    // MASS 1 >>>>> MASS 2
    private void OnDrawGizmos()
    {
        if (displayPoints)
        {
            if (body1 != null && body2 != null)
            {
                radius = calculateHillSphere(body1.mass, body2.mass, (body2.transform.position - body1.transform.position).magnitude);
                Gizmos.DrawSphere(body2.transform.position + ((body1.transform.position - body2.transform.position).normalized * radius), 1f);
            }
        }
    }

}
