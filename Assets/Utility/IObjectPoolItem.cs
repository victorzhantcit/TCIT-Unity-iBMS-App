namespace iBMSApp.Utility
{
    public interface IObjectPoolItem<T> where T : class
    {
        /// <summary>
        /// 綁定一個數據到項目中，並設置是否禁用該項目
        /// </summary>
        /// <param name="bindingReference"></param>
        /// <param name="isDisabled"></param>
        public void Bind(T bindingReference, bool isDisabled);

        /// <summary>
        /// 顯示該項目
        /// </summary>
        public void Show();

        /// <summary>
        /// 隱藏該項目
        /// </summary>
        public void Hide();
    }
}

