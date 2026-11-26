using System.Collections;
using TMPro;
using UnityEngine;

namespace Slafurry.UI.HUD
{
    /// <summary>
    /// Tells the player what to do: the current objective (from game state), short toast
    /// messages (locked door, low battery, police progress) and a controls hint at the start.
    /// </summary>
    public class ObjectiveHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _objective;
        [SerializeField] private TMP_Text _toast;
        [SerializeField] private TMP_Text _controls;
        [SerializeField] private float _toastSeconds = 3f;
        [SerializeField] private float _controlsSeconds = 14f;

        private static ObjectiveHUD _instance;
        private Coroutine _toastRoutine;
        private string _lastObjective;

        // Tutorial override (Onboarding): two-line panel + the hint line under it.
        private string _tutorialTitle, _tutorialText, _tutorialHint;
        private bool TutorialActive => _tutorialText != null;
        private RectTransform _panel;
        private float _panelHeight;
        private Vector2 _toastPos, _controlsPos;
        private string _controlsDefault;
        private Coroutine _hideControls;

        public static void SetTutorial(string title, string text, string hint)
        {
            if (_instance == null) return;
            if (_instance._tutorialTitle == title && _instance._tutorialText == text && _instance._tutorialHint == hint) return;
            _instance._tutorialTitle = title;
            _instance._tutorialText = text;
            _instance._tutorialHint = hint;
            _instance.ApplyLayout();
        }

        public static void ClearTutorial()
        {
            if (_instance == null) return;
            _instance._tutorialTitle = _instance._tutorialText = _instance._tutorialHint = null;
            _instance.ApplyLayout();
        }

        private static readonly string[] PoliceMessages =
        {
            "Dispatch: help is on the way.",
            "The police are getting closer.",
            "Sirens in the distance...",
            "Almost here. Hold on!",
            "They're outside!",
        };

        /// <summary>Show a short message under the objective.</summary>
        public static void Toast(string message) { if (_instance != null) _instance.ShowToast(message); }

        private void Awake()
        {
            _instance = this;
            if (_toast != null) { _toast.alpha = 0f; _toastPos = _toast.rectTransform.anchoredPosition; }
            if (_objective != null && _objective.transform.parent is RectTransform panel)
            {
                _panel = panel;
                _panelHeight = panel.sizeDelta.y;
            }
            if (_controls != null) { _controlsPos = _controls.rectTransform.anchoredPosition; _controlsDefault = _controls.text; }

            // The first-round tutorial lives with the objective display.
            if (FindObjectOfType<Onboarding>() == null) gameObject.AddComponent<Onboarding>();
        }

        private void ApplyLayout()
        {
            const float extra = 24f;
            if (_panel != null) _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, _panelHeight + (TutorialActive ? extra : 0f));
            if (_toast != null) _toast.rectTransform.anchoredPosition = _toastPos + Vector2.down * (TutorialActive ? extra : 0f);
            if (_controls != null)
            {
                _controls.rectTransform.anchoredPosition = _controlsPos + Vector2.down * (TutorialActive ? extra : 0f);
                if (TutorialActive)
                {
                    if (_hideControls != null) { StopCoroutine(_hideControls); _hideControls = null; }
                    _controls.gameObject.SetActive(true);
                    _controls.alpha = 1f;
                    _controls.text = _tutorialHint;
                }
                else if (_controls.text != _controlsDefault)
                {
                    _controls.text = _controlsDefault; // tutorial over: show the full controls once more
                    _hideControls = StartCoroutine(HideControlsLater());
                }
            }
            _lastObjective = null; // force refresh
        }

        private void OnEnable()
        {
            PoliceRadio.OnMilestone += OnPoliceMilestone;
            PhoneAudio.OnLowBattery += OnLowBattery;
        }

        private void OnDisable()
        {
            PoliceRadio.OnMilestone -= OnPoliceMilestone;
            PhoneAudio.OnLowBattery -= OnLowBattery;
        }

        private void OnDestroy() { if (_instance == this) _instance = null; }

        [ContextMenu("Reset tutorial (play it again next round)")]
        private void ResetTutorial() => Onboarding.Reset();

        private void Start()
        {
            if (_controls != null && !TutorialActive) _hideControls = StartCoroutine(HideControlsLater());
        }

        private void Update()
        {
            string text = TutorialActive
                ? $"<size=75%><color=#E3B341>{_tutorialTitle}</color></size>\n{_tutorialText}"
                : CurrentObjective();
            if (text == _lastObjective || _objective == null) return;
            _lastObjective = text;
            _objective.text = text;
        }

        private static string CurrentObjective()
        {
            if (GameOutcome.IsFinished) return "";
            bool hasPhone = PhonePresence.Instance != null && PhonePresence.Instance.HasPhone;
            if (!hasPhone) return "Find your phone.";

            var police = PoliceArrival.Instance;
            if (police == null || !police.CallStarted)
            {
                bool canCall = PhoneCall.Instance == null || PhoneCall.Instance.CanCall;
                return canCall
                    ? "Open your phone [Tab] and call 911."
                    : "Your phone is dead. Find a charger and an outlet.";
            }
            return $"Survive until the police arrive. ({police.Progress * 100f:F0}%)";
        }

        private void OnPoliceMilestone(int index) =>
            ShowToast(PoliceMessages[Mathf.Clamp(index, 0, PoliceMessages.Length - 1)]);

        private void OnLowBattery() => ShowToast("Battery low. Find the charger and an outlet.");

        private void ShowToast(string message)
        {
            if (_toast == null) return;
            if (_toastRoutine != null) StopCoroutine(_toastRoutine);
            _toastRoutine = StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            _toast.text = message;
            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime) { _toast.alpha = t / 0.2f; yield return null; }
            _toast.alpha = 1f;
            yield return new WaitForSecondsRealtime(_toastSeconds);
            for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime) { _toast.alpha = 1f - t / 0.6f; yield return null; }
            _toast.alpha = 0f;
            _toastRoutine = null;
        }

        private IEnumerator HideControlsLater()
        {
            yield return new WaitForSeconds(_controlsSeconds);
            if (TutorialActive) yield break;
            for (float t = 0f; t < 1f; t += Time.deltaTime) { _controls.alpha = 1f - t; yield return null; }
            if (!TutorialActive) _controls.gameObject.SetActive(false);
            _hideControls = null;
        }
    }
}
