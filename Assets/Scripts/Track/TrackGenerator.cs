using UnityEngine;

/// <summary>
/// Generates a highway-style track at runtime.
/// Creates road mesh, lane markings, guardrails, and scenery.
/// </summary>
public class TrackGenerator : MonoBehaviour
{
    [Header("Track Shape")]
    public int segments = 100;
    public float trackLength = 2000f;
    public float trackWidth = 12f;
    public float curveFrequency = 0.005f;
    public float curveAmplitude = 50f;

    [Header("Scenery")]
    public GameObject treePrefab;
    public GameObject polePrefab;
    public GameObject guardrailPrefab;

    [Header("Materials")]
    public Material roadMaterial;
    public Material grassMaterial;
    public Material lineYellowMaterial;
    public Material lineWhiteMaterial;

    private Vector3[] trackPoints;

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        // Generate track center points (long highway with gentle curves)
        trackPoints = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float z = t * trackLength;
            float x = Mathf.Sin(z * curveFrequency) * curveAmplitude
                    + Mathf.Sin(z * curveFrequency * 2.3f) * curveAmplitude * 0.3f;
            trackPoints[i] = new Vector3(x, 0, z);
        }

        CreateRoadMesh();
        CreateLaneMarkings();
        CreateGuardrails();
        CreateScenery();
    }

    void CreateRoadMesh()
    {
        GameObject road = new GameObject("Road");
        road.transform.parent = transform;
        MeshFilter mf = road.AddComponent<MeshFilter>();
        MeshRenderer mr = road.AddComponent<MeshRenderer>();
        MeshCollider mc = road.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        Vector3[] vertices = new Vector3[segments * 2];
        Vector2[] uvs = new Vector2[segments * 2];
        int[] triangles = new int[(segments - 1) * 6];

        for (int i = 0; i < segments; i++)
        {
            Vector3 forward = (i < segments - 1)
                ? (trackPoints[i + 1] - trackPoints[i]).normalized
                : (trackPoints[i] - trackPoints[i - 1]).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            vertices[i * 2] = trackPoints[i] - right * trackWidth * 0.5f;
            vertices[i * 2 + 1] = trackPoints[i] + right * trackWidth * 0.5f;

            uvs[i * 2] = new Vector2(0, (float)i / segments * 50f);
            uvs[i * 2 + 1] = new Vector2(1, (float)i / segments * 50f);
        }

        for (int i = 0; i < segments - 1; i++)
        {
            int ti = i * 6;
            int vi = i * 2;
            triangles[ti] = vi;
            triangles[ti + 1] = vi + 2;
            triangles[ti + 2] = vi + 1;
            triangles[ti + 3] = vi + 1;
            triangles[ti + 4] = vi + 2;
            triangles[ti + 5] = vi + 3;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mf.mesh = mesh;
        mr.material = roadMaterial;
        mc.sharedMesh = mesh;
    }

    void CreateLaneMarkings()
    {
        // Yellow center dashes
        for (int i = 0; i < segments - 1; i += 4)
        {
            Vector3 pos = trackPoints[i];
            Vector3 next = trackPoints[Mathf.Min(i + 1, segments - 1)];
            Vector3 dir = (next - pos).normalized;

            GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mark.transform.parent = transform;
            mark.transform.position = pos + Vector3.up * 0.02f;
            mark.transform.rotation = Quaternion.LookRotation(dir);
            mark.transform.localScale = new Vector3(0.15f, 0.02f, trackLength / segments * 2f);
            mark.GetComponent<Renderer>().material = lineYellowMaterial;
            Destroy(mark.GetComponent<Collider>());
        }

        // White edge lines
        for (int i = 0; i < segments - 1; i += 2)
        {
            Vector3 pos = trackPoints[i];
            Vector3 next = trackPoints[Mathf.Min(i + 1, segments - 1)];
            Vector3 dir = (next - pos).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

            float edgeOffset = trackWidth * 0.48f;

            // Left edge
            GameObject leftMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftMark.transform.parent = transform;
            leftMark.transform.position = pos - right * edgeOffset + Vector3.up * 0.02f;
            leftMark.transform.rotation = Quaternion.LookRotation(dir);
            leftMark.transform.localScale = new Vector3(0.12f, 0.02f, trackLength / segments);
            leftMark.GetComponent<Renderer>().material = lineWhiteMaterial;
            Destroy(leftMark.GetComponent<Collider>());

            // Right edge
            GameObject rightMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightMark.transform.parent = transform;
            rightMark.transform.position = pos + right * edgeOffset + Vector3.up * 0.02f;
            rightMark.transform.rotation = Quaternion.LookRotation(dir);
            rightMark.transform.localScale = new Vector3(0.12f, 0.02f, trackLength / segments);
            rightMark.GetComponent<Renderer>().material = lineWhiteMaterial;
            Destroy(rightMark.GetComponent<Collider>());
        }
    }

    void CreateGuardrails()
    {
        if (guardrailPrefab == null) return;

        for (int i = 0; i < segments; i += 3)
        {
            Vector3 pos = trackPoints[i];
            Vector3 next = trackPoints[Mathf.Min(i + 1, segments - 1)];
            Vector3 dir = (next - pos).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

            // Left guardrail
            Instantiate(guardrailPrefab, pos - right * (trackWidth * 0.55f), Quaternion.LookRotation(dir), transform);
            // Right guardrail
            Instantiate(guardrailPrefab, pos + right * (trackWidth * 0.55f), Quaternion.LookRotation(dir), transform);
        }
    }

    void CreateScenery()
    {
        for (int i = 0; i < segments; i += 5)
        {
            Vector3 pos = trackPoints[i];
            Vector3 next = trackPoints[Mathf.Min(i + 1, segments - 1)];
            Vector3 dir = (next - pos).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

            // Trees on both sides
            if (treePrefab != null)
            {
                float offset = trackWidth * 0.5f + Random.Range(3f, 8f);
                if (Random.value > 0.3f)
                    Instantiate(treePrefab, pos - right * offset, Quaternion.identity, transform);
                if (Random.value > 0.3f)
                    Instantiate(treePrefab, pos + right * offset, Quaternion.identity, transform);
            }

            // Power poles (right side only, every 10 segments)
            if (polePrefab != null && i % 10 == 0)
            {
                float poleOffset = trackWidth * 0.5f + 2f;
                Instantiate(polePrefab, pos + right * poleOffset, Quaternion.LookRotation(dir), transform);
            }
        }
    }

    /// <summary>Get waypoints for AI opponents.</summary>
    public Vector3[] GetTrackPoints() => trackPoints;
}
