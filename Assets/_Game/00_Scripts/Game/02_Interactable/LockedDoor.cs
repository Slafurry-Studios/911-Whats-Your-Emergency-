using UnityEngine;
using Slafurry.Core.Interface;

/// <summary>The front exit: always locked. Interacting just tells the player so.</summary>
public class LockedDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string _prompt = "Try the door";
    [SerializeField] private string _message = "It's locked. The police are my only way out.";

    public string InteractionPrompt => _prompt;
    public bool CanInteract => true;

    public void Interact() => Slafurry.UI.HUD.ObjectiveHUD.Toast(_message);
}
