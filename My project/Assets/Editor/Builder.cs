using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Builder
{
    // 共通設定
    // 開発用

    // iOS設定
    //const string IOS_SIGNING_TEAM_ID = "";
    //const string LOCAL_IOS_SIGNING_TEAM_ID = "";
    //const string PURCHASE_TEST_IOS_SIGNING_TEAM_ID = "";
    //const string REL_IOS_SIGNING_TEAM_ID = "";

    // Android設定
    //const string keystoreName = "";
    //const string keystorePass = "";
    //const string keyaliasName = "";
    //const string keyaliasPass = "";

    //static string[] GetEnabledScenes()
    //{
    //    return (
    //               from scene in EditorBuildSettings.scenes
    //               where scene.enabled
    //               where !string.IsNullOrEmpty(scene.path)
    //               select scene.path
    //           ).ToArray();
    //}

    [MenuItem("CI/Build Android")]
    private static void BuildAndroid()
    {
        // Setting for Android
        //EditorPrefs.SetBool("NdkUseEmbedded", true);
        //EditorPrefs.SetBool("SdkUseEmbedded", true);
        //EditorPrefs.SetBool("JdkUseEmbedded", true);
        //PlayerSettings.Android.keystoreName = keystoreName;
        //PlayerSettings.Android.keystorePass = keystorePass;
        //PlayerSettings.Android.keyaliasName = keyaliasName;
        //PlayerSettings.Android.keyaliasPass = keyaliasPass;

        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildUtility.GetBuildTargetGroup(BuildTarget.Android), BuildTarget.Android);

        SetAndroidIL2CPP(true);
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        // buildAppBundle を 明示的にfalseにする
        EditorUserBuildSettings.buildAppBundle = false;

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        //PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        //デバッグモード
        SetDefineSymbols(BuildTarget.Android);
        //接続環境を設定する
        SetEnvServerSymbols(BuildTarget.Android);
        //バージョン設定
        SetVersion();

        // Build
        bool result = Build(BuildTarget.Android);

        // Exit Editor
        EditorApplication.Exit(result ? 0 : 1);
    }

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

    //[MenuItem("CI/Build Dedicated Linux Server")
    //private static bool BuildLinuxServer()
    //{
    //    // Setting for Dedicated Server
    //    //PlayerSettings.SetScriptingBackend(NamedBuildTarget.LinuxHeadlessSimulation, ScriptingImplementation.IL2CPP);
    //    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.LinuxHeadlessSimulation, BuildTarget.LinuxHeadlessSimulation);

    //    // Build
    //    bool result = Build(BuildTarget.StandaloneLinux64);
    //    //bool result = Build(BuildTarget.StandaloneLinux64);

    //    // Exit Editor
    //    EditorApplication.Exit(result ? 0 : 1);
    //}

    // ビルド実行の共通メソッド
    private static bool Build(BuildTarget buildTarget)
    {
        // 環境変数を取得
        //string outputPath = GetEnvVar("OUTPUT_PATH");               // Output path
        //string bundleId = GetEnvVar("BUNDLE_ID");                   // Bundle Identifier
        //string productName = GetEnvVar("PRODUCT_NAME");              // Product Name
        //string companyName = GetEnvVar("COMPANY_NAME");              // Company Name

        string outputPath = AddExpand(buildTarget);

        Debug.Log("[Builder] Build OUTPUT_PATH :" + outputPath);
        Debug.Log("[Builder] Build BUILD_SCENES :" + String.Join("", getScenes()));

        // Player Settings
        BuildOptions buildOptions;
        buildOptions = BuildOptions.Development | BuildOptions.CompressWithLz4;
        PlayerSettings.statusBarHidden = true;

        //if (!string.IsNullOrEmpty(companyName)) { PlayerSettings.companyName = companyName; }
        //if (!string.IsNullOrEmpty(productName)) { PlayerSettings.productName = productName; }
        //if (!string.IsNullOrEmpty(bundleId)) { PlayerSettings.applicationIdentifier = bundleId; }

        // Build
        var report = BuildPipeline.BuildPlayer(getScenes(), outputPath, buildTarget, buildOptions);
        var summary = report.summary;

        // Build Report
        for (int i = 0; i < report.steps.Length; ++i)
        {
            var step = report.steps[i];
            Debug.Log($"{step.name} Depth:{step.depth} Duration:{step.duration}");

            for (int d = 0; d < step.messages.Length; ++d)
            {
                Debug.Log($"{step.messages[d].content}");
            }
        }

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("<color=white>[Builder] Build Success : " + outputPath + "</color>");
            return true;
        }
        else
        {
            Debug.Assert(false, "[Builder] Build Error : " + report.name);
            return false;
        }
    }

    private static string GetEnvVar(string pKey)
    {
        return Environment.GetEnvironmentVariable(pKey);
    }

    /// <summary>
    /// シーン一覧を取得
    /// </summary>
    private static string[] getScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }
        string[] ret = new string[scenes.Count];
        scenes.CopyTo(ret);
        return ret;
    }

    /// <summary>
    /// 出力ファイル名を取得
    /// </summary>
    private static string AddExpand(BuildTarget buildTarget)
    {
        // プロジェクト名から出力ファイル名を取得
        // プロダクト名を取得（実装箇所_年月日(YYYYMMDD形式)_(番号_)プラットフォーム の形式を想定）
        string productName = Application.productName;
        Debug.Log("[Builder] ProductName : " + productName);

        // プラットフォーム部分の前までのファイル名を作成（年月日_番号(24時間時)_実装箇所_プラットフォーム の形式を想定）
        //Match match = Regex.Match(productName, "^(.+)_[0-9]{8}_");
        Regex regex = new Regex("^(.+)_[0-9]{8}_");
        Match match = regex.Match(productName);
        string packagingPart = match.Groups[1].Value;

        Debug.Log("[Builder] AddExpand : " + match.Value);
        foreach (Group group in match.Groups)
            Debug.Log("[Builder] AddExpand : " + group.Value);

        string outputPath = packagingPart + "_" +  DateTime.Now.ToString("yyyyMMdd_HH") + "_";
        switch (buildTarget)
        {
            case BuildTarget.Android:
                outputPath += "An.apk";
                break;
        }

        return outputPath;
    }

    /// <summary>
    /// AndroidでIL2CPPを使うかどうかセットする
    /// </summary>
    static void SetAndroidIL2CPP(bool il2cpp)
    {
        // 参考 : https://qiita.com/KazuyaSeto/items/61754d82dc7121511d40
        var implementation = il2cpp ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, implementation);
        //PlayerSettings.SetPropertyInt("ScriptingBackend", (int)implementation, BuildTarget.Android);

        // ARM64
        //var architectures = il2cpp ? AndroidArchitecture.All : AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
        // この問題はターゲットアーキテクチャーにx86をチェックしている場合に発生
        // 2021/09 unity2020でx86指定がなくなった
        var architectures = il2cpp ? (AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7) : (AndroidArchitecture.ARMv7);
        PlayerSettings.Android.targetArchitectures = architectures;

        Debug.Log("ScriptingBackend is " + implementation.ToString());
    }

    // TODO:以下、既存プロジェクトのままなので、必要であれば編集
    /// <summary>
    /// Sets the version.
    /// </summary>
    static void SetVersion()
    {
        // ビルド時のパラメータを取得
        var args = BuildUtility.GetCommandLineArgs();
        if (!string.IsNullOrEmpty(args.version))
        {
            PlayerSettings.bundleVersion = args.version;
        }

        if (!string.IsNullOrEmpty(args.versionCode))
        {
            PlayerSettings.iOS.buildNumber = args.versionCode;

            int bundleVersionCode = default(int);
            if (int.TryParse(args.versionCode, out bundleVersionCode))
            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            }
        }

    }

    /// <summary>
    /// Sets the define symbols.
    /// </summary>
    /// <param name="target">Target.</param>
    static void SetDefineSymbols(BuildTarget target)
    {
        var groupTarget = BuildUtility.GetBuildTargetGroup(target);

        var arg = BuildUtility.GetCommandLineArgs();

        // 現在定義されているものを読み込む
        string[] symbols = BuildUtility.GetScriptingDefineSymbols(groupTarget);
        string newsymbols = "";
        bool isDebug = false;
        // DEBUG_MODEがない場合がある
        foreach (string symbol in symbols)
        {
            if (symbol.Equals("DEBUG_MODE"))
            {
                isDebug = true;
                break;
            }
        }
        foreach (string symbol in symbols)
        {
            // debugオンなら全部付与
            if (arg.debugMode)
            {
                newsymbols += symbol + ";";
            }
            else
            {
                // debugオフなら定義以外を付与
                if (!symbol.Equals("DEBUG_MODE"))
                {
                    newsymbols += symbol + ";";
                }
            }
        }
        // DEBUG_MODEがなくて、デバッグONにしたい場合
        if (!isDebug && arg.debugMode)
        {
            newsymbols += "DEBUG_MODE";
        }
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(groupTarget, newsymbols);
        PlayerSettings.SetScriptingDefineSymbols(BuildUtility.GetNamedBuildTarget(target), newsymbols);

    }

    /// <summary>
    /// 接続環境を設定する
    /// </summary>
    /// <param name="target">Target.</param>
    static void SetEnvServerSymbols(BuildTarget target)
    {
        var groupTarget = BuildUtility.GetBuildTargetGroup(target);

        var arg = BuildUtility.GetCommandLineArgs();

        // 現在定義されているものを読み込む
        string[] symbols = BuildUtility.GetScriptingDefineSymbols(groupTarget);
        string newsymbols = "";
        string envName = "ENV_DEV";
        // 接続環境設定
        if (!string.IsNullOrEmpty(arg.envServer))
        {
            if (arg.envServer.Equals("1"))
            {
                envName = "ENV_STG";
            }
            else if (arg.envServer.Equals("2"))
            {
                envName = "ENV_APP";
            }
            else if (arg.envServer.Equals("3"))
            {
                envName = "ENV_REVIEW";
            }
        }
        // 置換されたかチェック
        bool isReplace = false;
        foreach (string symbol in symbols)
        {
            // 通常のものはそのまま
            if (!symbol.Equals("ENV_DEV"))
            {
                newsymbols += symbol + ";";
            }
            // 接続環境を置換
            else if (symbol.Equals("ENV_DEV"))
            {
                isReplace = true;
                newsymbols += envName + ";";
            }

        }

        // 置換されなかったということはdefineが何もなかった
        if (!isReplace)
        {
            newsymbols += envName + ";";
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(groupTarget, newsymbols);

    }
}