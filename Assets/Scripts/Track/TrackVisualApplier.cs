using UnityEngine;

/// <summary>
/// Applies selected track visuals at runtime — sky, fog, ground color, lighting.
/// Attach to any scene object. Activates when TrackSelector confirms.
/// </summary>
public class TrackVisualApplier : MonoBehaviour
{
    private bool applied = false;

    void Update()
    {
        if (applied) return;

        // Wait until track selector is done
        var selector = FindFirstObjectByType<TrackSelector>();
        if (selector != null && selector.gameObject.activeSelf) return;

        applied = true;
        ApplyTrackVisuals();
    }

    void ApplyTrackVisuals()
    {
        var track = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex];

        // Sky / ambient
        RenderSettings.ambientSkyColor = track.skyColor;
        RenderSettings.ambientEquatorColor = Color.Lerp(track.skyColor, track.groundColor, 0.5f);
        RenderSettings.ambientGroundColor = track.groundColor;

        // Fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = track.fogColor;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = track.isNight ? 30f : 100f;
        RenderSettings.fogEndDistance = track.isNight ? 200f : 600f;

        // Camera background
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = track.skyColor;
        }

        // Sun / directional light
        var sun = GameObject.Find("Directional Light");
        if (sun != null)
        {
            var light = sun.GetComponent<Light>();
            if (track.isNight)
            {
                light.intensity = 0.3f;
                light.color = new Color(0.4f, 0.4f, 0.7f);
            }
            else
            {
                light.intensity = 1.5f;
                light.color = Color.Lerp(Color.white, track.skyColor, 0.2f);
            }
        }

        // Recolor road segments
        var roadParent = GameObject.Find("Road");
        if (roadParent != null)
        {
            foreach (var renderer in roadParent.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = track.roadColor;
                if (track.name == "Neon Circuit")
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", new Color(0, 0.3f, 0.6f) * 0.5f);
                }
            }
        }

        // Recolor grass/ground
        var grass = GameObject.Find("Grass");
        if (grass != null)
        {
            grass.GetComponent<Renderer>().material.color = track.groundColor;
        }

        // Night mode — add point lights for street lamps
        if (track.isNight)
        {
            AddStreetLights();
        }

        // Neon Circuit — add glowing edges
        if (track.name == "Neon Circuit")
        {
            AddNeonEdges();
        }

        Debug.Log($"✅ Track applied: {track.name}");
    }

    void AddStreetLights()
    {
        var lightsParent = new GameObject("StreetLights");
        for (int i = 0; i < 16; i++)
        {
            float angle = (float)i / 16 * Mathf.PI * 2;
            var track = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex];
            float rx = track.trackScaleX + 10f;
            float rz = track.trackScaleZ + 10f;
            float x = Mathf.Cos(angle) * rx;
            float z = Mathf.Sin(angle) * rz;

            var lightObj = new GameObject($"StreetLight_{i}");
            lightObj.transform.parent = lightsParent.transform;
            lightObj.transform.position = new Vector3(x, 8f, z);

            var pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.range = 25f;
            pointLight.intensity = 2f;

            if (TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex].name == "Neon Circuit")
            {
                Color[] neonColors = { Color.cyan, Color.magenta, new Color(1, 0.3f, 0), Color.green };
                pointLight.color = neonColors[i % neonColors.Length];
                pointLight.intensity = 3f;
            }
            else
            {
                pointLight.color = new Color(1f, 0.85f, 0.5f); // Warm street lamp
            }
        }
    }

    void AddNeonEdges()
    {
        // Add glowing strips along track edges
        var neonParent = new GameObject("NeonEdges");
        var track = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex];

        for (int i = 0; i < 40; i++)
        {
            float angle = (float)i / 40 * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * track.trackScaleX;
            float z = Mathf.Sin(angle) * track.trackScaleZ;
            float nextAngle = (float)(i + 1) / 40 * Mathf.PI * 2;
            float nx = Mathf.Cos(nextAngle) * track.trackScaleX;
            float nz = Mathf.Sin(nextAngle) * track.trackScaleZ;

            Vector3 pos = new Vector3((x + nx) / 2, 0.1f, (z + nz) / 2);
            Vector3 dir = new Vector3(nx - x, 0, nz - z).normalized;

            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "NeonStrip";
            strip.transform.parent = neonParent.transform;
            strip.transform.position = pos;
            strip.transform.rotation = Quaternion.LookRotation(dir);
            strip.transform.localScale = new Vector3(0.3f, 0.2f, Vector3.Distance(new Vector3(x, 0, z), new Vector3(nx, 0, nz)));

            var mat = new Material(Shader.Find("Standard"));
            Color neonColor = (i % 2 == 0) ? Color.cyan : new Color(1f, 0f, 0.5f);
            mat.color = neonColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", neonColor * 2f);
            strip.GetComponent<Renderer>().material = mat;
            Destroy(strip.GetComponent<Collider>());
        }
    }
}
