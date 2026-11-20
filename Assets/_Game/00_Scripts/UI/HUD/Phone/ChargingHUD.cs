using UnityEngine;
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

            if (_panel != null) _panel.SetActive(false);
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

            FaceInput();
            RefreshAmpereFromCable();
            RefreshAmpereText();
        }

        private void OnSessionChanged(bool open)
        {
            if (_panel != null) _panel.SetActive(open);

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

            float snapRadius = SnapRadius;
            float distance = DistanceToSlot(screenPosition);

            if (distance <= snapRadius)
            {
                SnapCableToSlot();
                charging.Connect();
                charging.UpdateAmpere(Vector2.zero);
            }
            else
            {
                PlaceAtBottom();
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

        private void SnapCableToSlot()
        {
            _cableRect.position = _inputSlot.position;
        }

        /// <summary>Place cable at bottom layer when released without snapping.</summary>
        private void PlaceAtBottom()
        {
            if (_cableRect == null || _canvas == null) return;

            var canvasRect = _canvas.GetComponent<RectTransform>();
            if (canvasRect == null) return;

            _cableRect.SetParent(canvasRect, false);
            float bottomY = -canvasRect.rect.height * 0.5f + _cableRect.rect.height * 0.5f;
            _cableRect.anchoredPosition = new Vector2(0f, bottomY);
            _cableRect.localRotation = Quaternion.identity;
            _cableRect.SetSiblingIndex(0);
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
            _ampereText.text = $"{ampere:F1}A";
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
