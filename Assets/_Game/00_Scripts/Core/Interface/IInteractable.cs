namespace Slafurry.Core.Interface
{
    public interface IInteractable
    {
        void Interact();
        string InteractionPrompt { get; }
        bool CanInteract { get; }
    }
}
