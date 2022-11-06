

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace GameFramework.Resource
{
    public partial class AddressableManager
    {
        public struct Label
        {
            public static readonly string Default = "default";
            public static readonly string HighPriority = "high_prio";
        }

        public async UniTask<List<string>> CheckForCatalogUpdatesAsync()
        {
            return await RunHandle(Addressables.CheckForCatalogUpdates());
        }
        
        public async UniTask<List<IResourceLocator>> UpdateCatalogsAsync(List<string> catalogs = null)
        {
            return await RunHandle(Addressables.UpdateCatalogs(catalogs));
        }

        public async UniTask<long> GetDownloadSizeAsync()
        {
            return await RunHandle(Addressables.GetDownloadSizeAsync(Label.Default), true);
        }
        
        public async UniTask<long> GetHighPriorityDownloadSizeAsync()
        {
            var hasAsset = await HasAssetAsync(Label.HighPriority);
            if (!hasAsset) return 0;
            return await RunHandle(Addressables.GetDownloadSizeAsync(Label.HighPriority), true);
        }

        public IUniTaskAsyncEnumerable<float> UpdateResourceAsync()
        {
            var handle = Addressables.DownloadDependenciesAsync(Label.Default);
            return UniTaskAsyncEnumerable.Create<float>(async (writer, token) =>
            {
                while (!token.IsCancellationRequested && !handle.IsDone)
                {
                    await writer.YieldAsync(handle.PercentComplete); // instead of `yield return`
                    await UniTask.Yield();
                }
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    throw handle.OperationException;
                }
            });
        }

        public async UniTask UpdateHighPriorityResourceAsync()
        {
            var hasAsset = await HasAssetAsync(Label.HighPriority);
            if (!hasAsset) return;
            await RunHandle(Addressables.DownloadDependenciesAsync(Label.HighPriority));
        }

        private async UniTask<T> RunHandle<T>(AsyncOperationHandle<T> handle, bool releaseHandle = false)
        {
            await handle.Task;
            try
            {
                if (handle.OperationException != null)
                {
                    throw handle.OperationException;
                }
                return handle.Result;
            }
            finally
            {
                if (releaseHandle)
                    ReleaseHandle(handle);
            }
        }

        private async UniTask RunHandle(AsyncOperationHandle handle, bool releaseHandle = false)
        {
            await handle.Task;
            try
            {
                if (handle.OperationException != null)
                {
                    throw handle.OperationException;
                }
            }
            finally
            {
                if (releaseHandle)
                    ReleaseHandle(handle);
            }
        }
    }
}