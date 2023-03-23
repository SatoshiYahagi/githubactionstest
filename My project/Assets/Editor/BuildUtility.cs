using UnityEditor;
using System;
using System.IO;
using System.Linq;
using UnityEditor.Build;

public static class BuildUtility
{
	public struct CommandLineArgs
	{
		public string apkPath;
		public string xcodePath;
		public string version;
		public string versionCode;
		public string buildNumber;
		public bool debugMode;
		public bool androidIl2cpp;
        public string envServer;
    }

    public static CommandLineArgs GetCommandLineArgs()
	{
		var ret = new CommandLineArgs();

		// パラメータ取得
		var args = System.Environment.GetCommandLineArgs();

		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "/apkPath":
					ret.apkPath = args[i + 1];
					break;
				case "/xcodePath":
					ret.xcodePath = args[i + 1];
					break;
				case "/version":
					ret.version = args[i + 1];
					break;
				case "/versionCode":
					ret.versionCode = args[i + 1];
					break;
				case "/buildNumber":
					ret.buildNumber = args[i + 1];
					break;
				case "/debugMode":
					ret.debugMode = int.Parse(args[i + 1]) == 1;
					break;
				case "/il2cpp":
					ret.androidIl2cpp = int.Parse(args[i + 1]) == 1;
					break;
                case "/envServer":
                    ret.envServer = args[i + 1];
                    break;
            }
        }

		return ret;
	}

	/// <summary>
	/// defineを設定する
	/// 上書き
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="symbols">Symbols.</param>
	public static void SetScriptingDefineSymbols(BuildTargetGroup target, params string[] symbols)
	{
		PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols));
	}

	/// <summary>
	/// defineを追加する
	/// 既に追加されているものは何もしない
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="symbols">Symbols.</param>
	public static void AppendScriptingDefineSymbols(BuildTargetGroup target, params string[] symbols)
	{
		var oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');
		var newSymbols = string.Join(";", oldSymbols.Concat(symbols).Distinct().ToArray());
		PlayerSettings.SetScriptingDefineSymbolsForGroup(target, newSymbols);
	}

	/// <summary>
	/// project reference > player > define を取得する
	/// </summary>
	/// <returns>The scripting define symbols.</returns>
	/// <param name="target">Target.</param>
	public static string[] GetScriptingDefineSymbols(BuildTargetGroup target)
	{
		return PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';');
	}

	/// <summary>
	/// ディレクトリが存在するかチェックし、存在しなければ作成する。
	/// </summary>
	public static string GetSafeDirectoryPath(string path)
	{
		var dirInfo = new DirectoryInfo(path);

		if (!dirInfo.Exists)
		{
			dirInfo.Create();
		}

		return path;
	}

	public static BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
	{
		switch (buildTarget)
		{
		case BuildTarget.iOS:
			return BuildTargetGroup.iOS;
		case BuildTarget.Android:
			return BuildTargetGroup.Android;
		default:
			throw new ArgumentException("Selected build target does not supported: " + buildTarget.ToString());
		}
	}

	public static NamedBuildTarget GetNamedBuildTarget(BuildTarget buildTarget)
	{
		switch (buildTarget)
		{
			case BuildTarget.iOS:
				return NamedBuildTarget.iOS;
			case BuildTarget.Android:
				return NamedBuildTarget.Android;
			default:
				throw new ArgumentException("Selected build target does not supported: " + buildTarget.ToString());
		}
	}
}

