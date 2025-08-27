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
            public GameObject VisualObject; // ����
            public T[] EnumValues; // �������T�|�C��
        }

        [SerializeField]
        private List<GameObject> _basicVisualObjects = new List<GameObject>();

        [SerializeField]
        private List<EnumVisualMapping> _visualMappings = new List<EnumVisualMapping>();

        public List<EnumVisualMapping> VisualMappings => _visualMappings;

        private T _currentEnumValue;

        /// <summary>
        /// ����ثe�����A
        /// </summary>
        public T CurrentEnumValue => _currentEnumValue;

        /// <summary>
        /// ��l�ƨ�w�]��
        /// </summary>
        [SerializeField]
        private T _defaultEnumValue;

        protected virtual void Start()
        {
            // ��l�ƨ�w�]��
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
        /// ��������w���T�|��
        /// </summary>
        public virtual void SetEnumValue(T enumValue, bool enable = true, bool hideOthers = true)
        {
            // �T�ΩҦ�����
            if (hideOthers)
                for (int i = 0; i < _visualMappings.Count; i++)
                    ActivateVisualObject(_visualMappings[i].VisualObject, false);

            // �d��P��e�T�|�Ȭ���������ñҥ�
            foreach (EnumVisualMapping mapping in _visualMappings)
            {
                if (mapping == null && mapping.VisualObject == null)
                    continue;
                //Debug.Log($"{mapping.VisualObject.name}: {string.Join(",", mapping.EnumValues.Select(x => x.ToString()))}");
                bool visibleState = mapping.EnumValues.Contains(enumValue);
                ActivateVisualObject(mapping.VisualObject, visibleState);
            }

            // ��s��e�T�|��
            _currentEnumValue = enumValue;
        }

        /// <summary>
        /// �T�ΩҦ����U������
        /// </summary>
        public void ActivateAll(bool enable)
        {
            for (int i = 0; i < _visualMappings.Count; i++)
                ActivateVisualObject(_visualMappings[i].VisualObject, enable);
        }

        /// <summary>
        /// �ҥΩθT�ι���������
        /// </summary>
        protected void ActivateVisualObject(GameObject visualObject, bool active)
        {
            if (visualObject == null) return;

            visualObject.SetActive(active);
        }
    }
}
