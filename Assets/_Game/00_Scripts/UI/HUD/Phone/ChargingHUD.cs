using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Slafurry.Systems.InputHub;
using Slafurry.Utils.UI;

namespace Slafurry.UI.HUD.Phone
{
    public class ChargingHUD : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _batteryText;
        [SerializeField] private Slider _batterySlider; // display-only battery bar (0-100)
        [SerializeField] private TMP_Text _ampereText;
        [SerializeField] private UIDragHandler _cable;
        [SerializeField] private RectTransform _inputSlot;
        [SerializeField] private RectTransform _dragParent; // parent to keep cable under while dragged
        [SerializeField] private Button _closeButton;

        [Header("Cable Facing")]
        [Tooltip("Degrees added when cable looks at input. 0 = art points right, -90 = art points up.")]
        [SerializeField] private float _facingOffsetDegrees = 0f;

        [Header("Ease of use")]
        [Tooltip("Drop the cable at least this close (canvas units) to plug it in (PhoneData's radius if larger).")]
        [SerializeField] private float _minSnapRadius = 110f;
        [Tooltip("While plugged in the cable stays on a circle this far from the port: just move it around.")]
        [SerializeField] private float _wiggleRadius = 70f;
        [Tooltip("Charge (0..1 of max) that counts as 'strong'.")]
        [SerializeField, Range(0f, 1f)] private float _strongCharge = 0.75f;
        [Tooltip("Ignore E/Esc this long after opening (the E that opened it).")]
        [SerializeField] private float _exitGrace = 0.35f;

        [Header("Guidance text (created at runtime when empty)")]
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private TMP_Text _footerText;
        [SerializeField] private Color _weakColor = new Color32(214, 69, 69, 255);
        [SerializeField] private Color _okColor = new Color32(227, 179, 65, 255);
        [SerializeField] private Color _strongColor = new Color32(110, 210, 110, 255);
        [SerializeField] private Color _portHighlight = new Color(0.4f, 1f, 0.5f, 0.35f);

        private float _openedAt;
        private Image _slotImage;
        private Color _slotColor;

        private bool _sessionSubscribed;
        private bool _batterySubscribed;
        private RectTransform _cableRect;
        private Vector2 _homePos;
        private Quaternion _homeRot;
        private Transform _homeParent;
        private Canvas _canvas;
        private void Awake()
        {
            _cableRect = (RectTransform)_cable.transform;
            _homePos = _cableRect.anchoredPosition;
            _homeRot = _cableRect.localRotation;
            _homeParent = _cableRect.parent;
            _canvas = GetComponentInParent<Canvas>();

            _cable.onDragEnded.AddListener(OnDragEnded);
            _cable.onDragStarted.AddListener(OnDragStarted);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            _slotImage = _inputSlot != null ? _inputSlot.GetComponent<Image>() : null;
            if (_slotImage != null) _slotColor = _slotImage.color;
            CreateGuidanceText();

            if (_panel != null) _panel.SetActive(false);
        }

        // Hint (what to do now) at the top of the panel and the exit shortcut at the bottom.
        private void CreateGuidanceText()
        {
            if (_panel == null) return;
            var panel = (RectTransform)_panel.transform;
            if (_hintText == null)
                _hintText = MakeText(panel, "ChargingHint", new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(900f, 70f), 28f);
            if (_footerText == null)
            {
                _footerText = MakeText(panel, "ChargingFooter", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(900f, 40f), 22f);
                _footerText.text = "[E] or [Esc]  stop charging";
                _footerText.color = new Color(1f, 1f, 1f, 0.7f);
            }
        }

        private TMP_Text MakeText(RectTransform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, anchor.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var t = go.GetComponent<TextMeshProUGUI>();
            if (_ampereText != null) { t.font = _ampereText.font; t.fontSharedMaterial = _ampereText.fontSharedMaterial; }
            t.fontSize = fontSize;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            t.raycastTarget = false;
            t.color = Color.white;
            return t;
        }

