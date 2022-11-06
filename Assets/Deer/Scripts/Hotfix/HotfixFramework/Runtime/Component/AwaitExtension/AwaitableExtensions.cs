using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Await
{
    public static partial class AwaitableExtensions
    {
        private static readonly Dictionary<int, TaskCompletionSource<UIForm>> s_UIFormTcs =
            new Dictionary<int, TaskCompletionSource<UIForm>>();

        private static readonly Dictionary<int, TaskCompletionSource<Entity>> s_EntityTcs =
            new Dictionary<int, TaskCompletionSource<Entity>>();

        private static readonly Dictionary<string, TaskCompletionSource<bool>> s_DataTableTcs =
            new Dictionary<string, TaskCompletionSource<bool>>();

        private static readonly Dictionary<string, TaskCompletionSource<bool>> s_SceneTcs =
            new Dictionary<string, TaskCompletionSource<bool>>();

        private static readonly HashSet<int> s_WebSerialIDs = new HashSet<int>();
        private static readonly List<WebResult> s_DelayReleaseWebResult = new List<WebResult>();

        private static readonly HashSet<int> s_DownloadSerialIds = new HashSet<int>();
        private static readonly List<DownLoadResult> s_DelayReleaseDownloadResult = new List<DownLoadResult>();

#if UNITY_EDITOR
        private static bool s_IsSubscribeEvent = false;
#endif

        /// <summary>
        /// 注册需要的事件 (需再流程入口处调用 防止框架重启导致事件被取消问题)
        /// </summary>
        public static void SubscribeEvent()
        {
            EventComponent eventComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            eventComponent.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
#if UNITY_EDITOR
            s_IsSubscribeEvent = true;
#endif
        }

#if UNITY_EDITOR
        private static void TipsSubscribeEvent()
        {
            if (!s_IsSubscribeEvent)
            {
                throw new Exception("Use await/async extensions must to subscribe event!");
            }
        }
#endif

        /// <summary>
        /// 打开界面（可等待）
        /// </summary>
        public static Task<UIForm> OpenUIFormAsync(this UIComponent uiComponent,string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            int serialId = uiComponent.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
            var tcs = new TaskCompletionSource<UIForm>();
            s_UIFormTcs.Add(serialId, tcs);
            return tcs.Task;
        }

        private static void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            s_UIFormTcs.TryGetValue(ne.UIForm.SerialId, out TaskCompletionSource<UIForm> tcs);
            if (tcs != null)
            {
                tcs.SetResult(ne.UIForm);
                s_UIFormTcs.Remove(ne.UIForm.SerialId);
            }
        }

        private static void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormFailureEventArgs ne = (OpenUIFormFailureEventArgs)e;
            s_UIFormTcs.TryGetValue(ne.SerialId, out TaskCompletionSource<UIForm> tcs);
            if (tcs != null)
            {
                tcs.SetException(new GameFrameworkException(ne.ErrorMessage));
                s_UIFormTcs.Remove(ne.SerialId);
            }
        }

        /// <summary>
        /// 显示实体（可等待）
        /// </summary>
        public static Task<Entity> ShowEntityAsync(this EntityComponent entityComponent, int entityId,
            Type entityLogicType, string entityAssetName, string entityGroupName, int priority,object userData)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = new TaskCompletionSource<Entity>();
            s_EntityTcs.Add(entityId, tcs);
            entityComponent.ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, userData);
            return tcs.Task;
        }


        private static void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
