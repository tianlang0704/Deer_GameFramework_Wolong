// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-06-07 14-14-22
//修改作者:杜鑫
//修改时间:2022-06-07 14-14-22
//版 本:0.1 
// ===============================================
using GameFramework;
using GameFramework.Event;
using HotfixFramework.Runtime;
using Main.Runtime.Procedure;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace HotfixBusiness.Procedure
{
    public class ProcedureChangeScene : ProcedureBase
    {
        private bool m_LoadSceneComplete;
        private System.Type m_nextProcedure;
        private string m_sceneName;
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            (m_sceneName, m_nextProcedure) = ((string, System.Type))procedureOwner.GetData<VarTuple>("nextProcedure");
            OnStartLoadScene();
            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnHandleLoadSceneSuccess);
            GameEntry.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnHandleLoadSceneFailure);
            GameEntry.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnHandleLoadSceneUpdate);
            GameEntry.Event.Subscribe(LoadSceneDependencyAssetEventArgs.EventId, OnHandleLoadSceneDependencyAsset);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (m_LoadSceneComplete)
            {
                ChangeState(procedureOwner, m_nextProcedure);
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnHandleLoadSceneSuccess);
            GameEntry.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnHandleLoadSceneFailure);
            GameEntry.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnHandleLoadSceneUpdate);
            GameEntry.Event.Unsubscribe(LoadSceneDependencyAssetEventArgs.EventId, OnHandleLoadSceneDependencyAsset);
        }

        void OnStartLoadScene() 
        {
            UnloadAllScene();
            GameEntry.Entity.HideAllLoadedEntities();
            GameEntry.ObjectPool.ReleaseAllUnused();
            GameEntry.Resource.ForceUnloadUnusedAssets(true);
            GameEntry.Scene.LoadScene(AssetUtility.Scene.GetSceneAsset(m_sceneName), Constant.AssetPriority.SceneAsset);
        }

        void UnloadAllScene() 
        {
            string[] loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            foreach (string sceneAssetName in loadedSceneAssetNames) 
            {
                GameEntry.Scene.UnloadScene(sceneAssetName);
            }
        }
        private void OnHandleLoadSceneSuccess(object sender, GameEventArgs e)
        {
            m_LoadSceneComplete = true;
        }
        private void OnHandleLoadSceneFailure(object sender, GameEventArgs e)
        {
        }
        private void OnHandleLoadSceneUpdate(object sender, GameEventArgs e)
        {
        }
        private void OnHandleLoadSceneDependencyAsset(object sender, GameEventArgs e)
        {
        }
    }
}