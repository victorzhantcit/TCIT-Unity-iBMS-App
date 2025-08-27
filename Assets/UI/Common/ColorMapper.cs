using System.Collections.Generic;
using UnityEngine;

namespace iBMSApp.UI.Common
{
    public class ColorMapper : MonoBehaviour
    {
        public static ColorMapper Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private ColorMappingData mappingData;

        private Dictionary<string, Color> _colorDict;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isUpdating)
            {
                Debug.LogWarning("ColorMapper�GEditor ���b��s�귽�A�����l��");
                return;
            }
#endif
            InitColorDictionary();  
        }
         
        private void InitColorDictionary()
        {
            _colorDict = new Dictionary<string, Color>();
            foreach (var entry in mappingData.colorMappings)
            {
                foreach (var name in entry.names)
                {
                    if (!string.IsNullOrEmpty(name))
                        _colorDict[name] = entry.color;
                }
            }
        }

        public Color GetColor(string name)
        {
            if (_colorDict == null)
            {
                Debug.LogWarning("Color dictionary �|����l�ơI");
                return Color.white;
            }

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("ColorMapper�G�ǤJ���W�٬��ũ� null�A��^�w�]�C��");
                return Color.white;
            }

            return _colorDict.TryGetValue(name, out var color) ? color : Color.white;
        }
    }
}
