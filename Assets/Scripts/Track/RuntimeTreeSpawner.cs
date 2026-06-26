using UnityEngine;

/// <summary>
/// Spawns trees at runtime AFTER track is selected.
/// Ensures trees are NEVER on the road.
/// </summary>
public class RuntimeTreeSpawner : MonoBehaviour
{
    private bool spawned = false;

    void Update()
    {
        if (spawned) return;

        // Wait until track selector is done
        var trackSel = FindFirstObjectByType<TrackSelector>();
        if (trackSel != null && trackSel.gameObject.activeSelf) return;

        spawned = true;
        SpawnTrees();
    }

    void SpawnTrees()
    {
        // Get selected track info
        float rz = 100f;
        float rx = 150f;
        if (TrackSelector.AvailableTracks != null && TrackSelector.SelectedTrackIndex < TrackSelector.AvailableTracks.Length)
        {
            rx = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex].trackScaleX;
            rz = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex].trackScaleZ;
        }

        bool isHighway = rz < 50f;

        for (int i = 0; i < 40; i++)
        {
            Vector3 treePos;

            if (isHighway)
            {
                // Highway: road along Z axis, center at X=0, width=20m
                // Trees at X = ±40 to ±60 (VERY far from road)
                float side = (i % 2 == 0) ? 1f : -1f;
                float xOffset = 40f + Random.Range(0f, 20f);
                float zPos = Random.Range(-rx * 2f, rx * 2f);
                treePos = new Vector3(side * xOffset, 0, zPos);
            }
            else
            {
                // Oval: place outside track perimeter
                float angle = (float)i / 40 * Mathf.PI * 2;
                float outerX = Mathf.Cos(angle) * (rx + 40f + Random.Range(0f, 20f));
                float outerZ = Mathf.Sin(angle) * (rz + 40f + Random.Range(0f, 20f));
                treePos = new Vector3(outerX, 0, outerZ);
            }

            CreateTree(treePos);
        }
    }

    void CreateTree(Vector3 pos)
    {
        var tree = new GameObject("Tree");
        tree.transform.position = pos;

        float height = 5f + Random.Range(0f, 4f);

        // Trunk
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, height * 0.5f, 0);
        trunk.transform.localScale = new Vector3(0.4f, height * 0.5f, 0.4f);
        var trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.35f, 0.22f, 0.1f);
        trunk.GetComponent<Renderer>().material = trunkMat;
        Destroy(trunk.GetComponent<Collider>());

        // Canopy
        var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.transform.parent = tree.transform;
        canopy.transform.localPosition = new Vector3(0, height + 1.5f, 0);
        canopy.transform.localScale = new Vector3(4f, 3.5f, 4f);
        var canopyMat = new Material(Shader.Find("Standard"));
        canopyMat.color = new Color(0.15f + Random.Range(0f, 0.1f), 0.4f + Random.Range(0f, 0.15f), 0.1f);
        canopy.GetComponent<Renderer>().material = canopyMat;
        Destroy(canopy.GetComponent<Collider>());
    }
}
