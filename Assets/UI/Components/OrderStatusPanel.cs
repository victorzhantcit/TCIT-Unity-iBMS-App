using iBMSApp.UI.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

namespace iBMSApp.UI.Components
{
    public class OrderStatusPanel : MonoBehaviour
    {
        [SerializeField] private PieChart _statusPieChart;
        [SerializeField] private TMP_Text _statusPieChartValue;
        [SerializeField] private TMP_Text _pendingCountLabel;
        [SerializeField] private Image _pendingBackground;
        [SerializeField] private TMP_Text _processingCountLabel;
        [SerializeField] private Image _processingBackground;
        [SerializeField] private TMP_Text _processedCountLabel;
        [SerializeField] private Image _processedBackground;

        public int PendingCount { get; private set; }
        public int ProcessingCount { get; private set; }
        public int ProcessedCount { get; private set; }
        public int CompletedRate { get; private set; }

        private Action QueryStatusPending = null;
        private Action QueryStatusProcessing = null;
        private Action QueryStatusProcessed = null;
        private Action QueryStatusAll = null;

        private void Start()
        {
            if (ColorMapper.Instance == null)
                return;

            Color pendingColor = ColorMapper.Instance.GetColor("bgc-pending");
            Color processingColor = ColorMapper.Instance.GetColor("bgc-processing");
            Color processedColor = ColorMapper.Instance.GetColor("bgc-processed");

            if (_pendingCountLabel != null)
                _pendingCountLabel.color = pendingColor;
            if (_pendingBackground != null)
                _pendingBackground.color = pendingColor;
            if (_processingCountLabel != null)
                _processingCountLabel.color = processingColor;
            if (_processingBackground != null)
                _processingBackground.color = processingColor;
            if (_processedCountLabel != null)
                _processedCountLabel.color = processedColor;
            if (_processedBackground != null)
                _processedBackground.color = processedColor;
        }

        public void UpdateStatus(int pendingCount = 0, int processingCount = 0, int processedCount = 0, bool rateUpdate = true)
        {
            int totalCount = pendingCount + processingCount + processedCount;

            PendingCount = pendingCount;
            ProcessingCount = processingCount;
            ProcessedCount = processedCount;

            if (_pendingCountLabel != null)
                _pendingCountLabel.text = pendingCount.ToString();
            if (_processingCountLabel != null)
                _processingCountLabel.text = processingCount.ToString();
            if (_processedCountLabel != null)
                _processedCountLabel.text = processedCount.ToString();

            Canvas.ForceUpdateCanvases();
            if (rateUpdate && _statusPieChart != null && _statusPieChartValue != null)
            {
                CompletedRate = (processedCount * 100);
                if ((pendingCount + processingCount + processedCount) == 0) CompletedRate = 100;
                else CompletedRate = CompletedRate / (pendingCount + processingCount + processedCount);

                _statusPieChartValue.text = CompletedRate.ToString();
                _statusPieChart.UpdateData("serie0", 0, 100 - CompletedRate);
                _statusPieChart.UpdateData("serie0", 1, CompletedRate);

                _statusPieChart.RefreshChart(); // 若沒自動刷新，可以加這行強制更新
            }
        }

        public void AssignEventQueryAll(Action action) => QueryStatusAll = action;
        public void AssignEventPending(Action action) => QueryStatusPending = action;
        public void AssignEventProcessing(Action action) => QueryStatusProcessing = action;
        public void AssignEventProcessed(Action action) => QueryStatusProcessed = action;


        public void OnAllStatusClicked() => QueryStatusAll?.Invoke();
        public void OnPendingClicked() => QueryStatusPending?.Invoke();
        public void OnProcessingClicked() => QueryStatusProcessing?.Invoke();
        public void OnProcessedClicked() => QueryStatusProcessed?.Invoke();
    }
}
