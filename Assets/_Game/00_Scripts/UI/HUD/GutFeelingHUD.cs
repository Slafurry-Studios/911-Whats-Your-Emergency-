using System.Collections;
using UnityEngine;
using TMPro;

namespace Slafurry.UI.HUD
{
    public class GutFeelingHUD : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private float _visibleSeconds = 2f;

        private Coroutine _showRoutine;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Show()
        {
            if (_panel == null) return;
            if (_panel == gameObject)
            {
                Debug.LogError("[GutFeelingHUD] _panel must be a child — not this object.", this);
                return;
            }

            // Use the objective's toast slot so it replaces whatever message is there
            // instead of stacking a second line of text on top of it.
            var label = _panel.GetComponentInChildren<TMP_Text>(true);
            if (label != null && ObjectiveHUD.Available)
            {
                ObjectiveHUD.Toast(label.text);
                return;
            }

            if (_showRoutine != null) StopCoroutine(_showRoutine);
            _showRoutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            _panel.SetActive(true);
            yield return new WaitForSeconds(_visibleSeconds);
            _panel.SetActive(false);
            _showRoutine = null;
        }
    }
}
