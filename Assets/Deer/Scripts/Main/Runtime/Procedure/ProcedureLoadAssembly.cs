// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-06-09 00-55-01
//修改作者:杜鑫
//修改时间:2022-06-09 00-55-01
//版 本:0.1 
// ===============================================
using GameFramework;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
#if ENABLE_HYBRID_CLR_UNITY
using HybridCLR;
#endif
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

enum LoadImageErrorCode
{
    OK = 0,
    BAD_IMAGE, // dll 不合法
    NOT_IMPLEMENT, // 不支持的元数据特性
    AOT_ASSEMBLY_NOT_FIND, // 对应的AOT assembly未找到
    HOMOLOGOUS_ONLY_SUPPORT_AOT_ASSEMBLY, // 不能给解释器assembly补充元数据
    HOMOLOGOUS_ASSEMBLY_HAS_LOADED, // 已经补充过了，不能再次补充
};


namespace Main.Runtime.Procedure
{
    public class ProcedureLoadAssembly : ProcedureBase
    {
        private Assembly m_MainLogicAssembly;
        private List<Assembly> m_HotfixAssemblys;
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_HotfixAssemblys = new List<Assembly>();
            UniTask.Void( async () =>
            {
                await LoadAssemblies();
                AllAsmLoadComplete();
            });
        }
        
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntryMain.UI.CloseAllLoadingUIForms();
            GameEntryMain.UI.CloseUIWithUIGroup("Default");
        }

        private async UniTask LoadAssemblies()
        {
            if (!GameEntryMain.Base.EditorResourceMode && DeerSettingsUtils.HybridCLRCustomGlobalSettings.Enable)
            {
                await LoadFromHybridClr();
            }
            else
            {
                GetMainLogicAssembly();
            }
#if !UNITY_EDITOR
            if (DeerSettingsUtils.HybridCLRCustomGlobalSettings.Enable)
            {
                await LoadMetadataForAOTAssembly();
                await LoadDHEForAOTAssembly();
            }
#endif
        }

        private async UniTask LoadFromHybridClr()
        {
            var assetPathList = DeerSettingsUtils.HybridCLRCustomGlobalSettings.HotUpdateAssemblies
                .Select((s) =>
                {
                    var path = Utility.Path.GetRegularPath(
                        Path.Combine(
                            "Assets",
                            DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetPath,
                            $"{s}{DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetExtension}"
                            )
                        );
                    return (s, path);
                })
                .ToList();
            try
            {
                foreach (var sp in assetPathList)
                {
                    Log.Debug($"LoadAsset: [ {sp.path} ]");
                    var textAsset = await GameEntryMain.Resource.LoadAsset<TextAsset>(sp.path);
                    var assembly = Assembly.Load(textAsset.bytes);
                    m_HotfixAssemblys.Add((assembly));
                    if (String.CompareOrdinal(DeerSettingsUtils.HybridCLRCustomGlobalSettings.LogicMainDllName, sp.s) == 0) {
                        m_MainLogicAssembly = assembly;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e.Message);
            }
        }
        
                /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        public async UniTask LoadMetadataForAOTAssembly()
        {
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessor_xxx里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            var metaInfoList = DeerSettingsUtils.HybridCLRCustomGlobalSettings.AOTMetaAssemblies
                .Select((s) =>
                {
                    var path = Utility.Path.GetRegularPath(
                        Path.Combine(
                            "Assets",
                            DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetPath,
                            $"{s}{DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetExtension}"
                        )
                    );
                    
                    return (s, path);
                })
                .ToList();
            try
            {
                foreach ((string name, string path) d in metaInfoList)
                {
#if ENABLE_HYBRID_CLR_UNITY
                    string path = d.path;
                    Log.Debug($"LoadMetadataAsset: [ {path} ]");
                    var asset = await GameEntryMain.Resource.LoadAsset<TextAsset>(path);
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    LoadImageErrorCode err = (LoadImageErrorCode)HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(asset.bytes, mode); 
                    Debug.Log($"LoadMetadataForAOTAssembly:{(string)d.name}. mode:{mode} ret:{err}");
#endif

                }
            }
            catch (Exception e)
            {
                Log.Fatal(e.Message);
            }
        }

        public async UniTask LoadDHEForAOTAssembly()
        {
            // var dynamicInfoList = DeerSettingsUtils.HybridCLRCustomGlobalSettings
            //     .Select((s) =>
            //     {
            //         var path = Utility.Path.GetRegularPath(
            //             Path.Combine(
            //                 "Assets",
            //                 DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetPath,
            //                 $"{s}{DeerSettingsUtils.HybridCLRCustomGlobalSettings.AssemblyTextAssetExtension}"
            //             )
            //         );
            //         
            //         return (s, path);
            //     })
            //     .ToList();
            // HybridCLR.RuntimeApi.LoadDifferentialHybridAssembly();
        }


        private void GetMainLogicAssembly()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Compare(DeerSettingsUtils.HybridCLRCustomGlobalSettings.LogicMainDllName, $"{asm.GetName().Name}.dll",
                        StringComparison.Ordinal) == 0)
                {
                    m_MainLogicAssembly = asm;
                }
                foreach (var hotUpdateDllName in DeerSettingsUtils.HybridCLRCustomGlobalSettings.HotUpdateAssemblies)
                {
                    if (hotUpdateDllName == $"{asm.GetName().Name}.dll")
                    {
                        m_HotfixAssemblys.Add(asm);
                    }
                }
                if (m_MainLogicAssembly != null && m_HotfixAssemblys.Count == DeerSettingsUtils.HybridCLRCustomGlobalSettings.HotUpdateAssemblies.Count)
                {
                    break;
                }
            }
        }
        
        private void AllAsmLoadComplete()
        {
            if (null == m_MainLogicAssembly)
            {
                Log.Fatal("Main logic assembly missing.");
                return;
            }
            var appType = m_MainLogicAssembly.GetType("AppMain");
            if (null == appType)
            {
                Log.Fatal("Main logic type 'AppMain' missing.");
                return;
            }
            var entryMethod = appType.GetMethod("Entrance");
            if (null == entryMethod)
            {
                Log.Fatal("Main logic entry method 'Entrance' missing.");
                return;
            }
            object[] objects = new object[] { new object[] { m_HotfixAssemblys } };
            entryMethod.Invoke(appType, objects);
        }
    }
}