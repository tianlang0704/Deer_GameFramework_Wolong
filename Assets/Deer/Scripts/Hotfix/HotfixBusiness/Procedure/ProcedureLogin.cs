// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-06-05 19-20-08
//修改作者:杜鑫
//修改时间:2022-06-05 19-20-08
//版 本:0.1 
// ===============================================

using Cysharp.Threading.Tasks;
using GameFramework;
using HotfixFramework.Runtime;
using Main.Runtime;
using Main.Runtime.Procedure;
using Main.Runtime.UI;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace HotfixBusiness.Procedure
{
    public class ProcedureLogin : ProcedureBase
    {
        private ProcedureOwner m_ProcedureOwner;
        private int? m_UIFormSerialId;
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ProcedureOwner = procedureOwner;
            m_UIFormSerialId = GameEntry.UI.OpenUIForm(UIFormId.UILoginForm,this);

            UniTask.Void(async () =>
            {
                await UniTask.Delay(1000);
            });
        }
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            if (m_UIFormSerialId!=0)
            {
                GameEntry.UI.CloseUIForm((int)m_UIFormSerialId);
            }
        }
        public void ChangeStateToMain() 
        {
            m_ProcedureOwner.SetData<VarString>("nextProcedure", ProcedureEnum.ProcedureMain.ToString());
            ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
        }

        public void ChangeStateToBag()
        {
            ChangeState<ProcedureBag>(m_ProcedureOwner);
        }
    }
}