        private void Update()
        {
            if (!_sessionSubscribed && PhoneCharging.Instance != null)
            {
                PhoneCharging.Instance.OnSessionChanged += OnSessionChanged;
                if (PhoneCharging.Instance.IsCharging)
                    OnSessionChanged(true);
                _sessionSubscribed = true;
            }

            if (!_batterySubscribed && PhoneBattery.Instance != null)
            {
                PhoneBattery.Instance.OnChanged += OnBatteryChanged;
                OnBatteryChanged(PhoneBattery.Instance.Percent);
                _batterySubscribed = true;
            }

            if (_panel == null || !_panel.activeSelf) return;

            // Exit shortcut (the gameplay map is off while charging, so read the keys directly).
            var kb = Keyboard.current;
            if (kb != null && Time.unscaledTime - _openedAt > _exitGrace &&
                (kb.eKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
            {
                Close();
                return;
            }

            KeepOnWiggleCircle();
            FaceInput();
            RefreshAmpereFromCable();
            RefreshAmpereText();
            RefreshGuidance();
        }

        private float CanvasScale => _canvas != null ? _canvas.rootCanvas.scaleFactor : 1f;

        // Plugged in: the cable can only move around the port, so wiggling is just circling the mouse.
        private void KeepOnWiggleCircle()
        {
            var charging = PhoneCharging.Instance;
            if (charging == null || !charging.IsConnected) return;

            Vector2 slot = _inputSlot.position;
            Vector2 offset = (Vector2)_cableRect.position - slot;
            Vector2 dir = offset.sqrMagnitude > 0.0001f ? offset.normalized : Vector2.down;
            _cableRect.position = slot + dir * (_wiggleRadius * CanvasScale);
        }

        private void RefreshGuidance()
        {
            var charging = PhoneCharging.Instance;
            bool connected = charging != null && charging.IsConnected;
            float strength = connected && charging.Data != null && charging.Data.MaxAmpere > 0f
                ? charging.Ampere / charging.Data.MaxAmpere
                : 0f;

            if (_hintText != null)
            {
                if (!connected)
                    _hintText.text = "Drag the cable into the phone's charging port.";
                else if (strength >= _strongCharge)
                    _hintText.text = "Strong charge! Hold it there.";
                else
                    _hintText.text = $"Wiggle the cable around the port to find a stronger charge.  ({strength * 100f:F0}%)";
            }

            // Show where the port is until the cable is in.
            if (_slotImage != null)
            {
                if (connected) _slotImage.color = _slotColor;
                else
                {
                    var c = _portHighlight;
                    c.a *= 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 6f);
                    _slotImage.color = c;
                }
            }
        }

        private void OnSessionChanged(bool open)
        {
            if (_panel != null) _panel.SetActive(open);
            if (open) _openedAt = Time.unscaledTime;
            else if (_slotImage != null) _slotImage.color = _slotColor;

            RestoreCable();

            if (open) Controls.DisableInput();
            else Controls.EnableInput();
        }

        private void Close()
        {
            PhoneCharging.Instance?.End();
        }

        private void OnDragEnded(Vector2 screenPosition)
        {
            var charging = PhoneCharging.Instance;
            if (charging == null || !charging.IsCharging)
            {
                RestoreCable();
                return;
            }

            // already plugged: keep cable where dropped, stay connected
            if (charging.IsConnected) return;

            float snapRadius = Mathf.Max(SnapRadius, _minSnapRadius);
            float distance = DistanceToSlot(screenPosition);

            if (distance <= snapRadius)
            {
                // Plug in where it was dropped: on the wiggle circle, in the drop direction.
                Vector2 slot = _inputSlot.position;
                Vector2 dir = screenPosition - slot;
                dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.down;
                _cableRect.position = slot + dir * (_wiggleRadius * CanvasScale);
                charging.Connect();
                RefreshAmpereFromCable();
            }
            else
            {
                RestoreCable(); // missed: back to where it started, easy to grab again
            }
        }

        /// <summary>Keep cable behind other UI while being dragged, under its set parent.</summary>
        private void OnDragStarted()
        {
            if (_cableRect == null) return;

            var parent = _dragParent != null ? _dragParent : _canvas?.transform;
            if (parent == null) return;

            _cableRect.SetParent(parent, worldPositionStays: true);
            _cableRect.SetSiblingIndex(0);
        }

        private void FaceInput()
        {
            var charging = PhoneCharging.Instance;
            if (charging == null || !charging.IsConnected) return;

            Vector2 toSlot = (Vector2)_inputSlot.position - (Vector2)_cableRect.position;
            if (toSlot.sqrMagnitude < 0.0001f) return;

            float angle = Mathf.Atan2(toSlot.y, toSlot.x) * Mathf.Rad2Deg + _facingOffsetDegrees;
            angle = Mathf.Clamp(angle, -50f, 50f);
            _cableRect.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void RefreshAmpereFromCable()
        {
            var charging = PhoneCharging.Instance;
            if (charging == null || !charging.IsCharging || !charging.IsConnected) return;

            Vector2 offset = _cableRect.position - _inputSlot.position;
            charging.UpdateAmpere(offset);
        }

        private float SnapRadius
        {
            get
            {
                var data = PhoneCharging.Instance != null ? PhoneCharging.Instance.Data : null;
                return data != null ? data.CableSnapRadius : 40f;
            }
        }

        private float DistanceToSlot(Vector2 screenPosition)
        {
            var camera = _canvas != null ? _canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _inputSlot, screenPosition, camera, out Vector2 local))
                return local.magnitude;

            return float.MaxValue;
        }

