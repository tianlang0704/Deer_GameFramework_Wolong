

using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using Object = UnityEngine.Object;

namespace GameFramework.Resource
{
    public partial class AddressableManager : IResourceManager
    {
        public HasAssetResult HasAsset(string assetName)
        {
            var task = HasAssetAsync(assetName).AsTask();
            task.Wait();
            return task.Result ? HasAssetResult.AssetOnDisk : HasAssetResult.NotExist;
        }

        public void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData = null)
        {
            LoadAsset(assetName, loadAssetCallbacks, userData);
        }

        public void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData = null)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    var res = await LoadAssetAsync<object>(assetName);
                    loadAssetCallbacks.LoadAssetSuccessCallback(assetName, res, 0, userData);
                }
                catch (Exception e)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.AssetError, e.Message, userData);
                }
            });
        }

        public void UnloadAsset(object asset)
        {
            Release(asset);
        }

        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData = null)
        {
            LoadScene(sceneAssetName, loadSceneCallbacks, userData);
        }

        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData = null)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    var res = await LoadSceneAsync(sceneAssetName);
                    loadSceneCallbacks.LoadSceneSuccessCallback(sceneAssetName, 0, userData);
                }
                catch (Exception e)
                {
                    loadSceneCallbacks.LoadSceneFailureCallback(sceneAssetName, LoadResourceStatus.AssetError, e.Message, userData);
                }
            });
        }

        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData = null)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    await UnloadSceneAsync(sceneAssetName);
                    unloadSceneCallbacks.UnloadSceneSuccessCallback(sceneAssetName, userData);
                }
                catch (Exception e)
                {
                    unloadSceneCallbacks.UnloadSceneFailureCallback(sceneAssetName, userData);
                }
            });
            
        }

        public int GetBinaryLength(string binaryAssetName)
        {
            throw new NotImplementedException();
        }

        public void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData = null)
        {
            throw new NotImplementedException();
        }

        public int LoadBinaryFromFileSystem(string binaryAssetName, byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}