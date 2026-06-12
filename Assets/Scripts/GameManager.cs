using System.Collections.Generic;
using System.Linq;
using Cosmobot.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot
{
    [DefaultExecutionOrder(ExecutionOrder.GameManager)]
    public class GameManager : SingletonSystem<GameManager>
    {
        private Scene? playerScene;
        private Transform playerTransform;
        private PlayerController playerController;
        private PlayerCamera playerCamera;

        public static Transform PlayerTransform => SemiSafeGetPlayer(Instance.playerTransform);
        public static PlayerController PlayerController => SemiSafeGetPlayer(Instance.playerController);
        public static PlayerCamera PlayerCamera => SemiSafeGetPlayer(Instance.playerCamera);

        public static T GetPlayerComponent<T>() where T : class
        {
            return SemiSafeGetPlayer(Instance.playerTransform.GetComponent<T>());
        }

        private static T SemiSafeGetPlayer<T>(T o) where T : class
        {
#if UNITY_EDITOR
            Scene? scene = Instance.playerScene;
            if (!scene.HasValue || !scene.Value.isLoaded || !Instance.playerTransform)
            {
                Debug.LogWarning("Accessing unloaded player!");
                return null;
            }
#endif
            return o;
        }

        protected override void SystemAwake()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
            // first setup
            OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnSceneLoad(Scene _, LoadSceneMode _1)
        {
            SetupPlayer();
        }

        void OnSceneUnload(Scene scene)
        {
            if (scene.IsValid() && scene.isLoaded && scene != playerScene) return;
            ClearPlayer();
        }

        void ClearPlayer()
        {
            playerScene = null;
            playerTransform = null;
            playerController = null;
            playerCamera = null;
        }

        void SetupPlayer()
        {
            if (playerTransform) // will also check if object is valid
            {
                return;
            }

            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(Tags.Player);
            List<GameObject> playerCandidates = playerObjects.Where(IsValidPlayerCandidate).ToList();
            if (playerCandidates.Count == 0)
            {
                return;
            }

            GameObject selectedPlayer = playerCandidates[0];
            if (playerCandidates.Count > 1)
            {
                string playerObjectsNames = playerObjects.Skip(1).Select(g => g.name).Aggregate((a, b) => $"{a}, '{b}'");
                Debug.LogWarning($"Found multiple GameObject that appear to be an Player GameObjects" +
                                 $"Using first one (click to select used object)." +
                                 $"Other found objects: {playerObjectsNames}", selectedPlayer);
            }

            playerTransform = selectedPlayer.transform;
            playerController = selectedPlayer.GetComponent<PlayerController>();
            playerCamera = selectedPlayer.GetComponentInChildren<PlayerCamera>();
            playerScene = selectedPlayer.scene; // use accusal GO scene
        }

        bool IsValidPlayerCandidate(GameObject playerCandidate)
        {
            return playerCandidate
                && playerCandidate.GetComponent<PlayerController>()
                && playerCandidate.GetComponentInChildren<PlayerCamera>();
        }
    }
}