        private void RestoreCable()
        {
            _cableRect.SetParent(_homeParent, false);
            _cableRect.anchoredPosition = _homePos;
            _cableRect.localRotation = _homeRot;
        }

        private void RefreshAmpereText()
        {
            if (_ampereText == null) return;

            var charging = PhoneCharging.Instance;
            float ampere = charging != null && charging.IsConnected ? charging.Ampere : 0f;
            float max = charging != null && charging.Data != null && charging.Data.MaxAmpere > 0f ? charging.Data.MaxAmpere : 1f;
            float t = ampere / max;
            _ampereText.text = $"{ampere:F1}A";
            _ampereText.color = t < 0.5f ? Color.Lerp(_weakColor, _okColor, t * 2f) : Color.Lerp(_okColor, _strongColor, (t - 0.5f) * 2f);
        }

        private void OnBatteryChanged(float percent)
        {
            if (_batteryText != null)
                _batteryText.text = $"{percent:F0}%";

            if (_batterySlider != null)
                _batterySlider.value = Mathf.Clamp01(percent / 100f);
        }

        private void OnDrawGizmos()
        {
            if (_inputSlot == null) return;

            Vector3 slot = _inputSlot.position;

            if (Application.isPlaying && PhoneCharging.Instance != null && PhoneCharging.Instance.IsCharging)
            {
                Vector2 best = PhoneCharging.Instance.BestDirection;
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(slot, slot + (Vector3)(best * 2f));
                DrawArrowHead(slot, slot + (Vector3)(best * 2f), Color.magenta);
            }

            if (_cableRect == null) return;

            Vector3 cable = _cableRect.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cable, slot);

            if (Application.isPlaying && PhoneCharging.Instance != null && PhoneCharging.Instance.IsConnected)
            {
                Vector3 visualFwd = _cableRect.rotation * Vector3.right;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(cable, cable + visualFwd * 1.5f);
                DrawArrowHead(cable, cable + visualFwd * 1.5f, Color.green);
            }
        }

        private static void DrawArrowHead(Vector3 from, Vector3 to, Color color)
        {
            Vector3 dir = to - from;
            if (dir.sqrMagnitude < 0.0001f) return;

            dir.Normalize();
            Vector3 side = Vector3.Cross(dir, Vector3.forward) * 0.15f;
            Gizmos.color = color;
            Gizmos.DrawLine(to, to - dir * 0.25f + side);
            Gizmos.DrawLine(to, to - dir * 0.25f - side);
        }

        private void OnDestroy()
        {
            if (_cable != null)
            {
                _cable.onDragEnded.RemoveListener(OnDragEnded);
                _cable.onDragStarted.RemoveListener(OnDragStarted);
            }

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);

            if (_sessionSubscribed && PhoneCharging.Instance != null)
                PhoneCharging.Instance.OnSessionChanged -= OnSessionChanged;

            if (_batterySubscribed && PhoneBattery.Instance != null)
                PhoneBattery.Instance.OnChanged -= OnBatteryChanged;
        }
    }
}