/*            EntityData data = (EntityData)ne.UserData;
            s_EntityTcs.TryGetValue(data.Id, out var tcs);
            if (tcs != null)
            {
                tcs.SetResult(ne.Entity);
                s_EntityTcs.Remove(data.Id);
            }*/
        }

        private static void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            s_EntityTcs.TryGetValue(ne.EntityId, out var tcs);
            if (tcs != null)
            {
                tcs.SetException(new GameFrameworkException(ne.ErrorMessage));
                s_EntityTcs.Remove(ne.EntityId);
            }
        }


        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static Task<bool> LoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = new TaskCompletionSource<bool>();
            s_SceneTcs.Add(sceneAssetName, tcs);
            sceneComponent.LoadScene(sceneAssetName);
            return tcs.Task;
        }

        private static void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            s_SceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                tcs.SetResult(true);
                s_SceneTcs.Remove(ne.SceneAssetName);
            }
        }

        private static void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            s_SceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                tcs.SetException(new GameFrameworkException(ne.ErrorMessage));
                s_SceneTcs.Remove(ne.SceneAssetName);
            }
        }


        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static Task<WebResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent,
            string webRequestUri, WWWForm wwwForm = null, object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tsc = new TaskCompletionSource<WebResult>();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, wwwForm,
                AwaitDataWrap<WebResult>.Create(userdata, tsc));
            s_WebSerialIDs.Add(serialId);
            return tsc.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static Task<WebResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent,
            string webRequestUri, byte[] postData, object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tsc = new TaskCompletionSource<WebResult>();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, postData,
                AwaitDataWrap<WebResult>.Create(userdata, tsc));
            s_WebSerialIDs.Add(serialId);
            return tsc.Task;
        }

        private static void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (s_WebSerialIDs.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<WebResult> webRequestUserdata)
                {
                    WebResult result = WebResult.Create(ne.GetWebResponseBytes(), false, string.Empty,
                        webRequestUserdata.UserData);
                    s_DelayReleaseWebResult.Add(result);
                    webRequestUserdata.Source.TrySetResult(result);
                    ReferencePool.Release(webRequestUserdata);
                }

                s_WebSerialIDs.Remove(ne.SerialId);
                if (s_WebSerialIDs.Count == 0)
                {
                    for (int i = 0; i < s_DelayReleaseWebResult.Count; i++)
                    {
                        ReferencePool.Release(s_DelayReleaseWebResult[i]);
                    }

                    s_DelayReleaseWebResult.Clear();
                }
            }
        }

        private static void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
            if (s_WebSerialIDs.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<WebResult> webRequestUserdata)
                {
                    WebResult result = WebResult.Create(null, true, ne.ErrorMessage, webRequestUserdata.UserData);
                    webRequestUserdata.Source.TrySetResult(result);
                    s_DelayReleaseWebResult.Add(result);
                    ReferencePool.Release(webRequestUserdata);
                }

                s_WebSerialIDs.Remove(ne.SerialId);
                if (s_WebSerialIDs.Count == 0)
                {
                    for (int i = 0; i < s_DelayReleaseWebResult.Count; i++)
                    {
                        ReferencePool.Release(s_DelayReleaseWebResult[i]);
                    }

                    s_DelayReleaseWebResult.Clear();
                }
            }
        }

        /// <summary>
        /// 增加下载任务（可等待)
        /// </summary>
        public static Task<DownLoadResult> AddDownloadAsync(this DownloadComponent downloadComponent,
            string downloadPath,
            string downloadUri,
            object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = new TaskCompletionSource<DownLoadResult>();
            int serialId = downloadComponent.AddDownload(downloadPath, downloadUri,
                AwaitDataWrap<DownLoadResult>.Create(userdata, tcs));
            s_DownloadSerialIds.Add(serialId);
            return tcs.Task;
        }

        private static void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;
            if (s_DownloadSerialIds.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<DownLoadResult> awaitDataWrap)
                {
                    DownLoadResult result = DownLoadResult.Create(false, string.Empty, awaitDataWrap.UserData);
                    s_DelayReleaseDownloadResult.Add(result);
                    awaitDataWrap.Source.TrySetResult(result);
                    ReferencePool.Release(awaitDataWrap);
                }

                s_DownloadSerialIds.Remove(ne.SerialId);
                if (s_DownloadSerialIds.Count == 0)
                {
                    for (int i = 0; i < s_DelayReleaseDownloadResult.Count; i++)
                    {
                        ReferencePool.Release(s_DelayReleaseDownloadResult[i]);
                    }

                    s_DelayReleaseDownloadResult.Clear();
                }
            }
        }

        private static void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = (DownloadFailureEventArgs)e;
            if (s_DownloadSerialIds.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<DownLoadResult> awaitDataWrap)
                {
                    DownLoadResult result = DownLoadResult.Create(true, ne.ErrorMessage, awaitDataWrap.UserData);
                    s_DelayReleaseDownloadResult.Add(result);
                    awaitDataWrap.Source.TrySetResult(result);
                    ReferencePool.Release(awaitDataWrap);
                }

                s_DownloadSerialIds.Remove(ne.SerialId);
                if (s_DownloadSerialIds.Count == 0)
                {
                    for (int i = 0; i < s_DelayReleaseDownloadResult.Count; i++)
                    {
                        ReferencePool.Release(s_DelayReleaseDownloadResult[i]);
                    }

                    s_DelayReleaseDownloadResult.Clear();
                }
            }
        }
    }
}