using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.Utility
{
    public class BaseListItemPool<TPoolItem, TData> : MonoBehaviour
           where TPoolItem : MonoBehaviour, IObjectPoolItem<TData>
           where TData : class 
    {
        public bool IsInitialized { get; private set; } = false;

        [SerializeField] private RectTransform _content;
        [SerializeField] private TPoolItem _prefab;

        private InterfacePool<TPoolItem, TData> _objectPool;
        private int siblingIndex = 0;

        private void InitPool()
        {
            if (_objectPool == null)
            {
                _objectPool = new InterfacePool<TPoolItem, TData>(_prefab, _content);
            }
            else
            {
                _objectPool.Clear();
            }
            siblingIndex = 0;
            IsInitialized = true;
        }

        /// <summary>
        /// �j�w�@�ӦC��쪫������A�ñN�C�Ӷ��ظj�w��������ƾڡC
        /// </summary>
        /// <param name="bindingList"></param>
        /// <param name="isDisabled"></param>
        public void BindList(List<TData> bindingList, bool isDisabled, Action<TPoolItem, TData> OnItemBound = null)
        {
            if (!IsInitialized)
                InitPool();

            ClearList();
            foreach (var bindObject in bindingList)
            {
                if (bindObject == null)
                {
                    Debug.LogWarning("Binding object is null on BaseListItemController!");
                    continue;
                }

                TPoolItem item = _objectPool.Get();

                item.Bind(bindObject, isDisabled);
                item.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;

                OnItemBound?.Invoke(item, bindObject);
            }
            RefreshLayout();
        }

        /// <summary>
        /// �j�w��Ӷ��ب쪫������A�ñN��j�w��������ƾڡC
        /// </summary>
        /// <param name="bindingObject">�n�j�w����ƪ���</param>
        /// <param name="isDisabled">�Ӷ��جO�_��Ū</param>
        /// <param name="OnItemBound">���ظj�w�����ɪ��^�I</param>
        public void BindItem(TData bindingObject, bool isDisabled, Action<TPoolItem, TData> OnItemBound = null)
        {
            if (!IsInitialized)
                InitPool();

            TPoolItem item = _objectPool.Get();

            item.Bind(bindingObject, isDisabled);
            item.transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;

            OnItemBound?.Invoke(item, bindingObject);
        }

        /// <summary>
        /// �M���C�����Ҧ����ءA�����񥦭̨쪫������C
        /// </summary>
        public void ClearList()
        {
            if (!IsInitialized)
                InitPool();

            siblingIndex = 0;
            foreach (Transform item in _content)
            {
                if (!item.gameObject.activeSelf)
                    continue;

                if (item.TryGetComponent<TPoolItem>(out var itemComponent))
                    _objectPool.Release(itemComponent);
            }
            Canvas.ForceUpdateCanvases();
        }

        public void ClearItem(TPoolItem item)
        {
            item.transform.SetSiblingIndex(_content.childCount - 1);
            _objectPool.Release(item);
        }

        public void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }
    }
}
