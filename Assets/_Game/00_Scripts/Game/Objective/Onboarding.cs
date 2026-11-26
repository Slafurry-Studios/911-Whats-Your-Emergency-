using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Slafurry.Systems.Pause;
using Slafurry.UI.HUD;

/// <summary>
/// First-round tutorial, one thing at a time:
/// 1 move → 2 pick up an item → 3 hide (and come out) → 4 throw something at the kidnapper.
/// The kidnapper is held asleep for steps 1–3, then woken; when he first spots the player time
/// slows for a moment and the hotbar pulses. Done once (saved); Enter skips it.
/// </summary>
public class Onboarding : MonoBehaviour
{
    private const string DoneKey = "911_TutorialDone";

    [SerializeField] private float _moveDistance = 4f;
    [SerializeField] private float _slowMoScale = 0.3f;
    [SerializeField] private float _slowMoSeconds = 1.4f;

    private enum Step { Move, Pickup, Hide, Unhide, Throw, Done }

    private Step _step = Step.Done;
    private KidnapperBrain _kidnapper;
    private Transform _player;
    private Vector2 _lastPos;
    private float _walked;
    private bool _itemPicked;
    private bool _spotted;
    private bool _hit;
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
        PlayerInventory.Instance.OnItemAdded += OnItemAdded;
        if (_kidnapper != null)
        {
            _kidnapper.OnSpottedPlayer += OnSpotted;
            _kidnapper.OnHitByItem += OnKidnapperHit;
        }
        Enter(Step.Move);
    }

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
                if (!PlayerHide.Instance.IsHidden) Enter(Step.Throw);
                break;

            case Step.Throw:
                bool hasItem = PlayerInventory.Instance.OtherItems.Any(i => i != null && i.IsThrowable);
                ObjectiveHUD.SetTutorial("Tutorial 4/4 · Fight back",
                    _spotted ? "He's seen you! Throw something at him!" : "Someone is coming... and he is faster than you.",
                    hasItem
                        ? "Drag an item from your hotbar onto him to slow him down, then run."
                        : "Grab something to throw first: walk up to a rock, ball or pot and press [E].");
                if (_hit) Finish("Nice hit! He's slowed down. Now find your phone and call 911.");
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
                ObjectiveHUD.SetTutorial("Tutorial 1/4 · Move", "Walk around with [W][A][S][D].",
                    "Hold [Shift] to sprint (uses stamina).   ·   [Enter] skip tutorial");
                break;

            case Step.Pickup:
                ObjectiveHUD.SetTutorial("Tutorial 2/4 · Find items", "Find something you can pick up.",
                    "Walk up to a pulsing item and press [E]. Rocks, balls and pots go in your hotbar.");
                foreach (var item in NearestItems(3)) Pulse(item);
                break;

            case Step.Hide:
                ObjectiveHUD.Toast("Got it!");
                ObjectiveHUD.SetTutorial("Tutorial 3/4 · Hide", "Hide behind a purple curtain.",
                    "Stand next to a curtain and press [E]. He can't see you there... unless he watched you go in.");
                var spot = HideArea.All.Where(h => h != null && !h.IsBroken)
                    .OrderBy(h => Vector2.Distance(h.transform.position, _player.position)).FirstOrDefault();
                if (spot != null) Pulse(spot);
                break;

            case Step.Unhide:
                ObjectiveHUD.SetTutorial("Tutorial 3/4 · Hide", "You're hidden. Nobody can see you in here.",
                    "Press [E] again to come out.");
                break;

            case Step.Throw:
                KidnapperBrain.HoldAsleep = false;
                if (_kidnapper != null) _kidnapper.WakeUp();
                ObjectiveHUD.Toast("Did you hear that?");
                var slot = GameObject.Find("=== Item Hotbar ===");
                if (slot != null) Pulse(slot.transform);
                break;
        }
    }

    private void OnItemAdded(ItemData item) => _itemPicked = true;

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
