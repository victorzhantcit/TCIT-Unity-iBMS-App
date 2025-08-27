using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using iBMSApp.UI.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Pages
{
    /// <summary>
    /// 尚未處理完畢 (Copy form <see cref="WorkOrderListView"/>)
    /// </summary>
    public class RepairOrderListView : NetworkPageMonoBehavior<RepairOrderListView>
    {
        private static readonly LocalizedStyle LabelLocalizedStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Comma;

        private static SemaphoreSlim Locker = new SemaphoreSlim(1, 1); // 非同步鎖

        [Header("UI")]
        [SerializeField] private OrderStatusPanel _orderStatusPanel;
        [SerializeField] private TMP_Text _latestDataTimeLabel;
        [SerializeField] private RepairOrderVirtualList _orderVirtualList;

        [Header("Data Filter")]
        [SerializeField] private int _ordersMonthSpan = 3;

        // Services(DI)
        private EqptService _eqptService;
        private ILocalStorageService _localStorage;

        private List<EqptRepairOrder> _orders = new List<EqptRepairOrder>();

        private int i_processed = 0;
        private int i_pending = 0;
        private int CompletionRate = 100;
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
            DebugLog("RefreshPage");
            await GetRepairList(0);
        }
        #endregion

        private bool LoadServices()
        {
            if (ServiceManager.Instance == null)
                return false;

            _eqptService = ServiceManager.Instance.EqptService;
            _localStorage = ServiceManager.Instance.LocalStorageService;
            _toastService = ServiceManager.Instance.ToastService;
            return true;
        }

        /// 初始化的 UI 顯示狀態由 <seealso cref="RepairOrderPageRouter"/> 管理
        private void InitUIOnStart()
        {
            _orderStatusPanel.AssignEventQueryAll(() => _ = GetRepairList(0));
            _orderStatusPanel.AssignEventPending(() => _ = GetRepairList(1));
            _orderStatusPanel.AssignEventProcessed(() => _ = GetRepairList(2));
        }

        /// <summary>
        /// 上傳格式詳見 <seealso cref="EqptRequestOrders"/>eea
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task GetRepairList(int type)
        {
            _orders.Clear();
            if (type == 0)
            {
                i_pending = 0; i_processed = 0;
            }

            string uid = await base.GetUidAsync();
            string nMonthsBeforeToday = DateTime.Now.AddMonths(-_ordersMonthSpan).ToString("yyyy-MM-dd");
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            var queryPara = new Dictionary<string, string>();

            queryPara.Add(nameof(EqptRequestOrders.BuildingCode), "RG");
            queryPara.Add(nameof(EqptRequestOrders.StartDate), nMonthsBeforeToday);
            queryPara.Add(nameof(EqptRequestOrders.EndDate), today);

            base.LoadingPage(true);
            var response = await _eqptService.PostRepairOrders(queryPara);

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                base.LoadingPage(false);
                return;
            }

            var orders = response.Data;

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    var rp = new EqptRepairOrder();
                    int orderStatusCode = 0;

                    // 由項目內部處理狀態的 Localization
                    //string statusKey = order.Status.StartsWith("S_") ? order.Status : "S_" + order.Status;
                    //var localized = Localizer.Instance.GetLocalizedString(statusKey);
                    rp.Status = order.Status;

                    if (order.Status.Equals("Pending") ||
                        order.Status.Equals("Processing") ||
                        order.Status.Equals("Scheduled"))
                    {
                        rp.statusColor = "c-pending";
                        rp.statusBGColor = "bgc-pending";
                        orderStatusCode = 0;
                    }
                    else
                    {
                        //完成
                        rp.statusColor = "c-processed";
                        rp.statusBGColor = "bgc-processed";
                        orderStatusCode = 1;
                    }

                    rp.BuildingName = order.BuildingName ?? "";
                    rp.BuildingCode = order.BuildingCode ?? "";
                    rp.RecordSn = order.RecordSn ?? "";
                    rp.Issue = order.Issue;
                    rp.CreateTime = order.CreateTime;
                    rp.IssuerName = order.IssuerName ?? "";
                    rp.Code = order.Code ?? "";
                    rp.DeviceType = order.DeviceType ?? "";
                    if (rp.DeviceType.Equals("Other"))
                    {
                        rp.DeviceDescription = order.DeviceDescription;//非設備類報修
                    }
                    else
                    {
                        var foundDevice = await _eqptService.GetDeviceData(x => x.Code == rp.Code); // deviceModel.GetDescriptionOfCodeAsync(rp.Code);
                        string DeviceDescription = foundDevice?.Description ?? "";

                        if (rp.Code.Equals("") || DeviceDescription.Equals(rp.Code))
                            rp.DeviceDescription = order.DeviceDescription;
                        else
                            rp.DeviceDescription = DeviceDescription;
                    }

                    rp.ProcessTime = order.ProcessTime;
                    rp.CompleteTime = order.CompleteTime;
                    rp.ScheduledDate = order.ScheduledDate;

                    if (orderStatusCode == 0)
                    {
                        if (type == 0) i_pending++;
                        if (type == 0 || type == 1) _orders.Add(rp);
                    }
                    else
                    {
                        if (type == 0) i_processed++;
                        if (type == 0 || type == 2) _orders.Add(rp);
                    }
                }

                _orderVirtualList.BindList(_orders, false, (item, data) =>
                {
                    bool isIssuerAndNotStart = data.IssuerName.Equals(uid) && data.ProcessTime == null;
                    item.BindEnterOrderEvent(
                        isIssuerAndNotStart,
                        editAction: () => EditRepairRecord(data.BuildingCode, data.RecordSn),
                        viewAction: () => ViewRepairRecord(data.BuildingCode, data.RecordSn)
                    );
                });

                if (type == 0)
                {
                    CompletionRate = (i_processed * 100);
                    if ((i_pending + i_processed) == 0) CompletionRate = 100;
                    else CompletionRate = CompletionRate / (i_pending + i_processed);
                }

                _orderStatusPanel.UpdateStatus(
                    pendingCount: i_pending,
                    processingCount: 0,
                    processedCount: i_processed,
                    rateUpdate: type == 0
                );
            }

            base.LoadingPage(false);
        }

        private void ViewRepairRecord(string buildingCode, string recordSn)
        {
            NavigateToRepairOrder(buildingCode, recordSn, true);
        }

        private void EditRepairRecord(string buildingCode, string recordSn)
        {
            NavigateToRepairOrder(buildingCode, recordSn, false);
        }

        private void NavigateToRepairOrder(string buildingCode, string recordSn, bool pageReadOnly)
        {
            _orders.Clear();
            //DebugLog("repairNo:" + RecordSn);
            base.GoToPageChild(new PageRefreshParams
            {
                BuildingCode = buildingCode,
                RecordSn = recordSn,
                PageReadOnly = pageReadOnly
            });
        }
    }
}
