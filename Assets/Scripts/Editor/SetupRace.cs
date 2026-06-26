using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Setup complete race — AI opponents, waypoints, race manager, HUD.
/// Tools → Sonic Speeed → Setup Race
/// </summary>
public class SetupRace : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Sonic Speeed/Setup Race")]
    public static void Setup()
    {
        var player = GameObject.Find("PlayerCar");
        if (player == null)
        {
            Debug.LogError("PlayerCar not found! Run 'Setup With Imported Model' first.");
            return;
        }

        // Create waypoints along the road
        var waypoints = CreateWaypoints();

        // Create AI racers
        var aiRacers = CreateAIRacers(waypoints, player.transform);

        // Setup Race Manager
        SetupRaceManager(player.transform, aiRacers, waypoints);

        // Setup HUD
        SetupHUD(player);

        Debug.Log("✅ Race setup complete! 3 AI opponents + checkpoints + HUD ready.");
    }

    static Transform[] CreateWaypoints()
    {
        // Remove old waypoints
        var oldWP = GameObject.Find("Waypoints");
        if (oldWP != null) DestroyImmediate(oldWP);

        var waypointParent = new GameObject("Waypoints");
        var points = new List<Transform>();

        // Generate waypoints using same GetTrackPosition logic
        int numWaypoints = 20;
        for (int i = 0; i < numWaypoints; i++)
        {
            float angle = (float)i / numWaypoints * Mathf.PI * 2;

            // Get track params
            float rx = 150f;
            float rz = 100f;
            if (TrackSelector.AvailableTracks != null && TrackSelector.SelectedTrackIndex < TrackSelector.AvailableTracks.Length)
            {
                rx = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex].trackScaleX;
                rz = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex].trackScaleZ;
            }

            float x, z;
            if (rz < 50f)
            {
                // Highway — straight, go and come back (lap = round trip)
                float halfWaypoints = numWaypoints / 2f;
                if (i < numWaypoints / 2)
                {
                    // Going forward
                    float t = (float)i / halfWaypoints;
                    z = t * rx * 3f - rx * 1.5f;
                    x = Mathf.Sin(t * Mathf.PI * 0.5f) * 3f;
                }
                else
                {
                    // Coming back (offset to other lane)
                    float t = (float)(i - numWaypoints / 2) / halfWaypoints;
                    z = rx * 1.5f - t * rx * 3f;
                    x = -3f + Mathf.Sin(t * Mathf.PI * 0.5f) * 3f;
                }
            }
            else
            {
                // Oval
                x = Mathf.Cos(angle) * rx;
                z = Mathf.Sin(angle) * rz;
            }

            var wp = new GameObject($"WP_{i}");
            wp.transform.parent = waypointParent.transform;
            wp.transform.position = new Vector3(x, 0.5f, z);
            points.Add(wp.transform);
        }

        return points.ToArray();
    }

    static AIRacer[] CreateAIRacers(Transform[] waypoints, Transform player)
    {
        // Remove old AI
        var oldAI = GameObject.FindObjectsByType<AIRacer>(FindObjectsSortMode.None);
        foreach (var ai in oldAI) DestroyImmediate(ai.gameObject);

        // Load car model
        var carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/lamborghini.fbx");

        Color[] colors = { Color.red, new Color(0, 0.5f, 1f), new Color(0, 0.8f, 0.2f) };
        float[] speeds = { 18f, 22f, 20f };
        string[] names = { "AI_Red", "AI_Blue", "AI_Green" };
        var racers = new List<AIRacer>();

        for (int i = 0; i < 3; i++)
        {
            GameObject aiCar;

            if (carPrefab != null)
            {
                // Use same model as player
                var aiParent = new GameObject(names[i]);
                aiParent.transform.position = player.position + new Vector3((i - 1) * 4f, 0, -(i + 1) * 8f);

                var visual = (GameObject)PrefabUtility.InstantiatePrefab(carPrefab);
                if (visual == null) visual = Instantiate(carPrefab);
                visual.name = "CarVisual";
                visual.transform.SetParent(aiParent.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.Euler(180, 0, 0);

                // Scale same as player
                var bounds = GetAIBounds(aiParent);
                float targetLength = 4.5f;
                float currentLength = Mathf.Max(bounds.size.z, bounds.size.x);
                if (currentLength > 0.01f)
                    visual.transform.localScale = Vector3.one * (targetLength / currentLength);

                // Paint AI car different color
                foreach (var renderer in aiParent.GetComponentsInChildren<Renderer>())
                {
                    var mats = renderer.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        var mat = new Material(Shader.Find("Standard"));
                        mat.color = colors[i];
                        mat.SetFloat("_Metallic", 0.85f);
                        mat.SetFloat("_Glossiness", 0.9f);
                        mats[m] = mat;
                    }
                    renderer.sharedMaterials = mats;
                }

                // Remove colliders from model
                foreach (var col in aiParent.GetComponentsInChildren<Collider>())
                    DestroyImmediate(col);

                aiCar = aiParent;
            }
            else
            {
                // Fallback cube
                aiCar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                aiCar.name = names[i];
                aiCar.transform.position = player.position + new Vector3((i - 1) * 3f, 0, -(i + 1) * 5f);
                aiCar.transform.localScale = new Vector3(1.8f, 0.6f, 4.2f);

                var mat = new Material(Shader.Find("Standard"));
                mat.color = colors[i];
                mat.SetFloat("_Metallic", 0.8f);
                mat.SetFloat("_Glossiness", 0.85f);
                aiCar.GetComponent<Renderer>().material = mat;
                DestroyImmediate(aiCar.GetComponent<Collider>());
            }

            // Add AI script
            var ai = aiCar.AddComponent<AIRacer>();
            ai.waypoints = waypoints;
            ai.baseSpeed = speeds[i];
            ai.speedVariation = 5f;
            ai.playerTransform = player;

            racers.Add(ai);
        }

        return racers.ToArray();
    }

    static Bounds GetAIBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static void SetupRaceManager(Transform player, AIRacer[] racers, Transform[] waypoints)
    {
        // Remove old
        var old = GameObject.Find("RaceManager");
        if (old != null) DestroyImmediate(old);

        var rmObj = new GameObject("RaceManager");
        var rm = rmObj.AddComponent<RaceManager>();
        rm.player = player;
        rm.aiRacers = racers;
        rm.checkpoints = waypoints;
        rm.totalLaps = 3;
    }

    static void SetupHUD(GameObject player)
    {
        // Remove old
        var old = GameObject.Find("RaceHUD");
        if (old != null) DestroyImmediate(old);
        var oldCountdown = GameObject.Find("RaceCountdown");
        if (oldCountdown != null) DestroyImmediate(oldCountdown);
        var oldMinimap = GameObject.Find("MinimapObj");
        if (oldMinimap != null) DestroyImmediate(oldMinimap);

        var vc = player.GetComponent<VehicleController>();
        var rm = GameObject.Find("RaceManager").GetComponent<RaceManager>();
        var nitroSys = player.GetComponent<NitroSystem>();
        var driftSys = player.GetComponent<DriftScoring>();
        var aiRacers = GameObject.FindObjectsByType<AIRacer>(FindObjectsSortMode.None);

        // HUD
        var hudObj = new GameObject("RaceHUD");
        var hud = hudObj.AddComponent<RaceHUD>();
        hud.player = vc;
        hud.raceManager = rm;
        hud.nitro = nitroSys;
        hud.drift = driftSys;

        // Countdown + Finish screen
        var countdownObj = new GameObject("RaceCountdown");
        var countdown = countdownObj.AddComponent<RaceCountdown>();
        countdown.player = vc;
        countdown.aiRacers = aiRacers;
        countdown.raceManager = rm;

        // Mobile touch controls
        var touch = player.GetComponent<MobileTouchControls>();
        if (touch == null) touch = player.AddComponent<MobileTouchControls>();
        touch.vehicle = vc;
        touch.nitro = nitroSys;

        // Tyre marks
        var tyreMarks = player.GetComponent<TyreMarks>();
        if (tyreMarks == null) tyreMarks = player.AddComponent<TyreMarks>();
        tyreMarks.vehicle = vc;

        // Minimap
        var minimapObj = new GameObject("MinimapObj");
        var minimap = minimapObj.AddComponent<Minimap>();
        minimap.player = player.transform;
        minimap.aiRacers = aiRacers;

        // Car Selector (shows before race)
        var oldSelector = GameObject.Find("CarSelectorUI");
        if (oldSelector != null) DestroyImmediate(oldSelector);
        var selectorObj = new GameObject("CarSelectorUI");
        selectorObj.AddComponent<CarSelector>();

        // Track Selector (shows after car select)
        var oldTrackSel = GameObject.Find("TrackSelectorUI");
        if (oldTrackSel != null) DestroyImmediate(oldTrackSel);
        var trackSelObj = new GameObject("TrackSelectorUI");
        trackSelObj.AddComponent<TrackSelector>();

        // Track visual applier (applies colors/lighting when track selected)
        var oldApplier = GameObject.Find("TrackApplier");
        if (oldApplier != null) DestroyImmediate(oldApplier);
        var applierObj = new GameObject("TrackApplier");
        applierObj.AddComponent<TrackVisualApplier>();

        // Coins on track
        var oldCoins = GameObject.Find("CoinSystem");
        if (oldCoins != null) DestroyImmediate(oldCoins);
        var coinObj = new GameObject("CoinSystem");
        coinObj.AddComponent<CoinSpawner>();

        // Power-ups on track
        var oldPU = GameObject.Find("PowerUpSystem");
        if (oldPU != null) DestroyImmediate(oldPU);
        var puObj = new GameObject("PowerUpSystem");
        puObj.AddComponent<PowerUpSystem>();

        // Star rating + celebration
        var oldStar = GameObject.Find("StarRating");
        if (oldStar != null) DestroyImmediate(oldStar);
        var starObj = new GameObject("StarRating");
        starObj.AddComponent<StarRating>();

        // Runtime tree spawner (places trees AFTER track is selected)
        var oldTrees = GameObject.Find("TreeSpawner");
        if (oldTrees != null) DestroyImmediate(oldTrees);
        var treeObj = new GameObject("TreeSpawner");
        treeObj.AddComponent<RuntimeTreeSpawner>();
    }
#endif
}
