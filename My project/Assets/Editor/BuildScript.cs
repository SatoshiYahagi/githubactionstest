using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;public class MobileBuild{    static string[] GetEnabledScenes()    {        return (                   from scene in EditorBuildSettings.scenes                   where scene.enabled                   where !string.IsNullOrEmpty(scene.path)                   select scene.path               ).ToArray();    }    [MenuItem("CI/Build Android")]    private static void BuildAndroid()    {
        // Setting for Android
        EditorPrefs.SetBool("NdkUseEmbedded", true);        EditorPrefs.SetBool("SdkUseEmbedded", true);        EditorPrefs.SetBool("JdkUseEmbedded", true);        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        // Build
        bool result = Build(BuildTarget.Android);

        // Exit Editor
        EditorApplication.Exit(result ? 0 : 1);    }

    //[MenuItem("CI/Build IOS")]
    //private static void BuildIOS()
    //{
    //    // Setting for iOS
    //    PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
    //    EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Debug;

    //    // Build
    //    bool result = Build(BuildTarget.iOS);

    //    // Exit Editor
    //    EditorApplication.Exit(result ? 0 : 1);
    //}

    [MenuItem("CI/Build Dedicated Linux Server")
    private static bool BuildLinuxServer()
    {
        // Setting for Dedicated Server
        //PlayerSettings.SetScriptingBackend(NamedBuildTarget.LinuxHeadlessSimulation, ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.LinuxHeadlessSimulation, BuildTarget.LinuxHeadlessSimulation);

        // Build
        bool result = Build(BuildTarget.Linux);
        //bool result = Build(BuildTarget.StandaloneLinux64);

        // Exit Editor
        EditorApplication.Exit(result ? 0 : 1);
    }    private static bool Build(BuildTarget buildTarget)    {
        // TODO:OutputPathを日時ベースで設定 プロジェクト名から取得も検討
        // Get Env
        string outputPath = GetEnvVar("OUTPUT_PATH");               // Output path
        string bundleId = GetEnvVar("BUNDLE_ID");                   // Bundle Identifier
        string productName = GetEnvVar("PRODUCT_NAME");              // Product Name
        string companyName = GetEnvVar("COMPANY_NAME");              // Company Name

        outputPath = AddExpand(buildTarget, outputPath);


        Debug.Log("[MobileBuild] Build OUTPUT_PATH :" + outputPath);        Debug.Log("[MobileBuild] Build BUILD_SCENES :" + String.Join("", GetEnabledScenes()));

        // Player Settings
        BuildOptions buildOptions;        buildOptions = BuildOptions.Development | BuildOptions.CompressWithLz4;        if (!string.IsNullOrEmpty(companyName)) { PlayerSettings.companyName = companyName; }        if (!string.IsNullOrEmpty(productName)) { PlayerSettings.productName = productName; }        if (!string.IsNullOrEmpty(bundleId)) { PlayerSettings.applicationIdentifier = bundleId; }

        // Build
        var report = BuildPipeline.BuildPlayer(GetEnabledScenes(), outputPath, buildTarget, buildOptions);        var summary = report.summary;

        // Build Report
        for (int i = 0; i < report.steps.Length; ++i)        {            var step = report.steps[i];            Debug.Log($"{step.name} Depth:{step.depth} Duration:{step.duration}");            for (int d = 0; d < step.messages.Length; ++d)            {                Debug.Log($"{step.messages[d].content}");            }        }        if (summary.result == BuildResult.Succeeded)        {            Debug.Log("<color=white>[MobileBuild] Build Success : " + outputPath + "</color>");            return true;        }        else        {            Debug.Assert(false, "[MobileBuild] Build Error : " + report.name);            return false;        }    }    private static string GetEnvVar(string pKey)    {        return Environment.GetEnvironmentVariable(pKey);    }    private static string AddExpand(BuildTarget buildTarget, string outputPath)    {        switch (buildTarget)        {            case BuildTarget.Android:                outputPath += ".apk";                break;        }        return outputPath;    }}