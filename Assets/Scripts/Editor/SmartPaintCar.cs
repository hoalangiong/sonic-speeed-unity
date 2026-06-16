using UnityEngine;
using UnityEditor;

/// <summary>
/// Smart paint — assigns correct colors based on mesh/material names.
/// Body = yellow metallic, glass = transparent dark, wheels = black, etc.
/// Tools → Sonic Speeed → Smart Paint Car
/// </summary>
public class SmartPaintCar : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Sonic Speeed/Smart Paint Car")]
    public static void SmartPaint()
    {
        var car = GameObject.Find("PlayerCar");
        if (car == null)
        {
            Debug.LogError("PlayerCar not found!");
            return;
        }

        var renderers = car.GetComponentsInChildren<Renderer>();
        int count = 0;

        foreach (var renderer in renderers)
        {
            string objName = renderer.gameObject.name.ToLower();
            var materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                string matName = materials[i] != null ? materials[i].name.ToLower() : "";
                string combined = objName + " " + matName;

                Material newMat = new Material(Shader.Find("Standard"));

                if (combined.Contains("glass") || combined.Contains("window") || combined.Contains("windshield"))
                {
                    // Glass — transparent dark
                    newMat.color = new Color(0.05f, 0.05f, 0.1f, 0.3f);
                    newMat.SetFloat("_Mode", 3);
                    newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    newMat.SetInt("_ZWrite", 0);
                    newMat.EnableKeyword("_ALPHABLEND_ON");
                    newMat.renderQueue = 3000;
                    newMat.SetFloat("_Metallic", 0.9f);
                    newMat.SetFloat("_Glossiness", 0.95f);
                }
                else if (combined.Contains("wheel") || combined.Contains("tire") || combined.Contains("tyre") || combined.Contains("rim"))
                {
                    // Wheels — dark with slight metallic
                    newMat.color = new Color(0.08f, 0.08f, 0.08f);
                    newMat.SetFloat("_Metallic", 0.6f);
                    newMat.SetFloat("_Glossiness", 0.4f);
                }
                else if (combined.Contains("calliper") || combined.Contains("brake"))
                {
                    // Brake callipers — red
                    newMat.color = new Color(0.8f, 0.1f, 0.05f);
                    newMat.SetFloat("_Metallic", 0.7f);
                    newMat.SetFloat("_Glossiness", 0.6f);
                }
                else if (combined.Contains("light") || combined.Contains("lamp") || combined.Contains("headlight"))
                {
                    // Lights — white emissive
                    newMat.color = Color.white;
                    newMat.EnableKeyword("_EMISSION");
                    newMat.SetColor("_EmissionColor", Color.white * 2f);
                    newMat.SetFloat("_Metallic", 0f);
                    newMat.SetFloat("_Glossiness", 0.95f);
                }
                else if (combined.Contains("interior") || combined.Contains("seat") || combined.Contains("dashboard"))
                {
                    // Interior — dark leather
                    newMat.color = new Color(0.05f, 0.05f, 0.05f);
                    newMat.SetFloat("_Metallic", 0.1f);
                    newMat.SetFloat("_Glossiness", 0.3f);
                }
                else if (combined.Contains("chrome") || combined.Contains("metal") || combined.Contains("exhaust"))
                {
                    // Chrome/metal parts
                    newMat.color = new Color(0.8f, 0.8f, 0.8f);
                    newMat.SetFloat("_Metallic", 0.95f);
                    newMat.SetFloat("_Glossiness", 0.9f);
                }
                else if (combined.Contains("carbon"))
                {
                    // Carbon fiber — dark glossy
                    newMat.color = new Color(0.1f, 0.1f, 0.1f);
                    newMat.SetFloat("_Metallic", 0.3f);
                    newMat.SetFloat("_Glossiness", 0.85f);
                }
                else if (combined.Contains("grill") || combined.Contains("grille"))
                {
                    // Grille — dark
                    newMat.color = new Color(0.05f, 0.05f, 0.05f);
                    newMat.SetFloat("_Metallic", 0.4f);
                    newMat.SetFloat("_Glossiness", 0.3f);
                }
                else if (combined.Contains("badge") || combined.Contains("logo") || combined.Contains("emblem"))
                {
                    // Badge — gold
                    newMat.color = new Color(0.85f, 0.7f, 0.2f);
                    newMat.SetFloat("_Metallic", 0.9f);
                    newMat.SetFloat("_Glossiness", 0.85f);
                }
                else if (combined.Contains("engine"))
                {
                    // Engine — silver metallic
                    newMat.color = new Color(0.5f, 0.5f, 0.5f);
                    newMat.SetFloat("_Metallic", 0.8f);
                    newMat.SetFloat("_Glossiness", 0.5f);
                }
                else
                {
                    // Default: car body paint — YELLOW LAMBORGHINI
                    newMat.color = new Color(1f, 0.8f, 0f);
                    newMat.SetFloat("_Metallic", 0.85f);
                    newMat.SetFloat("_Glossiness", 0.92f);
                }

                newMat.name = $"Lambo_{count}";
                materials[i] = newMat;
                count++;
            }
            renderer.sharedMaterials = materials;
        }

        Debug.Log($"✅ Smart painted {count} materials. Car should look like a Lamborghini now!");
    }
#endif
}
