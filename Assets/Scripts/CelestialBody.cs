using UnityEngine;
public enum BodyType
{
    Planet,
    Star
}
[ExecuteAlways]
public class CelestialBody : MonoBehaviour
{
    // if object is anchored it means it cant move at all
    public bool isAnchored = false;

    // if it has gravity
    public bool hasGravity = true;

    // gameobject rigidbody
    public Rigidbody rb;
    public PlanetGenerator planetGenerator;
    public TrailRenderer trailRenderer;
    public LineRenderer trajectoryRenderer;

    public PlanetSettings planetSettings;
    public bool shouldUpdateSettings { private get => false; set {
            if (value) SettingsTranslator.SettingsToVariables(this, planetGenerator);
        }
    }
    private void Awake()
    {
        // get rigidbody
        rb = GetComponent<Rigidbody>();
        planetGenerator = GetComponent<PlanetGenerator>();
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
        NBodySimulation.planetAdded.Invoke(gameObject);
    }

    private void OnDestroy()
    {
        NBodySimulation.planetRemoved.Invoke(gameObject);
    }
    private void OnValidate()
    {
        if (planetGenerator != null)
        {
            shouldUpdateSettings = true;
        }
    }
}
