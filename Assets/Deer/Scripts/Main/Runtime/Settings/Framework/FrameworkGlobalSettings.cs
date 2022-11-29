// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-09-16 14-15-39
//修改作者:杜鑫
//修改时间:2022-09-16 14-15-39
//版 本:0.1 
// ===============================================
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 资源存放地
/// </summary>
[Serializable]
public class ResourcesArea 
{
    [Tooltip("资源管理类型")]
    [SerializeField] private string m_ResAdminType = "Default";
    public string ResAdminType { get { return m_ResAdminType; } }
    [Tooltip("资源管理编号")]
    [SerializeField] private string m_ResAdminCode = "0";
    public string ResAdminCode { get { return m_ResAdminCode; } }
    [SerializeField] private ServerTypeEnum m_ServerType = ServerTypeEnum.Intranet;
    public ServerTypeEnum ServerType { get { return m_ServerType; } }
    [Tooltip("是否在构建资源的时候清理上传到服务端目录的老资源")]
    [SerializeField] private bool m_CleanCommitPathRes = true;
    public bool CleanCommitPathRes { get { return m_CleanCommitPathRes; } }
    [Tooltip("内网地址")]
    [SerializeField]
    private string m_InnerResourceSourceUrl = "http://121.4.195.168:8088";
    public string InnerResourceSourceUrl { get { return m_InnerResourceSourceUrl; } }

    [Tooltip("外网地址")]
    [SerializeField]
    private string m_ExtraResourceSourceUrl = "http://121.4.195.168:8088";
    public string ExtraResourceSourceUrl { get { return m_ExtraResourceSourceUrl; } }

    [Tooltip("正式地址")]
    [SerializeField]
    private string m_FormalResourceSourceUrl = "http://121.4.195.168:8088";
    public string FormalResourceSourceUrl { get { return m_FormalResourceSourceUrl; } }
}
[Serializable]
public class ServerIpAndPort
{
    public string ServerName;
    public string Ip;
    public int Port;
}
//intranet 10100
//192.168.29.51
//extranet
//47.98.226.149
[Serializable]
public class ServerChannelInfo
{
    public string ChannelName;
    public string CurUseServerName;
    public List<ServerIpAndPort> ServerIpAndPorts;
}

/// <summary>
/// Please modify the description.
/// </summary>
[Serializable]
public class FrameworkGlobalSettings
{
    [SerializeField]
    [Tooltip("脚本作者名")]
    private string m_ScriptAuthor = "Default";
    public string ScriptAuthor { get { return m_ScriptAuthor; } }
    [SerializeField]
    [Tooltip("版本")]
    private string m_ScriptVersion = "0.1";
    public string ScriptVersion { get { return m_ScriptVersion; } }
    [SerializeField] private AppStageEnum m_AppStage = AppStageEnum.Debug;
    public AppStageEnum AppStage { get { return m_AppStage; } }
    [Header("Resources")]
    [Tooltip("资源存放地")]
    [SerializeField]
    private ResourcesArea m_ResourcesArea;
    public ResourcesArea ResourcesArea { get { return m_ResourcesArea; } }
    [Header("SpriteCollection")]
    [SerializeField]
    [FolderPath]
    private string m_AtlasFolder = "Assets/Deer/AssetsHotfix/UI/UIArt/Atlas";
    public string AtlasFolder { get { return m_AtlasFolder; } }

    [Header("Hotfix")]
    [SerializeField]
    private string m_ResourceVersionFileName = "ResourceVersion.txt";
    public string ResourceVersionFileName { get { return m_ResourceVersionFileName; } }
    public string WindowsAppUrl = "";
    public string MacOSAppUrl = "";
    public string IOSAppUrl = "";
    public string AndroidAppUrl = "";
    [Header("Server")] 
    [SerializeField]
    private string m_CurUseServerChannel;
    public string CurUseServerChannel
    {
        get => m_CurUseServerChannel;
    }
    [SerializeField]
    private List<ServerChannelInfo> m_ServerChannelInfos;

    public List<ServerChannelInfo> ServerChannelInfos
    {
        get => m_ServerChannelInfos;
    }

    [Header("Config")]
    [SerializeField]
    private string m_ConfigFolderName = "LubanConfig";
    public string ConfigFolderName { get { return m_ConfigFolderName; } }

}