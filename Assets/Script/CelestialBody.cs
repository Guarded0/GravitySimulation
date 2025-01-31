using UnityEngine;


public class CelestialBody : MonoBehaviour
{
    public float mass;
    public bool isAnchored = false;
    public bool isPlanet = false;
    public bool hasGravity = true;
    public Vector3 initialVelocity = Vector3.zero;
    public Vector3 velocity = Vector3.zero;


    public Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (isAnchored)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = initialVelocity;
    }
}
