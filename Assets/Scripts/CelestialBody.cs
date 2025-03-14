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

    public TrailRenderer trailRenderer;
    public LineRenderer trajectoryRenderer;

    private void Awake()
    {
        // get rigidbody
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        trajectoryRenderer = GetComponent<LineRenderer>();
        if (trailRenderer == null )
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        if (trajectoryRenderer == null )
        {
            trajectoryRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (isAnchored)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }


        // setup trail
        trailRenderer.time = 15f;
        trailRenderer.startColor = Color.white;
        trailRenderer.endColor = Color.white;
        trailRenderer.startWidth = 0.2f;
        // setup trajectory
        trajectoryRenderer.startColor = Color.white;
        trajectoryRenderer.endColor = Color.white;
        trajectoryRenderer.startWidth = 0.1f;

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set velocity to starting one
        velocity = initialVelocity;
        NBodySimulation.planetAdded.Invoke(gameObject);

    }
    private void OnDestroy()
    {
        NBodySimulation.planetRemoved.Invoke(gameObject);//
    }
}
