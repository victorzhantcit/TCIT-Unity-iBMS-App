namespace iBMSApp.Utility
{
    public interface IObjectPoolItem<T> where T : class
    {
        /// <summary>
        /// �j�w�@�Ӽƾڨ춵�ؤ��A�ó]�m�O�_�T�θӶ���
        /// </summary>
        /// <param name="bindingReference"></param>
        /// <param name="isDisabled"></param>
        public void Bind(T bindingReference, bool isDisabled);

        /// <summary>
        /// ��ܸӶ���
        /// </summary>
        public void Show();

        /// <summary>
        /// ���øӶ���
        /// </summary>
        public void Hide();
    }
}

