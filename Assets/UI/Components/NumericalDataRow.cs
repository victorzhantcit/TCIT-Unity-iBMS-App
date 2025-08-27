using iBMSApp.Shared;
using UnityEngine;

namespace iBMSApp.UI.Components
{
    public class NumericalDataRow : MonoBehaviour
    {
        [SerializeField] private NumbericalDataPanel _dataColumnPanel1;
        [SerializeField] private NumbericalDataPanel _dataColumnPanel2;

        private EqptOrderDevNumericalData _data1;
        private EqptOrderDevNumericalData _data2;

        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void UpdateColumnDisplay(int dataCount)
        {
            _dataColumnPanel1.gameObject.SetActive(dataCount >= 1);
            _dataColumnPanel2.gameObject.SetActive(dataCount >= 2);
        }

        public void UpdateNumericalDatas(bool isDisabled, EqptOrderDevNumericalData dataColumn1, EqptOrderDevNumericalData dataColumn2 = null)
        {
            _data1 = dataColumn1;
            _data2 = dataColumn2;

            if (dataColumn1 != null && dataColumn2 == null)
            {
                _dataColumnPanel1.Bind(_data1, isDisabled);
                UpdateColumnDisplay(1);
                Show();
            }
            else if (dataColumn1 != null && dataColumn2 != null)
            {
                _dataColumnPanel1.Bind(_data1, isDisabled);
                _dataColumnPanel2.Bind(_data2, isDisabled);
                UpdateColumnDisplay(2);
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}
