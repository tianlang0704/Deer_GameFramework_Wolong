// ================================================
//描 述 :  
//作 者 : 杜鑫 
//创建时间 : 2021-07-09 08-18-03  
//修改作者 : 杜鑫 
//修改时间 : 2021-07-09 08-18-03  
//版 本 : 0.1 
// ===============================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bright.Serialization;
using cfg;
using Cysharp.Threading.Tasks;
using GameFramework.Resource;
using Main.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;

namespace Deer
{
    public delegate void LoadConfigCompleteCallback(bool result, string resultMessage = "");
    public delegate void LoadConfigUpdateCallback(int totalNum, int completeNum);
    public delegate void MoveConfigToReadWriteCallback(bool isComplete, int totalNum, int completeNum);

    public class ConfigManager:MonoBehaviour
    {
        /// <summary>
        /// 全部配置表文件
        /// </summary>
        private Dictionary<string, ConfigInfo> m_Configs;

        #region 读表逻辑
        public async UniTask<Tables> LoadAllUserConfig()
        {
            Tables tables = new Tables();
            await tables.LoadAsync(file => ConfigLoader(file));
            return tables;
        }

        private static async UniTask<ByteBuf> ConfigLoader(string file)
        {
            var filePath = Path.Combine($"Assets/Deer/AssetsHotfix/Luban/{DeerSettingsUtils.FrameworkGlobalSettings.ConfigFolderName}/", $"{file}.bytes");
            if (GameEntryMain.Base.EditorResourceMode)
            {
                filePath = Path.GetFullPath(filePath);
                if (!File.Exists(filePath))
                {
                    Logger.Error("filepath:" + filePath + " not exists");
                    return null;
                }
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                filePath = $"file://{filePath}";
#endif
                UnityWebRequest unityWebRequest = UnityWebRequest.Get(filePath);
                await unityWebRequest.SendWebRequest();
                return new ByteBuf(unityWebRequest.downloadHandler.data);
            }
            else
            {
                //单机包模式和热更模式 读取AB
                var asset = await AddressableManager.Instance.LoadAssetAsync<TextAsset>(filePath);
                return new ByteBuf(asset.bytes);
            }
        }
        #endregion
    }
}