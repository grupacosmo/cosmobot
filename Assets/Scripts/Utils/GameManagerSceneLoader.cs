using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot.Utils
{
    public class GameManagerSceneLoader
    {
        private const string GameManagerSceneName = "GameManagerScene";

#if !UNITY_EDITOR
        private static bool loaded = false;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadGameManagerScene()
        {
#if !UNITY_EDITOR
            if (loaded) return;
            loaded = true;
#endif

            SceneManager.LoadScene(GameManagerSceneName, LoadSceneMode.Additive);
        }
    }
}
