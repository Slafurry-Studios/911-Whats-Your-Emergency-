using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Slafurry.Systems.Pause;
using Slafurry.UI.HUD;

/// <summary>
/// First-round tutorial, one thing at a time:
/// 1 move → 2 pick up an item → 3 hide (and come out) → 4 find the old phone → 5 call 911 →
/// 6 grab the charger and plug it in at an outlet → 7 throw something at the kidnapper.
/// The kidnapper is held asleep until step 7. The tutorial panel is the only text it uses
/// (no toasts on top), and it steps aside while the charging screen shows its own guidance.
/// Done once (saved); Enter skips it.
/// </summary>
public class Onboarding : MonoBehaviour
{
    private const string DoneKey = "911_TutorialDone";
    private const int Pages = 7;

    [SerializeField] private float _moveDistance = 4f;
    [SerializeField] private float _slowMoScale = 0.3f;
    [SerializeField] private float _slowMoSeconds = 1.4f;

    private enum Step { Move, Pickup, Hide, Unhide, Phone, Call, Charger, Plug, Charging, Throw, Done }

    private Step _step = Step.Done;
    private KidnapperBrain _kidnapper;
    private Transform _player;
    private Vector2 _lastPos;
    private float _walked;
    private bool _itemPicked;
    private bool _spotted;
    private bool _hit;
    private bool _callAfterCharge;
    private ItemData _cable;
    private Transform _phoneItem, _cableItem;
    private readonly List<Component> _pulsing = new();

    public static bool IsDone => PlayerPrefs.GetInt(DoneKey, 0) == 1;

    /// <summary>Forget that the tutorial was completed (e.g. from a menu option).</summary>
    public static void Reset() => PlayerPrefs.DeleteKey(DoneKey);

    private IEnumerator Start()
    {
        if (IsDone) yield break;

        KidnapperBrain.HoldAsleep = true;
        // Wait for the scene's singletons.
        while (PlayerInteract.Instance == null || PlayerInventory.Instance == null || PlayerHide.Instance == null)
            yield return null;

        _player = PlayerInteract.Instance.transform;
        _lastPos = _player.position;
        _kidnapper = FindObjectOfType<KidnapperBrain>();

        // Remember the phone and the charger lying in the house (their spots are random).
        foreach (var item in FindObjectsOfType<InteractItem>())
        {
            if (item.Item == null || !item.Item.IsEssential) continue;
            if (item.name.Contains("Phone")) _phoneItem = item.transform;
            else if (item.name.Contains("Cable") || item.name.Contains("Charg"))
            {
                _cableItem = item.transform;
                _cable = item.Item;
            }
        }

        PlayerInventory.Instance.OnItemAdded += OnItemAdded;
        if (_kidnapper != null)
        {
            _kidnapper.OnSpottedPlayer += OnSpotted;
            _kidnapper.OnHitByItem += OnKidnapperHit;
        }
        Enter(Step.Move);
    }

    private bool HasPhone => PhonePresence.Instance != null && PhonePresence.Instance.HasPhone;
    private bool HasCable => _cable != null && PlayerInventory.Instance.HasItem(_cable);
    private bool CallStarted => PoliceArrival.Instance != null && PoliceArrival.Instance.CallStarted;
    private bool IsCharging => PhoneCharging.Instance != null && PhoneCharging.Instance.IsCharging;

    private void Update()
    {
        if (_step == Step.Done) return;

        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ObjectiveHUD.Toast("Tutorial skipped.");
            Finish();
            return;
        }

