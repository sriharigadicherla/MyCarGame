using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Exports a standalone Windows .exe game build that anyone can play
/// without needing Unity or Unity Hub installed.
/// </summary>
public class GameBuildExporter
{
    [MenuItem("Tools/Build Standalone Windows Game (.exe)")]
    public static void BuildWindowsExecutable()
    {
        string buildDir = "Builds/Windows";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

        string exePath = $"{buildDir}/MyCarGame.exe";

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new string[] { "Assets/Scenes/MainScene.unity" };
        options.locationPathName = exePath;
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.None;

        Debug.Log("<color=cyan>[GameBuildExporter]</color> Starting Windows standalone build generation...");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            float sizeMb = report.summary.totalSize / (1024f * 1024f);
            Debug.Log($"<color=green>[GameBuildExporter]</color> SUCCESS! Windows game built at: {exePath} (Size: {sizeMb:F1} MB)");
        }
        else
        {
            Debug.LogError($"<color=red>[GameBuildExporter]</color> Build FAILED: {report.summary.result}");
        }
    }
}
