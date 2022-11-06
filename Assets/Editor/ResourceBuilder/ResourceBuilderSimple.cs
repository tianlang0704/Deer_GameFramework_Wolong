using System.Diagnostics;
using System.IO;
using GameFramework.Resource;
using HybridCLR.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Player;
using UnityEngine;
using Debug = UnityEngine.Debug;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace UnityGameFramework.Editor.ResourceTools
{
    internal static class ResourceBuilderSimple
    {
        [MenuItem("MyTools/Resource Tools/ClearCache", false, 40)]
        public static void ClearCache()
        {
            var addPath = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
            if (Directory.Exists(addPath)) {
                Directory.Delete(addPath, true);
            }
            Caching.ClearCache();
        }
        
        [MenuItem("MyTools/Resource Tools/SimpleBuild", false, 40)]
        public static void Build()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            GenConfig();
            CompileDll(target);
            CopyDllBuildFiles(target);
            // AddressableAssetSettings.BuildPlayerContent(out var result);
        }

        private static void GenConfig()
        {
            var bat = Path.Combine(Application.dataPath, "..\\LubanTools\\DesignerConfigs\\BuildConfig_Wolong.bat");
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + bat);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            var process = Process.Start(processInfo);
            process?.WaitForExit();
            var exitCode = process?.ExitCode ?? -1;
            process?.Close();
            Debug.Log($"GenConfig exit code: {exitCode}");
        }
        
        private static string AssemblyTextAssetPath 
        {
            get { return Path.Combine(Application.dataPath, DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetPath); }    
        }
        
        private static void CompileDll(BuildTarget target)
        {
            CompileDll(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target), target);
        }
        
        private static void CompileDll(string buildDir, BuildTarget target)
        {
            var group = BuildPipeline.GetBuildTargetGroup(target);

            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = target;
            Directory.CreateDirectory(buildDir);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
            foreach (var ass in scriptCompilationResult.assemblies)
            {
                //Debug.LogFormat("compile assemblies:{1}/{0}", ass, buildDir);
            }
            Debug.Log("compile finish!!!");
        }
        
        // 为AOT Meta复制已经剔除过后的DLL(需要构建一次过后才能拷贝)
        private static void CopyDllBuildFiles(BuildTarget buildTarget) 
        {
            AOTMetaAssembliesHelper.FindAllAOTMetaAssemblies(buildTarget);
            FolderUtils.ClearFolder(AssemblyTextAssetPath);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFiles)
            {
                string dllPath = $"{SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget)}/{dll}";
                string dllBytesPath = $"{AssemblyTextAssetPath}/{dll}{DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetExtension}";
                if (!Directory.Exists(AssemblyTextAssetPath))
                {
                    Directory.CreateDirectory(AssemblyTextAssetPath);
                }
                File.Copy(dllPath, dllBytesPath, true);
            }
            foreach (var dll in DeerSettingsUtils.HybridCLRCustomGlobalSettings.AOTMetaAssemblies)
            {
                string dllPath = $"{SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget)}/{dll}";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }
                string dllBytesPath = $"{AssemblyTextAssetPath}/{dll}{DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetExtension}";
                if (!Directory.Exists(AssemblyTextAssetPath))
                {
                    Directory.CreateDirectory(AssemblyTextAssetPath);
                }
                File.Copy(dllPath, dllBytesPath, true);
            }
            DeerSettingsUtils.SetHybridCLRHotUpdateAssemblies(SettingsUtil.HotUpdateAssemblyFiles);
            AssetDatabase.Refresh();
        }
    }
}