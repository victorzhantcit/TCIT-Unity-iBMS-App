using System;
using System.Collections;
using UnityEngine;

namespace iBMSApp.Services
{
    public class UnityToastService : MonoBehaviour, IToastService
    {
        public static UnityToastService Instance { get; private set; }

        public event Action<string, ToastLevel> OnShow = null!;
        public event Action OnHide = null!;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // 若切場景還要保留
        }

        public void ShowToast(string message, ToastLevel level)
        {
            OnShow?.Invoke(message, level);

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(2.5f);
            OnHide?.Invoke();
        }

        public void Dispose()
        {
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
        }
    }
}
