

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
// #if UNITY_EDITOR
// using UnityEditor.
// #endif

namespace GameFramework.Resource
{
    public partial class AddressableManager
    {
        private static AddressableManager _instance = null;
        public static AddressableManager Instance => _instance ?? new AddressableManager();
        private Dictionary<string,SceneInstance> _sceneTracker = new();
        public async UniTask ForceUnloadUnusedAssetsAsync(bool performGCCollect)
        {
            await Resources.UnloadUnusedAssets();
            if (performGCCollect) {
                GC.Collect();
            }
        }
        
        public async UniTask<T> LoadAssetAsync<T>(string assetName) where T:class
        {
            if (string.IsNullOrEmpty(assetName)) 
            {
                throw new Exception("Asset name is invalid.");
            }
            if (!assetName.StartsWith("Assets/", StringComparison.Ordinal)) 
            {
                throw new Exception($"Asset name '{assetName}' is invalid.");
            }
            var handle = Addressables.LoadAssetAsync<T>(assetName);
            while (handle.IsValid() && !handle.IsDone) 
            {   
                await handle.Task;
            }
            if (!handle.IsValid()) 
            {
                throw new Exception("Handle is Invalid");
            }
            if (handle.Status == AsyncOperationStatus.Failed) 
            {
                throw new Exception("Operation failed", handle.OperationException); 
            }
            return handle.Result;
        }

        public async UniTask<SceneInstance> LoadSceneAsync(string assetName)
        {
            if (_sceneTracker.TryGetValue(assetName, out var existingScene))
            {
                await Addressables.UnloadSceneAsync(existingScene);
            }
            var handle = Addressables.LoadSceneAsync(assetName);
            var sceneInstance = await handle.Task;
            if (handle.OperationException != null) throw handle.OperationException;
            _sceneTracker[assetName] = sceneInstance;
            return sceneInstance;
        }

        public async UniTask UnloadSceneAsync(string assetName)
        {
            if (!_sceneTracker.TryGetValue(assetName, out var sceneInstance)) return;
            _sceneTracker.Remove(assetName);
            await Addressables.UnloadSceneAsync(sceneInstance);
        }

        public async UniTask<bool> HasAssetAsync(string assetName)
        {
            var handle = Addressables.LoadResourceLocationsAsync(assetName);
            try
            {
                var locationList = await handle.Task;
                if (handle.OperationException != null) throw handle.OperationException;
                return locationList.Count > 0;
            }
            finally
            {
                ReleaseHandle(handle);
            }
        }
        
        public void Release(object asset)
        {
            Addressables.Release(asset);
        }

        private void ReleaseHandle<T>(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
        }

        private void ReleaseHandle(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
        }

        public bool IsEditorMode()
        {
#if UNITY_EDITOR
            var settingsPath = PlayerPrefs.GetString(Addressables.kAddressablesRuntimeDataPath, "");
            return settingsPath.StartsWith("GUID:");
#endif
            return false;
        }
    }
}