using System.Collections;
using UnityEngine;

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
