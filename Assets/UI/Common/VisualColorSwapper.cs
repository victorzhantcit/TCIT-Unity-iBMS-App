using System;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Common
{
    public class VisualColorSwapper : MonoBehaviour
    {
        [Serializable]
        public class GrapicColorOption
        {
            public Graphic RenderObject;
            public Color ActivatedColor;
            public Color DeactivatedColor;
        }

        [SerializeField] private GrapicColorOption[] _colorConfigs;
        [SerializeField] private ToggleState _isActivated = ToggleState.Deactivated;

        private void Start()
        {
            ApplyConfig(_isActivated);
        }

        public void SetConfigStatus(ToggleState activated)
        {
            if (activated == _isActivated)
                return;

            _isActivated = activated;
            ApplyConfig(activated);
        }

        private void ApplyConfig(ToggleState activated)
        {
            foreach (var config in _colorConfigs)
            {
                if (config.RenderObject == null) continue;
                config.RenderObject.color = (_isActivated == ToggleState.Activated)
                    ? config.ActivatedColor 
                    : config.DeactivatedColor;
            }
            _isActivated = activated;
        }
    }
}

