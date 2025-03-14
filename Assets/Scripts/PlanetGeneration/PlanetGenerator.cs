using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[ExecuteInEditMode]
public class PlanetGenerator : MonoBehaviour
{

    public int initialSphereResolution = 10;
    public Shader planetShader;
    public ComputeShader shapeNoiseShader;
    public ComputeShader detailsShader;
    public ComputeShader ridgesShader;

    
    public SimpleNoiseSettings baseNoiseSettings;
    public SimpleNoiseSettings detailsNoiseSettings;
    public RidgesNoiseSettings ridgesNoiseSettings;

    public ColorTextureGenerator colorTextureGenerator;
    public OceanSettings oceanSettings;

    private bool needsMeshUpdate = false;

    private Vector2 sphereBounds = Vector2.zero;
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
        shapeNoiseShader.SetBuffer(0, "vertices", verticesBuffer);
        shapeNoiseShader.SetBuffer(0, "heights", heightsBuffer);
        shapeNoiseShader.SetInt("octaves", baseNoiseSettings.numLayers);
        shapeNoiseShader.SetFloat("lacunarity", baseNoiseSettings.lacunarity);
        shapeNoiseShader.SetFloat("persistence", baseNoiseSettings.persistence);
        shapeNoiseShader.SetFloat("noiseScale", baseNoiseSettings.scale);
        shapeNoiseShader.SetFloat("elevation", baseNoiseSettings.elevation);
        shapeNoiseShader.SetFloats("offset",new float[3] {baseNoiseSettings.offset.x, baseNoiseSettings.offset.y, baseNoiseSettings.offset.z});
        shapeNoiseShader.SetFloat("weight", 1.0f);
        shapeNoiseShader.Dispatch(0, 512, 1,1);

        // fine details

        
        detailsShader.SetBuffer(0, "vertices", verticesBuffer);
        detailsShader.SetBuffer(0, "heights", heightsBuffer);
        detailsShader.SetInt("octaves", detailsNoiseSettings.numLayers);
        detailsShader.SetFloat("lacunarity", detailsNoiseSettings.lacunarity);
        detailsShader.SetFloat("persistence", detailsNoiseSettings.persistence);
        detailsShader.SetFloat("noiseScale", detailsNoiseSettings.scale);
        detailsShader.SetFloat("elevation", detailsNoiseSettings.elevation);
        detailsShader.SetFloats("offset", new float[3] { detailsNoiseSettings.offset.x, detailsNoiseSettings.offset.y, detailsNoiseSettings.offset.z });
        detailsShader.SetFloat("weight", 0.05f);
        detailsShader.Dispatch(0, 512, 1, 1);

        ridgesShader.SetBuffer(0, "vertices", verticesBuffer);
        ridgesShader.SetBuffer(0, "heights", heightsBuffer);
        ridgesShader.SetInt("numLayers", ridgesNoiseSettings.numLayers);
        ridgesShader.SetFloat("persistence", ridgesNoiseSettings.persistence);
        ridgesShader.SetFloat("lacunarity", ridgesNoiseSettings.lacunarity);
        ridgesShader.SetFloat("noiseScale", ridgesNoiseSettings.noiseScale);
        ridgesShader.SetFloat("weight", ridgesNoiseSettings.weight);
        ridgesShader.SetFloat("power", ridgesNoiseSettings.power);
        ridgesShader.SetFloat("gain", ridgesNoiseSettings.gain);
        ridgesShader.SetFloat("elevation", ridgesNoiseSettings.elevation);
        ridgesShader.Dispatch(0, 512, 1, 1);


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
