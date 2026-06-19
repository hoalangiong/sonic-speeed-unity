using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Tools/Sonic Speeed/Build APK")]
    public static void BuildAndroid()
    {
        // Set player settings
        PlayerSettings.companyName = "SonicSpeeed";
        PlayerSettings.productName = "Sonic Speeed";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.sonic.speeed");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        // Get scenes
        string[] scenes = { "Assets/Scene/MainScene.unity" };

        // Build
        string outputPath = "C:/Users/Dell Precision 5560/Downloads/SonicSpeeed.apk";
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = outputPath;
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✅ APK Build SUCCESS! File: {outputPath} ({report.summary.totalSize / 1024 / 1024} MB)");
        }
        else
        {
            Debug.LogError($"❌ Build FAILED: {report.summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error)
                        Debug.LogError(msg.content);
                }
            }
        }
    }
}
