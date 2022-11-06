// ================================================
//描 述:
//作 者:CMonk
//创建时间:2022-11-08 00-35-43
//修改作者:CMonk
//修改时间:2022-11-08 00-35-43
//版 本:0.1 
// ===============================================

using HotfixBusiness.Procedure;
using HotfixFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotfixBusiness.UI
{
	/// <summary>
	/// Please modify the description.
	/// </summary>
	public partial class UIBagForm : UIFixBaseForm
	{
		protected override void OnInit(object userData) {
			 base.OnInit(userData);
			 GetBindComponents(gameObject);

/*--------------------Auto generate start button listener.Do not modify!--------------------*/
			m_Btn_Test.onClick.AddListener(Btn_TestEvent);
/*--------------------Auto generate end button listener.Do not modify!----------------------*/
		}

		private void Btn_TestEvent(){
			ProcedureBag procedure = (ProcedureBag)GameEntry.Procedure.CurrentProcedure;
            procedure.ChangeStateToLogin();
		}
/*--------------------Auto generate footer.Do not add anything below the footer!------------*/
	}
}
