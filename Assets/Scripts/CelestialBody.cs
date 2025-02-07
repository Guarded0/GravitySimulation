using UnityEngine;


public class CelestialBody : MonoBehaviour
{
    // mass of object
    public float mass;

    // if object is anchored it means it cant move at all
    public bool isAnchored = false;

    // if its a planet (if not it can be a star)
    public bool isPlanet = false;

    // if it has gravity
    public bool hasGravity = true;

    // velocity at start
    public Vector3 initialVelocity = Vector3.zero;

    // current velocity
    public Vector3 velocity = Vector3.zero;

    // gameobject rigidbody
    public Rigidbody rb;
    private void Awake()
    {
        // get rigidbody
        rb = GetComponent<Rigidbody>();
        if (isAnchored)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set velocity to starting one
        velocity = initialVelocity;
    }
}
