using UnityEngine;
public enum BodyType
{
    Planet,
    Star
}
[ExecuteAlways]
public class CelestialBody : MonoBehaviour
{
    // gameobject rigidbody
    public Rigidbody rb;
    private PlanetGenerator planetGenerator;
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

        // setup trail
        trailRenderer.time = 15f;
        trailRenderer.startWidth = 0.2f;
        // setup trajectory
        UpdateLineRenderers();
        trajectoryRenderer.startWidth = 0.1f;

        shouldUpdateSettings = true;
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
    void UpdateLineRenderers()
    {
        trailRenderer.startColor = planetSettings.surfaceColor.mid;
        trailRenderer.endColor = planetSettings.surfaceColor.mid;
        trajectoryRenderer.startColor = planetSettings.surfaceColor.mid;
        trajectoryRenderer.endColor = planetSettings.surfaceColor.mid;
    }
}
