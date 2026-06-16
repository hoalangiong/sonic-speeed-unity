using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tyre marks on ground when drifting.
/// Creates thin dark quads on the road surface behind the car.
/// </summary>
public class TyreMarks : MonoBehaviour
{
    public VehicleController vehicle;

    [Header("Settings")]
    public float markWidth = 0.3f;
    public float minSpeed = 40f;
    public float minSteer = 0.4f;
    public int maxMarks = 200;
    public Material markMaterial;

    private List<GameObject> marks = new List<GameObject>();
    private float lastMarkTime;
    private Vector3 lastMarkPos;

    void Start()
    {
        if (markMaterial == null)
        {
            markMaterial = new Material(Shader.Find("Standard"));
            markMaterial.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            markMaterial.SetFloat("_Glossiness", 0.8f);
        }
    }

    void Update()
    {
        if (vehicle == null) return;

        bool shouldMark = vehicle.IsDrifting ||
            (Mathf.Abs(vehicle.inputSteer) > minSteer && vehicle.CurrentSpeed > minSpeed && vehicle.IsGrounded);

        if (shouldMark && Time.time - lastMarkTime > 0.05f)
        {
            Vector3 pos = transform.position;
            pos.y = 0.02f; // Just above road

            // Only create mark if moved enough
            if (Vector3.Distance(pos, lastMarkPos) > 0.5f)
            {
                CreateMark(pos);
                lastMarkTime = Time.time;
                lastMarkPos = pos;
            }
        }
    }

    void CreateMark(Vector3 position)
    {
        // Left tyre
        CreateSingleMark(position + transform.right * -0.7f);
        // Right tyre
        CreateSingleMark(position + transform.right * 0.7f);
    }

    void CreateSingleMark(Vector3 pos)
    {
        var mark = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mark.name = "TyreMark";
        mark.transform.position = pos;
        mark.transform.rotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);
        mark.transform.localScale = new Vector3(markWidth, 1.5f, 1);
        mark.GetComponent<Renderer>().material = markMaterial;
        Destroy(mark.GetComponent<Collider>());

        marks.Add(mark);

        // Remove oldest marks if too many
        if (marks.Count > maxMarks)
        {
            Destroy(marks[0]);
            marks.RemoveAt(0);
        }
    }
}
