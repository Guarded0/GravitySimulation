using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MeshGenerator : MonoBehaviour
{

    public int initialSphereResolution = 10;
    public ComputeShader shapeNoiseShader;
    public ComputeShader detailsShader;

    public SimpleNoiseSettings baseNoiseSettings;

    [Header("Details noise")]
    public int detailOctaves = 0;

    public bool update = false;
    public Mesh currentMesh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
       
        // create base sphere
        SphereMesh sphere = new SphereMesh(initialSphereResolution);

        // prepare compute shader buffers
        ComputeBuffer verticesBuffer = new ComputeBuffer(sphere.Vertices.Length, sizeof(float) * 3);
        ComputeBuffer heightsBuffer = new ComputeBuffer(sphere.Vertices.Length, sizeof(float));
        verticesBuffer.SetData(sphere.Vertices);

        // apply compute shader effects
        // random noise Shader
        shapeNoiseShader.SetBuffer(0, "vertices", verticesBuffer);
        shapeNoiseShader.SetBuffer(0, "heights", heightsBuffer);
        shapeNoiseShader.SetInt("octaves", baseNoiseSettings.numLayers);
        shapeNoiseShader.SetFloat("lacunarity", baseNoiseSettings.lacunarity);
        shapeNoiseShader.SetFloat("persistence", baseNoiseSettings.persistence);
        shapeNoiseShader.SetFloat("noiseScale", baseNoiseSettings.scale);
        shapeNoiseShader.SetFloat("elevation", baseNoiseSettings.elevation);
        shapeNoiseShader.SetFloats("offset",new float[3] {baseNoiseSettings.offset.x, baseNoiseSettings.offset.y, baseNoiseSettings.offset.z});
        shapeNoiseShader.Dispatch(0, 512, 1,1);

        float[] heights = new float[sphere.Vertices.Length];
        heightsBuffer.GetData(heights,0,0, sphere.Vertices.Length);

        Vector3[] vertices = sphere.Vertices;
        for (int i = 0;  i < sphere.Vertices.Length; i++)
        {
            vertices[i] += vertices[i] * heights[i];
        }
        // generate mesh
        SetMesh(vertices, sphere.Triangles);

        verticesBuffer.Release();
        heightsBuffer.Release();
    }

    void SetMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh;
        if (gameObject.GetComponent<MeshFilter>() != null)
        {
            mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        }
        else
        {
            mesh = new Mesh();
        }
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (gameObject.GetComponent<MeshFilter>() == null)
        {
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}


[CustomEditor(typeof(MeshGenerator))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw default inspector

        MeshGenerator myScript = (MeshGenerator)target;

        if (GUILayout.Button("Generate mesh"))
        {
            myScript.GenerateMesh();
        }
    }
}