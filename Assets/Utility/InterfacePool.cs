using System.Collections.Generic;
using UnityEngine;

namespace iBMSApp.Utility
{
    public class InterfacePool<TPoolItem, TData>
           where TPoolItem : MonoBehaviour, IObjectPoolItem<TData>
           where TData : class
    {
        private readonly TPoolItem prefab;
        private readonly Transform parent;
        private readonly Queue<TPoolItem> pool = new Queue<TPoolItem>();

        public InterfacePool(TPoolItem prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        // �q�������X����A�p�G�����S���A�h�Ыؤ@�ӷs��
        public TPoolItem Get()
        {
            if (pool.Count > 0)
            {
                TPoolItem item = pool.Dequeue();
                item.Show();
                return item;
            }
            else
            {
                return Object.Instantiate(prefab, parent);
            }
        }

        // �^������A�N�������èé�^����
        public void Release(TPoolItem item)
        {
            item.Hide();
            pool.Enqueue(item);
        }

        // �M�Ū����
        public void Clear()
        {
            pool.Clear();
        }
    }
}
