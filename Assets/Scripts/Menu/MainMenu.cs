using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cosmobot
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject loadingScreen;
        public Slider slider;
        public TMP_Text progressText;

        public void Quit()
        {
            Application.Quit();
        }

        public void LoadLevel(int sceneIndex)
        {
            StartCoroutine(LoadAsynchronously(sceneIndex));
        }

        IEnumerator LoadAsynchronously(int sceneIndex)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            loadingScreen.SetActive(true);
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / .9f);
                slider.value = progress;
                progressText.text = progress * 100f + "%";
                yield return null;
            }
        }
    }
}