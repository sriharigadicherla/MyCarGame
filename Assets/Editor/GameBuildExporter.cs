using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Exports standalone game builds:
/// 1. Windows Standalone Executable (.exe)
/// 2. Android Standalone Package (.apk) for Mobile Phones
/// </summary>
public class GameBuildExporter
{
    [MenuItem("Tools/Build Standalone Windows Game (.exe)")]
    public static void BuildWindowsExecutable()
    {
        string buildDir = "Builds/Windows";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

        string exePath = $"{buildDir}/MyCarGame.exe";

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }

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

            string zipPath = "Builds/MyCarGame_Windows.zip";
            if (File.Exists(zipPath)) File.Delete(zipPath);
            System.IO.Compression.ZipFile.CreateFromDirectory(buildDir, zipPath);
            Debug.Log($"<color=green>[GameBuildExporter]</color> SUCCESS! Windows Zip archive created at: {zipPath}");
        }
        else
        {
            Debug.LogError($"<color=red>[GameBuildExporter]</color> Build FAILED: {report.summary.result}");
        }
    }

    [MenuItem("Tools/Build Android Game (.apk)")]
    public static void BuildAndroidApk()
    {
        string buildDir = "Builds/Android";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

        string apkPath = $"{buildDir}/MyCarGame.apk";

        // 1. Switch Active Build Target to Android so class layouts match
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("<color=yellow>[GameBuildExporter]</color> Switching active build target to Android...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        // 2. Setup PlayerSettings for Android
        PlayerSettings.companyName = "SrihariGames";
        PlayerSettings.productName = "MyCarGame";
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.srihari.mycargame");

        // Orientation: Landscape
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        // Minimum Android API: Android 7.0 (API 24)
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // Target ARM64 architecture (Standard for all modern Android phones)
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Scripting Backend: IL2CPP for ARM64 support
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        // Export directly as standalone .apk (not .aab and not Gradle project folder)
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new string[] { "Assets/Scenes/MainScene.unity" };
        options.locationPathName = apkPath;
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        Debug.Log("<color=cyan>[GameBuildExporter]</color> Starting Android APK build generation...");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            float sizeMb = report.summary.totalSize / (1024f * 1024f);
            Debug.Log($"<color=green>[GameBuildExporter]</color> SUCCESS! Android APK built at: {apkPath} (Size: {sizeMb:F1} MB)");
        }
        else
        {
            Debug.LogError($"<color=red>[GameBuildExporter]</color> Android Build FAILED: {report.summary.result}");
        }
    }
}
