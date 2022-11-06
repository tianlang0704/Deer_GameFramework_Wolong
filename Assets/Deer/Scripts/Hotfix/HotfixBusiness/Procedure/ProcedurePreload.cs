// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-06-05 18-42-47
//修改作者:杜鑫
//修改时间:2022-06-05 18-42-47
//版 本:0.1 
// ===============================================

using System;
using GameFramework.Resource;
using Main.Runtime.Procedure;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using UnityEngine;

namespace HotfixBusiness.Procedure
{
    public class ProcedurePreload : ProcedureBase
    {
        private ProcedureOwner m_procedureOwner = null;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Debug.Log("tackor HotFix ProcedurePreload OnEnter");

            m_procedureOwner = procedureOwner;
            //初始化所有角色信息管理器
            UniTask.Void(async () =>
            {
                await PreloadConfig();
                // ChangeState<ProcedureLogin>(procedureOwner);
                ChangeState<ProcedureMenu>(procedureOwner);
            });
            
        }
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }
        #region Config
        private async UniTask PreloadConfig()
        {
            try
            {
                await GameEntry.Config.LoadAllUserConfig();
            }
            catch (Exception e)
            {
                Logger.ColorInfo(ColorType.cadetblue, e.Message);
                throw;
            }
        }
        #endregion
    }
}