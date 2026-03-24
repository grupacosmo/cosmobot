using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot.Utils
{
    public class GameManagerSceneLoader
    {
        private const string GameManagerSceneName = "GameManagerScene";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadGameManagerScene()
        {
            if (SceneManager.GetSceneByName(GameManagerSceneName).isLoaded)
            {
                return;
            }

            SceneManager.LoadScene(GameManagerSceneName, LoadSceneMode.Additive);
        }
    }
}
