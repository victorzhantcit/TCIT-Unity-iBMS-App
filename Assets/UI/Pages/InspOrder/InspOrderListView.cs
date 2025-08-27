using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using iBMSApp.UI.Components;
using iBMSApp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    public class InspOrderListView : NetworkPageMonoBehavior<InspOrderListView>
    {
        private static readonly LocalizedStyle LabelLocalizedStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Comma;

        private static SemaphoreSlim Locker = new SemaphoreSlim(1, 1);//非同步鎖


        [Header("UI")]
        [SerializeField] private OrderStatusPanel _orderStatusPanel;
        [SerializeField] private TMP_Text _latestDataTimeLabel;
        [SerializeField] private VisualButtonHelper _redoButton;
        [SerializeField] private RectTransform _orderListContent;
        [SerializeField] private InspOrderListItem _orderListItemPrefab;
        [Header("Data Filter")]
        [SerializeField] private int _ordersMonthSpan = 3;

        // Services(DI)
        private EqptService _eqptService;
        private ILocalStorageService _localStorage;

        private ObjectPool<InspOrderListItem> _orderListItemPool;
        private List<EqptOrder> _orders = new List<EqptOrder>();

        private string _lastOnLineTime = "";
        private int i_processed = 0;
        private int i_pending = 0;
        private int i_processing = 0;
        private int CompletionRate = 100;
        private int _uploadsPendingCount = 0;
        private bool _updateLocker = true;

        private static string MakeLabelLocalizedInfo(string key, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                key,
                value,
                LabelLocalizedStyle
            );
        }

        #region Override BaseClass
        private new void Start()
        {
            base.Start();

            if (!LoadServices())
                return;

            InitUIOnStart();
            _updateLocker = false;
        }

        private new void Update()
        {
            if (_updateLocker)
                return;

            base.Update();
        }

        /// <inheritdoc/>
        public override void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        public override async Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            await GetList(0);
        }
        #endregion

        private bool LoadServices()
        {
            if (ServiceManager.Instance == null)
                return false;

            _eqptService = ServiceManager.Instance.EqptService;
            _localStorage = ServiceManager.Instance.LocalStorageService;
            return true;
        }

        private void InitUIOnStart()
        {
            _orderListItemPool = new ObjectPool<InspOrderListItem>(_orderListItemPrefab, _orderListContent);
            _orderStatusPanel.AssignEventQueryAll(() => _ = GetList(0));
            _orderStatusPanel.AssignEventPending(() => _ = GetList(1));
            _orderStatusPanel.AssignEventProcessing(() => _ = GetList(2));
            _orderStatusPanel.AssignEventProcessed(() => _ = GetList(3));
            _ = GetList(0);
            Show();
        }

        public void OnUploadsPendingClicked() => _ = Redo();

        /// <summary>
        /// 上傳格式詳見 <seealso cref="EqptRequestOrders"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task GetList(int type)
        {
            DebugLog("GetList() Locker.CurrentCount:" + Locker.CurrentCount);
            if (Locker.CurrentCount == 0) Locker.Release();
            await Locker.WaitAsync();//非同步等待

            try
            {
                base.LoadingPage(true);
                ClearOrderList();
                
                _uploadsPendingCount = await _eqptService.GetAllDataCountTasks();
                DebugLog($"GetList() _isOnline: {_isOnline}, taskCount: {_uploadsPendingCount}");
                if (!_isOnline || _uploadsPendingCount > 0 || type != 0)
                {
                    _orders = await _eqptService.GetAllDataInspOrders();
                    _lastOnLineTime = await _eqptService.GetLastOnlineTime();
                    UpdateLatestTime(_lastOnLineTime);
                    DebugLog("GetAllDataAsync(\"inspOrders\")...");
                }
                else
                {
                    var queryData = new Dictionary<string, string>();
                    string startDate = DateTime.Now.AddMonths(-(_ordersMonthSpan)).ToString("yyyy-MM-dd");
                    string endDate = DateTime.Now.AddMonths(_ordersMonthSpan).ToString("yyyy-MM-dd");

                    queryData.Add(nameof(EqptRequestOrders.BuildingCode), "RG");
                    queryData.Add(nameof(EqptRequestOrders.StartDate), startDate);
                    queryData.Add(nameof(EqptRequestOrders.EndDate), endDate);

                    var response = await _eqptService.PostInspOrders(queryData);

                    if (!response.IsSuccess)
                    {
                        base.HandleHttpRequestException(response);
                        base.LoadingPage(false);
                        return;
                    }

                    _orders = response.Data ?? new List<EqptOrder>();
                    _lastOnLineTime = await _eqptService.GetLastOnlineTime();
                    UpdateLatestTime(_lastOnLineTime);
                }

                int siblingIndex = 0;
                bool showedList = false;
                string PhotoSns = ",";

                _orderListContent.gameObject.SetActive(false);
                if (type == 0)
                {
                    i_pending = 0; i_processed = 0; i_processing = 0;
                }
                foreach (var o in _orders)
                {
                    o.Show = false; // 從 Web App 移植過來不知道為什麼沒有初始化這個值 
                    if (o.StatusType == 0)
                    {
                        if (type == 0) i_pending++;
                        if (type == 0 || type == 1) o.Show = true;
                    }
                    else if (o.StatusType == 1)
                    {
                        if (type == 0) i_processing++;
                        if (type == 0 || type == 2) o.Show = true;
                    }
                    else
                    {
                        if (type == 0) i_processed++;
                        if (type == 0 || type == 3) o.Show = true;
                    }

                    if (!o.PhotoSns.Equals("")) PhotoSns = PhotoSns + o.PhotoSns + ",";

                    if (o.Show == true)
                    {
                        InspOrderListItem orderView = _orderListItemPool.Get();

                        orderView.UpdateListItem(o);
                        orderView.BindEnterOrderEvent(() => _ = StartInspection(o.BuildingCode, o.RecordSn));
                        orderView.transform.SetSiblingIndex(siblingIndex);
                        siblingIndex++;

                        showedList = true;
                    }
                }
                if (!showedList)
                {
                    _latestDataTimeLabel.text += $"\n{Localizer.Instance.GetLocalizedString("無資料紀錄")}";
                }
                _orderListContent.gameObject.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_orderListContent);

                if (type == 0)
                {
                    CompletionRate = (i_processed * 100);
                    if ((i_pending + i_processing + i_processed) == 0) CompletionRate = 100;
                    else CompletionRate = CompletionRate / (i_pending + i_processing + i_processed);
                }

                // 更新頁面
                _orderStatusPanel.UpdateStatus(
                    pendingCount: i_pending,
                    processingCount: i_processing,
                    processedCount: i_processed, 
                    rateUpdate: type == 0
                );

                base.LoadingPage(false);
                if (_isOnline && _uploadsPendingCount == 0)
                {
                    await StoragePhotos(PhotoSns);
                }

                DebugLog("Done!");
            }
            catch (OperationCanceledException)
            {
                //DebugLog(e.Message);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                DebugLog("GetList() Locker.Release()..." + Locker.CurrentCount);
                if (Locker.CurrentCount > 0) Locker.Release();
                base.LoadingPage(false);
            }
        }

        private void UpdateLatestTime(string timeString = null)
        {
            bool hasPendingUploads = _uploadsPendingCount > 0;
            string uploadPendingHint = Localizer.Instance.GetLocalizedString("T_Total") +
                $" {_uploadsPendingCount} {Localizer.Instance.GetLocalizedString("T_PendingUpload")}";

            _latestDataTimeLabel.text = MakeLabelLocalizedInfo("T_OffLineTime", timeString ?? _lastOnLineTime);
            _redoButton.gameObject.SetActive(hasPendingUploads);
            if (hasPendingUploads)
            {
                _redoButton.ShowContentDefault();
                _redoButton.SetButtonInteractable(true);
                _redoButton.UpdateText(uploadPendingHint);
            }
        }

        private void ClearOrderList()
        {
            _orders.Clear();
            foreach (Transform item in _orderListContent)
            {
                if (item.gameObject.activeSelf == false)
                {
                    return;
                }
                if (item.TryGetComponent<InspOrderListItem>(out var itemComponent))
                {
                    _orderListItemPool.Release(itemComponent);
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// 已合併至 <seealso cref="GetList(int)"/> 優化顯示效率
        /// </summary>
        private void UpdateOrderList()
        {
            int siblingIndex = 0;
            foreach (EqptOrder order in _orders)
            {
                if (order.Show == true)
                {
                    InspOrderListItem orderView = _orderListItemPool.Get();

                    orderView.UpdateListItem(order);
                    orderView.BindEnterOrderEvent(() => _ = StartInspection(order.BuildingCode, order.RecordSn));
                    orderView.transform.SetSiblingIndex(siblingIndex);
                    siblingIndex++;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_orderListContent);
        }

        private async Task Redo()
        {
            _redoButton.ShowContentActivated();
            _redoButton.SetButtonInteractable(false);

            if (!_isOnline)
            {
                _toastService.ShowToast("離線中，無法執行", ToastLevel.Warning);
                UpdateLatestTime();
                return;
            }
            if (_uploadsPendingCount == 0)
            {
                _toastService.ShowToast("無待處理資料!", ToastLevel.Warning);
                UpdateLatestTime();
                return;
            }

            System.Net.HttpStatusCode responseStatus = await _eqptService.PostCursorTask();
            if (!_eqptService.IsSuccessStatusCode(responseStatus))
            {
                if (responseStatus == System.Net.HttpStatusCode.NoContent)
                {
                    await _authService.LogoutAsync();
                    base.GoToSceneIndex();
                }
                else if (responseStatus == System.Net.HttpStatusCode.Unauthorized) //401
                {
                    base.GoToSceneLogin();
                }
                else
                {
                    _toastService.ShowToast(responseStatus.ToString(), ToastLevel.Warning);
                }
                UpdateLatestTime();
                return;
            }

            _uploadsPendingCount = await _eqptService.GetAllDataCountTasks();
            if (_uploadsPendingCount == 0)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Completed"), ToastLevel.Success);
                await GetList(0);
            }
            else await Redo();
        }

        private async Task StoragePhotos(string photoSns)
        {
            List<string> photoList = photoSns.Trim(',').Split(',').Distinct().ToList();
            List<string> waitForUpdateSns = new List<string>();

            base.ResetProgressBar();
            int photoCount = photoList.Count;
            float unitPercent = (photoCount > 0) ? 1f / photoCount : 1f;
            for (int photoIndex = 0; photoIndex < photoCount; photoIndex++)
            {
                base.SetProgressBar(unitPercent * (photoIndex + 1));

                string sn = photoList[photoIndex];
                var photoData = await _eqptService.GetInspPhoto(sn);

                if (photoData == null)
                {
                    DebugLog($"Local photo ({sn}) not found. Adding to server request queue...");
                    waitForUpdateSns.Add(sn);
                    continue;
                }

                DateTime localCreateTime = photoData.CreateTime;

                if (await IsPhotoUpToDate(sn, localCreateTime))
                {
                    DebugLog($"Photo({sn}) is up to date.");
                    continue;
                }

                DebugLog($"Photo({sn}) version is outdated. Adding to server request queue...");
                waitForUpdateSns.Add(sn);
            }
            base.ResetProgressBar();

            photoCount = waitForUpdateSns.Count;
            unitPercent = (photoCount > 0) ? 1f / photoCount : 1f;
            for (int updateIndex = 0; updateIndex < photoCount; updateIndex++)
            {
                base.SetProgressBar(unitPercent * (updateIndex + 1));

                string sn = waitForUpdateSns[updateIndex];
                DebugLog($"Requesting photo({sn}) from server...");
                await GetPhoto(sn);
            }
            base.ResetProgressBar();
        }

        private async Task<bool> IsPhotoUpToDate(string sn, DateTime createTime)
        {
            if (!_isOnline || sn.Equals("")) return default;
            //DebugLog("GetPhotoVersion");
            var response = await _eqptService.PostPhotoVersion(new EqptPhotoVersion { Sn = int.Parse(sn), CreateTime = createTime });

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                return false;
            }

            return response.Data;
        }

        private async Task GetPhoto(string sn)
        {
            if (!_isOnline || string.IsNullOrEmpty(sn)) return;
            //DebugLog("GetPhoto");
            var response = await _eqptService.GetRequestPhoto(sn);

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                return;
            }

            string base64Img = response.Data.Photo;
            DateTime createTime = response.Data.CreateTime;

            if (!string.IsNullOrEmpty(base64Img))
            {
                await _eqptService.SetInspPhoto(sn, base64Img, createTime);
                DebugLog($"Photo({sn}) request completed.");
            }
        }

        private async Task StartInspection(string buildingCode, string recordSn)
        {
            await Task.Yield();

            if (_isOnline && _uploadsPendingCount != 0)
            {
                // toastService.ShowToast("有工作紀錄待補上傳!", ToastLevel.Warning);
                // return;
            }

            ClearOrderList();
            base.GoToPageChild(new PageRefreshParams {
                BuildingCode = buildingCode,
                RecordSn = recordSn
            });
        }
    }
}
