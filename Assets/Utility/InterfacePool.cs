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

        // 從池中取出物件，如果池中沒有，則創建一個新的
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

        // 回收物件，將物件隱藏並放回池中
        public void Release(TPoolItem item)
        {
            item.Hide();
            pool.Enqueue(item);
        }

        // 清空物件池
        public void Clear()
        {
            pool.Clear();
        }
    }
}
