namespace Cosmobot
{
    public interface IInteractable
    {
        public string Prompt { get; }

        public void Use();
    }
}
