using System;
using GameFramework;

/// <summary>
/// 资源路径相关
/// </summary>
public static partial class AssetUtility
{
    /// <summary>
    /// UI相关
    /// </summary>
    public static class UI
    {
        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Deer/AssetsHotfix/UI/UIForms/{0}/{1}.prefab", assetName.Replace("Form",""), assetName);
        }
        
        public static string GetNativeUIFormAsset(string assetName)
        {
            return Main.Runtime.AssetUtility.UI.GetUIFormAsset(assetName);
        }
        public static string GetUISubFormAsset(string assetName)
        {
            string[] args = assetName.Split('_');
            if (args is { Length: > 1 })
            {
                return Utility.Text.Format("Assets/Deer/AssetsHotfix/UI/UIForms/{0}/{1}.prefab", args[0], assetName);
            }
            Logger.Error("UISubForm prefab wrong name.It should be [UIxxx_xxxSubForm]");
            return string.Empty;
        }
        public static string GetUIComSubFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Deer/AssetsHotfix/UI/UIForms/UISub/{0}.prefab", assetName);
        }
        /// <summary>
        /// 获取精灵资源名称
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public static string GetSpritePath(string spriteName)
        {
            return $"Assets/Deer/UISprites/{spriteName}.png";
        }
        /// <summary>
        /// 获取精灵资源收集器
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public static string GetSpriteCollectionPath(string collectionName)
        {
            return $"Assets/Deer/AssetsHotfix/UI/UIArt/AtlasCollection/{collectionName}.asset";
        }

        /// <summary>
        /// 获取大图
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns></returns>
        public static string GetTexturePath(string textureName)
        {
            return string.Format("Assets/Deer/AssetsHotfix/UI/UIArt/Texture/{0}.png", textureName);
        }

        /// <summary>
        /// 获取大图
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public static string GetRenderTexturePath(string textureName)
        {
            return string.Format("Assets/Deer/AssetsHotfix/UI/UIArt/Texture/{0}.renderTexture", textureName);
        }
    }
}