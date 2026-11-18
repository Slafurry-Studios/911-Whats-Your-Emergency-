using System;
using UnityEngine;
using UnityEngine.UI;
using Slafurry.Systems.Audio;

namespace Slafurry.UI.Menu
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Button _backButton;

        private const string MusicPref = "MusicVolume_dB";
        private const string SFXPref = "SFXVolume_dB";

        public event Action BackRequested;

        private bool _audioSubscribed;
        private bool _syncingVolume;

        private void Awake()
        {
            if (_musicSlider != null) _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            if (_backButton != null) _backButton.onClick.AddListener(OnBackPressed);
        }

        private void Update()
        {
            if (!_audioSubscribed && AudioSystem.Instance != null)
            {
                AudioSystem.Instance.OnMusicVolumeChanged += OnMusicVolumeChanged;
                AudioSystem.Instance.OnSFXVolumeChanged += OnSFXVolumeChanged;
                _audioSubscribed = true;
                SyncFromPrefs();
            }
        }

        private void SyncFromPrefs()
        {
            ApplyMusic(AudioSystem.DecibelToLinear(PlayerPrefs.GetFloat(MusicPref, AudioSystem.DefaultDb)));
            ApplySFX(AudioSystem.DecibelToLinear(PlayerPrefs.GetFloat(SFXPref, AudioSystem.DefaultDb)));
        }

        private void ApplyMusic(float linear)
        {
            if (_musicSlider == null) return;

            _syncingVolume = true;
            _musicSlider.SetValueWithoutNotify(linear);
            _syncingVolume = false;
        }

        private void ApplySFX(float linear)
        {
            if (_sfxSlider == null) return;

            _syncingVolume = true;
            _sfxSlider.SetValueWithoutNotify(linear);
            _syncingVolume = false;
        }

        private void OnMusicSliderChanged(float value)
        {
            if (_syncingVolume) return;
            if (AudioSystem.Instance == null) return;

            AudioSystem.Instance.UpdateMusicVolume(AudioSystem.LinearToDecibel(value));
        }

        private void OnSFXSliderChanged(float value)
        {
            if (_syncingVolume) return;
            if (AudioSystem.Instance == null) return;

            AudioSystem.Instance.UpdateSFXVolume(AudioSystem.LinearToDecibel(value));
        }

        private void OnMusicVolumeChanged(float db) => ApplyMusic(AudioSystem.DecibelToLinear(db));

        private void OnSFXVolumeChanged(float db) => ApplySFX(AudioSystem.DecibelToLinear(db));

        private void OnBackPressed() => BackRequested?.Invoke();

        private void OnDestroy()
        {
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
            if (_backButton != null) _backButton.onClick.RemoveListener(OnBackPressed);

            if (_audioSubscribed && AudioSystem.Instance != null)
            {
                AudioSystem.Instance.OnMusicVolumeChanged -= OnMusicVolumeChanged;
                AudioSystem.Instance.OnSFXVolumeChanged -= OnSFXVolumeChanged;
                _audioSubscribed = false;
            }
        }
    }
}
