using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(Collider))]
    public class RobotUiInteractionHandler : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private ProgrammingUiManager programmingUiManager;

        private Programmable robot;
        
        public string Prompt { get; private set; } = "Press 'E' to interact";

        private void Start()
        {
            robot = GetComponent<Programmable>();
        }

        public void Use()
        {
            programmingUiManager.OpenUI(robot);
        }
    }
}
