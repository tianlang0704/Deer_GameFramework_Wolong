﻿// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-06-18 00-19-22
//修改作者:CMonk
//修改时间:2022-11-29 23-58-46
//版 本:0.1 
// ===============================================
using HotfixBusiness.Procedure;
using HotfixFramework.Runtime;
using Main.Runtime;
using UnityGameFramework.Runtime;

namespace HotfixBusiness.UI 
{
    /// <summary>
    /// Please modify the description.
    /// </summary>
    public partial class UILoginForm : UIFixBaseForm
    {
        protected override void OnInit(object userData) {
            base.OnInit(userData);
            GetBindComponents(gameObject);

/*--------------------Auto generate start button listener.Do not modify!--------------------*/
			 m_Btn_Login.onClick.AddListener(Btn_LoginEvent);
			 m_Btn_Login1.onClick.AddListener(Btn_Login1Event);
			 m_Btn_UIButtonTest.onClick.AddListener(Btn_UIButtonTestEvent);
			 m_Btn_UIButtonTestTips.onClick.AddListener(Btn_UIButtonTestTipsEvent);
			 m_Btn_UIButtonTestDialog.onClick.AddListener(Btn_UIButtonTestDialogEvent);
/*--------------------Auto generate end button listener.Do not modify!----------------------*/
            m_RImg_bg.SetTexture(AssetUtility.UI.GetTexturePath("loading_bg"));
            m_Img_Icon.SetSprite(AssetUtility.UI.GetSpriteCollectionPath("Icon"),AssetUtility.UI.GetSpritePath("Icon/Icon"));
            m_RImg_NetImage.SetTextureByNetwork("https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png");
        }

        private void Btn_LoginEvent()
        {
            ProcedureLogin procedure = (ProcedureLogin)GameEntry.Procedure.CurrentProcedure;
            procedure.ChangeStateToMain();
        }
        private void Btn_Login1Event(){}
        private void Btn_UIButtonTestEvent(){}

        private void Btn_UIButtonTestTipsEvent()
        {
	        GameEntry.UI.OpenTips("哈喽，我是飘字提示！！！");
        }

        private void Btn_UIButtonTestDialogEvent()
        {
	        DialogParams dialogParams = new DialogParams();
	        dialogParams.Mode = 2;
	        dialogParams.ConfirmText = "确定";
	        dialogParams.CancelText = "取消";
	        dialogParams.OnClickConfirm = delegate(object o)
	        {
		        GameEntry.UI.OpenTips("害，你点击了确定！");
	        };
	        dialogParams.OnClickCancel = delegate(object o)
	        {
		        GameEntry.UI.OpenTips("害，你点击了取消！");
	        };
	        dialogParams.OnClickBackground = delegate(object o)
	        {
		        GameEntry.UI.OpenTips("害，你点击了背景！");
	        };
	        dialogParams.Message = $"哈喽，我是提示信息框！！！";
	        GameEntry.UI.OpenDialog(dialogParams);
        }
/*--------------------Auto generate footer.Do not add anything below the footer!------------*/
	}
}
