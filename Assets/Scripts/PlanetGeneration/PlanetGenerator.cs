using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[ExecuteInEditMode]
public class PlanetGenerator : MonoBehaviour
{
    public int seed;
    public int initialSphereResolution = 10;
    public float planetRadius = 4f;
    public Shader planetShader;
    public ComputeShader PlanetHeightShader;

    
    public SimpleNoiseSettings baseNoiseSettings;
    public RidgidNoiseSettings ridgidNoiseSettings;
    public SimpleNoiseSettings ridgidMaskNoiseSettings;

    public ColorTextureGenerator colorTextureGenerator;
    public OceanSettings oceanSettings;
    public AtmosphereSettings atmosphereSettings;
    public bool needsMeshUpdate = false;

    private Vector2 sphereBounds = Vector2.zero;
    public float blendStrength = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        needsMeshUpdate = true;
        //GeneratePlanet();
    }
    private void Update()
    {
        if (needsMeshUpdate)
        {
            needsMeshUpdate=false;
            GenerateMesh();
        }
    }
    public void GenerateMesh()
    {
       
        // create base sphere
        SphereMesh sphere = new SphereMesh(initialSphereResolution);
        // prepare compute shader buffers
        ComputeBuffer verticesBuffer = new ComputeBuffer(sphere.Vertices.Length, sizeof(float) * 3);
        ComputeBuffer heightsBuffer = new ComputeBuffer(sphere.Vertices.Length, sizeof(float));
        // create height map 
        float[] initialHeights = new float[sphere.Vertices.Length];
        for (int i = 0; i < initialHeights.Length; i++)
        {
            initialHeights[i] = 1.0f;
        }
        verticesBuffer.SetData(sphere.Vertices);
        heightsBuffer.SetData(initialHeights);
        // apply compute shader effects
        // general shape
        PlanetHeightShader.SetBuffer(0, "vertices", verticesBuffer);
        PlanetHeightShader.SetBuffer(0, "heights", heightsBuffer);
        PlanetHeightShader.SetFloat("seed", seed);
        PlanetHeightShader.SetFloat("initialHeight", planetRadius);
        PlanetHeightShader.SetFloats("baseNoiseParams", baseNoiseSettings.GetValues());
        PlanetHeightShader.SetFloats("ridgidNoiseParams", ridgidNoiseSettings.GetValues());
        PlanetHeightShader.SetFloats("ridgidMaskNoiseParams", ridgidMaskNoiseSettings.GetValues());
        PlanetHeightShader.SetFloat("blend", blendStrength);
        PlanetHeightShader.Dispatch(0, 512, 1,1);
        float[] heights = new float[sphere.Vertices.Length];
        heightsBuffer.GetData(heights,0,0, sphere.Vertices.Length);
        // could maybe do this on shader...?
        Vector3[] vertices = sphere.Vertices;
        sphereBounds = new Vector2(Mathf.Infinity,0f);
        for (int i = 0;  i < sphere.Vertices.Length; i++)
        {
            vertices[i] = vertices[i] * heights[i];
            if (heights[i] < sphereBounds.x) sphereBounds.x = heights[i];
            if (heights[i] > sphereBounds.y) sphereBounds.y = heights[i];
        }
        atmosphereSettings.planetRadius = (sphereBounds.x + sphereBounds.y)/2;
        // generate mesh
        SetMesh(vertices, sphere.Triangles);

        verticesBuffer.Release();
        heightsBuffer.Release();

        // colors
        
        colorTextureGenerator.UpdateTexture();
        UpdateMaterial();
    }
    void GeneratePlanet()
    {
        GenerateMesh();
    }
    void SetMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new Mesh();


        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (gameObject.GetComponent<MeshFilter>() != null)
        {
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }
    }
    void UpdateMaterial()
    {
        Material material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
        material.SetVector("_elevationBounds", new Vector4(sphereBounds.x, sphereBounds.y, 0f, 0f));
        material.SetTexture("_texture", colorTextureGenerator.texture);
    }
    void recreateMaterial()
    {
        Material material = new Material(planetShader);
        gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
        UpdateMaterial();
    }


    [CustomEditor(typeof(PlanetGenerator))]
    public class MyScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // Draw default inspector

            PlanetGenerator myScript = (PlanetGenerator)target;

            if (GUILayout.Button("Make unique material"))
            {
                myScript.recreateMaterial();
            }
        }
    }
}
