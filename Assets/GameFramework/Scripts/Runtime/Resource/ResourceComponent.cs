//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Download;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Assertions.Must;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public sealed class ResourceComponent : GameFrameworkComponent
    {
        public string ApplicableGameVersion => "0";
        public string InternalResourceVersion => "0";

        public string ReadWritePath;

        private void Start()
        {
            ReadWritePath = Application.persistentDataPath;
        }

        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            AddressableManager.Instance.ForceUnloadUnusedAssetsAsync(performGCCollect).Forget();
        }
        
        /// <summary>
        /// 检查资源列表
        /// </summary>
        public async UniTask<List<string>> CheckCatalogAsync()
        {
            return await AddressableManager.Instance.CheckForCatalogUpdatesAsync();
        }

        /// <summary>
        /// 使用可更新模式并更新所有资源。
        /// </summary>
        /// <param name="catalogs">需要更新的目录。</param>
        public async UniTask<List<IResourceLocator>> UpdateCatalogsAsync(List<string> catalogs = null)
        {
            return await AddressableManager.Instance.UpdateCatalogsAsync(catalogs);
        }

        public async UniTask<long> GetDownloadSizeAsync()
        {
            return await AddressableManager.Instance.GetDownloadSizeAsync();
        }

        public async UniTask<long> GetHighPriorityDownloadSizeAsync()
        {
            return await AddressableManager.Instance.GetHighPriorityDownloadSizeAsync();
        }

        public IUniTaskAsyncEnumerable<float> UpdateResourceAsync()
        {
            return AddressableManager.Instance.UpdateResourceAsync();
        }

        public async UniTask UpdateHighPriorityResourceAsync()
        {
            await AddressableManager.Instance.UpdateHighPriorityResourceAsync();
        }
        
        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public bool HasAsset(string assetName)
        {
            var task = HasAssetAsync(assetName).AsTask();
            task.Wait();
            return task.Result;
        }
        public async UniTask<bool> HasAssetAsync(string assetName)
        {
            return await AddressableManager.Instance.HasAssetAsync(assetName);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        // public T LoadAsset<T>(string assetName) where T:class
        // {
        //     var task = LoadAssetAsync<T>(assetName).AsTask();
        //     task.Wait();
        //     return task.Result;
        // }
        public UniTask<T> LoadAssetAsync<T>(string assetName) where T:class
        {
            return LoadAsset<T>(assetName);
        }
        public async UniTask<T> LoadAsset<T>(string assetName) where T:class
        {
            T res = null;
            try
            {
                res = await AddressableManager.Instance.LoadAssetAsync<T>(assetName);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return res;
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            AddressableManager.Instance.Release(asset);
        }
    }
}