        switch (_step)
        {
            case Step.Move:
                Vector2 p = _player.position;
                _walked += Vector2.Distance(p, _lastPos);
                _lastPos = p;
                if (_walked >= _moveDistance) Enter(Step.Pickup);
                break;

            case Step.Pickup:
                if (_itemPicked) Enter(Step.Hide);
                break;

            case Step.Hide:
                if (PlayerHide.Instance.IsHidden) Enter(Step.Unhide);
                break;

            case Step.Unhide:
                if (!PlayerHide.Instance.IsHidden) Enter(HasPhone ? Step.Call : Step.Phone);
                break;

            case Step.Phone:
                if (HasPhone) Enter(Step.Call);
                break;

            case Step.Call:
                if (CallStarted) Enter(HasCable ? Step.Plug : Step.Charger);
                else if (PhoneCall.Instance != null && !PhoneCall.Instance.CanCall)
                {
                    // Dead phone: charge it first, then come back to the call.
                    _callAfterCharge = true;
                    Enter(HasCable ? Step.Plug : Step.Charger);
                }
                break;

            case Step.Charger:
                if (HasCable) Enter(Step.Plug);
                break;

            case Step.Plug:
                if (IsCharging) Enter(Step.Charging);
                break;

            case Step.Charging:
                // The charging screen explains itself; continue once the player closes it.
                if (!IsCharging)
                {
                    if (_callAfterCharge && !CallStarted) { _callAfterCharge = false; Enter(Step.Call); }
                    else Enter(Step.Throw);
                }
                break;

            case Step.Throw:
                bool hasItem = PlayerInventory.Instance.OtherItems.Any(i => i != null && i.IsThrowable);
                Show(7, "Fight back",
                    _spotted ? "He's seen you! Throw something at him!" : "Footsteps... He's awake, and faster than you.",
                    hasItem ? "Drag an item from the hotbar onto him" : "Grab a rock, ball or pot first [E]");
                if (_hit) Finish("Got him! Now survive until help arrives.");
                break;
        }
    }

    private void Enter(Step step)
    {
        _step = step;
        ClearPulses();
        switch (step)
        {
            case Step.Move:
                Show(1, "Move", "Walk around with [W][A][S][D].", "[Shift] sprint  ·  [Enter] skip tutorial");
                break;

            case Step.Pickup:
                Show(2, "Items", "Pick up something you can throw.", "Walk to a glowing item, press [E]");
                foreach (var item in NearestItems(3)) Pulse(item);
                break;

            case Step.Hide:
                Show(3, "Hide", "Hide behind a purple curtain.", "Stand next to it, press [E]");
                var spot = HideArea.All.Where(h => h != null && !h.IsBroken)
                    .OrderBy(h => Vector2.Distance(h.transform.position, _player.position)).FirstOrDefault();
                if (spot != null) Pulse(spot);
                break;

            case Step.Unhide:
                Show(3, "Hide", "Hidden. Unless he saw you go in, you're safe.", "[E] come out");
                break;

            case Step.Phone:
                Show(4, "Phone", "Find the old phone. It's your only way home.", "It's somewhere in this house, press [E] on it");
                if (_phoneItem != null && _phoneItem.gameObject.activeInHierarchy) Pulse(_phoneItem);
                break;

            case Step.Call:
                Show(5, "Call 911", "Open the phone with [Tab] and press Call.", "Once you call, the police start coming");
                break;

            case Step.Charger:
                Show(6, "Battery", "Your battery drains. Find the charging cable.", "A dead phone makes the police slower");
                if (_cableItem != null && _cableItem.gameObject.activeInHierarchy) Pulse(_cableItem);
                break;

            case Step.Plug:
                Show(6, "Battery", "Plug the cable in at a wall outlet.", "Walk to a glowing outlet, press [E]");
                foreach (var o in ChargingOutlet.All.Where(o => o != null && !o.IsBroken)
                             .OrderBy(o => Vector2.Distance(o.transform.position, _player.position)).Take(2))
                    Pulse(o);
                break;

            case Step.Charging:
                ObjectiveHUD.ClearTutorial(); // the charging screen has its own hints
                break;

            case Step.Throw:
                KidnapperBrain.HoldAsleep = false;
                if (_kidnapper != null) _kidnapper.WakeUp();
                var slot = GameObject.Find("=== Item Hotbar ===");
                if (slot != null) Pulse(slot.transform);
                break;
        }
    }

    private static void Show(int page, string title, string text, string hint) =>
        ObjectiveHUD.SetTutorial($"Tutorial {page}/{Pages} · {title}", text, hint);

    private void OnItemAdded(ItemData item)
    {
        if (item != null && item.IsThrowable) _itemPicked = true;
    }

    private void OnKidnapperHit(Vector2 at) => _hit = true;

    private void OnSpotted()
    {
        if (_step != Step.Throw || _spotted) return;
        _spotted = true;
        StartCoroutine(SlowMo());
    }

    private IEnumerator SlowMo()
    {
        if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) yield break;
        Time.timeScale = _slowMoScale;
        yield return new WaitForSecondsRealtime(_slowMoSeconds);
        if (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused) Time.timeScale = 1f;
    }

    private void Finish(string message = null)
    {
        // Skipped before he appeared: give the player the usual grace period from now.
        if (_step < Step.Throw && _kidnapper != null) _kidnapper.RestartWakeTimer();
        _step = Step.Done;
        ClearPulses();
        KidnapperBrain.HoldAsleep = false;
        PlayerPrefs.SetInt(DoneKey, 1);
        PlayerPrefs.Save();
        ObjectiveHUD.ClearTutorial();
        if (message != null) ObjectiveHUD.Toast(message);
    }

    private IEnumerable<InteractItem> NearestItems(int count) =>
        FindObjectsOfType<InteractItem>()
            .Where(i => i.CanInteract && i.Item != null && i.Item.IsThrowable)
            .OrderBy(i => Vector2.Distance(i.transform.position, _player.position))
            .Take(count);

    private void Pulse(Component c)
    {
        TutorialPulse.On(c);
        _pulsing.Add(c);
    }

    private void ClearPulses()
    {
        foreach (var c in _pulsing) TutorialPulse.Off(c);
        _pulsing.Clear();
    }

    private void OnDestroy()
    {
        KidnapperBrain.HoldAsleep = false;
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.OnItemAdded -= OnItemAdded;
        if (_kidnapper != null)
        {
            _kidnapper.OnSpottedPlayer -= OnSpotted;
            _kidnapper.OnHitByItem -= OnKidnapperHit;
        }
    }
}
