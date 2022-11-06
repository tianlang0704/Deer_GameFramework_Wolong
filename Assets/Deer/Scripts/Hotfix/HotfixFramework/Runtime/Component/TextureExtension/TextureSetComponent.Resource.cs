using System;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Texture
{
    public partial class TextureSetComponent
    {
        /// <summary>
        /// 资源组件
        /// </summary>
        private ResourceComponent m_ResourceComponent;

        private void InitializedResources()
        {
            m_ResourceComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();
        }
        /// <summary>
        /// 通过资源系统设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        public void SetTextureByResources(ISetTexture2dObject setTexture2dObject)
        {
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                var texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
                SetTexture(setTexture2dObject, texture);
            }
            else
            {
                // m_ResourceComponent.LoadAsset(setTexture2dObject.Texture2dFilePath, typeof(Texture2D),m_LoadAssetCallbacks,setTexture2dObject);
                UniTask.Void(async () =>
                {
                    try
                    {
                        var texture =
                            await m_ResourceComponent.LoadAsset<Texture2D>(setTexture2dObject.Texture2dFilePath);
                        if (texture != null)
                        {
                            m_TexturePool.Register(
                                TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture,
                                    TextureLoad.FromResource, m_ResourceComponent), true);
                            SetTexture(setTexture2dObject, texture);
                        }
                        else
                        {
                            Log.Error($"Load Texture2D failure asset type is {texture.GetType()}.");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Can not load Texture2D from '{1}' with error message '{2}'.", setTexture2dObject.Texture2dFilePath, e.Message);
                    }
                    
                });
            }
        }
    }
}