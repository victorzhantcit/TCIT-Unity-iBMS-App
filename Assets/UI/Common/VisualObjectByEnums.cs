using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace iBMSApp.UI.Common
{
    public abstract class VisualObjectByEnums<T> : MonoBehaviour where T : Enum
    {
        [Serializable]
        public class EnumVisualMapping
        {
            public GameObject VisualObject; // 物件
            public T[] EnumValues; // 對應的枚舉列表
        }

        [SerializeField]
        private List<GameObject> _basicVisualObjects = new List<GameObject>();

        [SerializeField]
        private List<EnumVisualMapping> _visualMappings = new List<EnumVisualMapping>();

        public List<EnumVisualMapping> VisualMappings => _visualMappings;

        private T _currentEnumValue;

        /// <summary>
        /// 獲取目前的狀態
        /// </summary>
        public T CurrentEnumValue => _currentEnumValue;

        /// <summary>
        /// 初始化到預設值
        /// </summary>
        [SerializeField]
        private T _defaultEnumValue;

        protected virtual void Start()
        {
            // 初始化到預設值
            SetEnumValue(_defaultEnumValue);
        }

        protected virtual void ActivateBasicVisuals(bool enabled)
        {
            foreach (var visual in _basicVisualObjects)
            {
                visual.SetActive(enabled);
            }
        }

        /// <summary>
        /// 切換到指定的枚舉值
        /// </summary>
        public virtual void SetEnumValue(T enumValue, bool enable = true, bool hideOthers = true)
        {
            // 禁用所有物件
            if (hideOthers)
                for (int i = 0; i < _visualMappings.Count; i++)
                    ActivateVisualObject(_visualMappings[i].VisualObject, false);

            // 查找與當前枚舉值相關的物件並啟用
            foreach (EnumVisualMapping mapping in _visualMappings)
            {
                if (mapping == null && mapping.VisualObject == null)
                    continue;
                //Debug.Log($"{mapping.VisualObject.name}: {string.Join(",", mapping.EnumValues.Select(x => x.ToString()))}");
                bool visibleState = mapping.EnumValues.Contains(enumValue);
                ActivateVisualObject(mapping.VisualObject, visibleState);
            }

            // 更新當前枚舉值
            _currentEnumValue = enumValue;
        }

        /// <summary>
        /// 禁用所有註冊的物件
        /// </summary>
        public void ActivateAll(bool enable)
        {
            for (int i = 0; i < _visualMappings.Count; i++)
                ActivateVisualObject(_visualMappings[i].VisualObject, enable);
        }

        /// <summary>
        /// 啟用或禁用對應的物件
        /// </summary>
        protected void ActivateVisualObject(GameObject visualObject, bool active)
        {
            if (visualObject == null) return;

            visualObject.SetActive(active);
        }
    }
}
