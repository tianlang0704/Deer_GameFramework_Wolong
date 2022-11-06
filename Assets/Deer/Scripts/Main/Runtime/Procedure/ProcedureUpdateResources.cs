// ================================================
//描 述:
//作 者:杜鑫
//创建时间:2022-06-09 01-52-23
//修改作者:杜鑫
//修改时间:2022-06-09 01-52-23
//版 本:0.1 
// ===============================================
using GameFramework;
using Main.Runtime.UI;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Main.Runtime.Procedure
{
    public class ProcedureUpdateResources : ProcedureBase
    {
        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            if (GameEntryMain.Base.EditorResourceMode)
            {
                ChangeState<ProcedureLoadAssembly>(procedureOwner);
                return;
            }

            Log.Info("Start update resource group ");
            UniTask.Void(async () =>
            {
                try
                {
                    // 更新列表
                    var catalogList = await GameEntryMain.Resource.CheckCatalogAsync();
                    if (catalogList.Count > 0)
                    {
                        await GameEntryMain.Resource.UpdateCatalogsAsync();
                    }
                    // 更新高优先度(包内内容, 大小很小, 直接更新)
                    var highPrioritySize = await GameEntryMain.Resource.GetHighPriorityDownloadSizeAsync();
                    if (highPrioritySize > 0)
                    {
                        Log.Info($"高优先度更新大小: {FileUtils.GetLengthString(highPrioritySize)}");
                        await GameEntryMain.Resource.UpdateHighPriorityResourceAsync();
                    }
                    // 更新普通游戏资源
                    var totalSize = await GameEntryMain.Resource.GetDownloadSizeAsync();
                    if (totalSize <= 0) 
                    {
                        // 无需更新, 直接加载脚本
                        ChangeState<ProcedureLoadAssembly>(procedureOwner);
                        return;
                    }
                    // 确认更新
                    await NoticeUpdate(totalSize);
                    // 打开更新界面
                    var nativeLoadingFormId = await GameEntryMain.UI.OpenUIFormAsync(Main.Runtime.AssetUtility.UI.GetUIFormAsset("UINativeLoadingForm"), "Default", false, this);
                    GameEntryMain.UI.SettingForegroundSwitch(false);//关闭前置背景
                    // 刷新更新进度
                    await foreach (var progress in GameEntryMain.Resource.UpdateResourceAsync())
                    {
                        RefreshProgress(nativeLoadingFormId, totalSize, progress);
                    }
                    // 更新完成, 加载脚本
                    ChangeState<ProcedureLoadAssembly>(procedureOwner);
                }
                catch (Exception e)
                {
                    Log.Error("Update resources complete with errors. Error message:" + e.Message);
                    NoticeError($"更新出错, 请检查网络").Forget();
                }
            });
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }
        
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }

        private async UniTask NoticeUpdate(long updateTotalZipLength)
        {
            bool isBack = false;
            string content = Utility.Text.Format("有{0}更新", FileUtils.GetLengthString(updateTotalZipLength));
            Log.Info(content);
            NativeMessageBoxOption nativeMessageBoxOption = new NativeMessageBoxOption();
            nativeMessageBoxOption.title = "提示";
            nativeMessageBoxOption.message = Utility.Text.Format("客官,有{0}更新，请立马更新！", FileUtils.GetLengthString(updateTotalZipLength));
            nativeMessageBoxOption.onSure = () => { isBack = true; };
            nativeMessageBoxOption.onCancel = () => { Application.Quit(); };
            await GameEntryMain.UI.OpenUIFormAsync(Main.Runtime.AssetUtility.UI.GetUIFormAsset("UINativeMessageBoxForm"), "Default", false, nativeMessageBoxOption);
            await UniTask.WaitUntil(() => isBack);
        }

        private async UniTask NoticeError(string text)
        {
            NativeMessageBoxOption nativeMessageBoxOption = new NativeMessageBoxOption();
            nativeMessageBoxOption.title = "错误";
            nativeMessageBoxOption.message = text;
            nativeMessageBoxOption.onSure = () => { Application.Quit(); };
            nativeMessageBoxOption.onCancel = () => { Application.Quit(); };
            await GameEntryMain.UI.OpenUIFormAsync(Main.Runtime.AssetUtility.UI.GetUIFormAsset("UINativeMessageBoxForm"), "Default", false, nativeMessageBoxOption);
        }
        
        private void RefreshProgress(int formID, long totalSize, float progress)
        {
            var tips = $"{FileUtils.GetByteLengthString((long)(totalSize * progress))}/{FileUtils.GetByteLengthString(totalSize)}";
            var nativeLoadingForm = (UINativeLoadingForm)GameEntryMain.UI.GetUIForm(formID)?.Logic;
            nativeLoadingForm?.RefreshProgress(progress, 1, tips);
        }
    }
}