using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace iBMSApp.UI.Pages
{
    public class LoadingPage : MonoBehaviour
    {
        public static LoadingPage Instance { get; private set; }

        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // 若切場景還要保留
            loadingPanel.SetActive(false);
        }

        //public void UpdateProgress(float progress)
        //{
        //    if (loadingText != null)
        //    {
        //        loadingText.text = $"載入中... {Mathf.RoundToInt(progress * 100)}%";
        //    }
        //}

        /// <summary>
        /// 開始非同步載入場景，並在完成前顯示載入過場動畫
        /// </summary>
        /// <param name="sceneName"></param>
        public void StartSceneLoading(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            loadingPanel.SetActive(true);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                //float progress = Mathf.Clamp01(operation.progress / 0.9f);
                //UpdateProgress(progress);

                yield return null;
            }

            loadingPanel.SetActive(false);
        }
    }
}
