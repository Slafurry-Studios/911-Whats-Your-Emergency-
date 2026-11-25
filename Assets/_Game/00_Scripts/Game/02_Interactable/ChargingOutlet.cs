using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Interface;

public class ChargingOutlet : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData _chargingCable;
    [SerializeField] private string _interactionPrompt = "Charge Phone";
    [Tooltip("Sprite after the kidnapper wrecks it (tint is used if empty).")]
    [SerializeField] private Sprite _brokenSprite;
    [SerializeField] private Color _brokenColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    /// <summary>All enabled outlets (the kidnapper picks sabotage targets from here).</summary>
    public static readonly List<ChargingOutlet> All = new();

    /// <summary>The outlet the current charging session started from.</summary>
    public static ChargingOutlet InUse { get; private set; }

    public bool IsBroken { get; private set; }

    public string InteractionPrompt => _interactionPrompt;

    public bool CanInteract
    {
        get
        {
            if (IsBroken) return false;
            if (PhonePresence.Instance != null && !PhonePresence.Instance.HasPhone) return false;
            if (PhoneCharging.Instance != null && PhoneCharging.Instance.IsCharging) return false;
            if (_chargingCable == null) return false;

            var inventory = PlayerInventory.Instance;
            return inventory != null && inventory.HasItem(_chargingCable);
        }
    }

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    private void OnDisable() => All.Remove(this);

    public void Interact()
    {
        if (!CanInteract) return;

        InUse = this;
        PhoneCharging.Instance?.Begin();
    }

    /// <summary>The kidnapper destroyed this outlet: stops a charge in progress here; it can't be used again.</summary>
    public void Break()
    {
        if (IsBroken) return;
        IsBroken = true;

        if (InUse == this && PhoneCharging.Instance != null && PhoneCharging.Instance.IsCharging)
            PhoneCharging.Instance.End();

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (_brokenSprite != null) sr.sprite = _brokenSprite;
            else sr.color = _brokenColor;
        }

        // Turn off this outlet's glow light(s).
        foreach (var light in FindObjectsOfType<UnityEngine.Rendering.Universal.Light2D>())
            if (Vector2.Distance(light.transform.position, transform.position) < 0.75f)
                light.enabled = false;
    }
}
