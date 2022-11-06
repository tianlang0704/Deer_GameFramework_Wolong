// ================================================
//描 述 :  
//作 者 : 杜鑫 
//创建时间 : 2022-03-09 19-28-53  
//修改作者 : 杜鑫 
//修改时间 : 2022-03-09 19-28-53  
//版 本 : 0.1 
// ===============================================
using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotfixFramework.Runtime
{
    public delegate void LoadAssetObjectComplete(bool success, object assetObj, int nLoadSerial);

    [DisallowMultipleComponent]
    [AddComponentMenu("Deer/AssetPrefab")]
    public class AssetObjectComponent : GameFrameworkComponent
    {
        private IObjectPool<AssetInstanceObject> m_InstancePool; //AssetObject资源池   
        private Dictionary<int, object> m_AssetObjectToLoad;
        private HashSet<object> m_AssetObjects;
        [SerializeField]
        private float m_InstanceAutoReleaseInterval = 5f;

        [SerializeField]
        private int m_InstanceCapacity = 16;

        [SerializeField]
        private float m_InstanceExpireTime = 60f;

        [SerializeField]
        private int m_InstancePriority = 0;

        public float InstanceAutoReleaseInterval
        {
            get
            {
                return m_InstancePool.AutoReleaseInterval;
            }
            set
            {
                m_InstancePool.AutoReleaseInterval = value;
            }
        }
        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get
            {
                return m_InstancePool.Capacity;
            }
            set
            {
                m_InstancePool.Capacity = value;
            }
        }
        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get
            {
                return m_InstancePool.ExpireTime;
            }
            set
            {
                m_InstancePool.ExpireTime = value;
            }
        }
        /// <summary>
        /// 获取或设置界面实例对象池的优先级。
        /// </summary>
        public int InstancePriority
        {
            get
            {
                return m_InstancePool.Priority;
            }
            set
            {
                m_InstancePool.Priority = value;
            }
        }
        protected override void Awake()
        {
            base.Awake();
            m_AssetObjectToLoad = new Dictionary<int, object>();
            m_AssetObjects = new HashSet<object>();
        }

        private void Start()
        {
            m_InstancePool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<AssetInstanceObject>("Asset Object Pool", 10, 16, 2, 0);
            m_InstancePool.Priority = m_InstancePriority;
            m_InstancePool.ExpireTime = m_InstanceExpireTime;
            m_InstancePool.Capacity = m_InstanceCapacity;
            m_InstancePool.AutoReleaseInterval = m_InstanceAutoReleaseInterval;
        }
        protected void OnDestroy()
        {
            m_InstancePool = null;
        }

        public async UniTask<T> LoadAssetAsync<T>(string strPath) where T : UnityEngine.Object
        {
            // 检查缓存
            AssetInstanceObject assetObject = m_InstancePool.Spawn(strPath);
            if (assetObject != null) {
                return assetObject.Target as T;
            }
            // 加载
            try
            {
                var asset = await GameEntryMain.Resource.LoadAsset<T>(strPath);
                // 加入缓存
                assetObject = AssetInstanceObject.Create(strPath, asset, Instantiate((UnityEngine.Object)asset));
                m_InstancePool.Register(assetObject, true);
                m_AssetObjects.Add(asset);
                return asset;
            }
            // 处理错误
            catch (Exception e)
            {
                string appendErrorMessage =
                    Utility.Text.Format("Load assetObject failure, asset name '{0}' , error message '{2}'.", strPath,
                        e.Message);
                Log.Error(appendErrorMessage);
                return null;
            }
        }

        public void HideObject(UnityEngine.Object asset) 
        {
            RecycleAsset(asset);
        }
        public void RecycleAsset(UnityEngine.Object asset) 
        {
            if (m_AssetObjects.Contains(asset))
            {
                GameObject tempObj = asset as GameObject;
                if (tempObj != null)
                {
                    tempObj.SetActive(false);
                }
                Unspawn(asset);
                m_AssetObjects.Remove(asset);
            }
        }
        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="asset"></param>
        private void Unspawn(object asset)
        {
            if (m_InstancePool == null)
            {
                Log.Error("AssetObjectComponent Unspwn m_InstancePool null");
                return;
            }
            m_InstancePool.Unspawn(asset);
        }
    }
}