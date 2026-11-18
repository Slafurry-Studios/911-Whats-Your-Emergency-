using System.Collections;
using Slafurry.Systems.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Slafurry.Systems.Bootstrap
{
    /// <summary>
    /// Placed in the Bootstrap scene (first scene in Build Settings).
    /// Its task: make sure all GameSystem&lt;T&gt; (SceneLoader, AudioSystem, etc.)
    /// finished Initialize() + PostInitialize(), then move to the target scene.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Name of the scene to load once all systems are ready.")]
        [SerializeField] private SceneReference targetScene;

        [Header("Systems To Wait For")]
        [Tooltip("References to all GameSystems that must be ready before changing scene. " +
                 "Drag their components here (SceneLoader, AudioSystem, etc.).")]
        [SerializeField] private MonoBehaviour[] systemsToWaitFor;

        [Header("Optional")]
        [Tooltip("Extra delay (seconds) after all systems are ready, before loading the scene. Useful for splash screen minimum duration.")]
        [SerializeField] private float minimumDelay = 0f;

        [Header("Logo Splash")]
        [Tooltip("Logo images shown one after another. Each fades in, stays, fades out. Leave empty to skip.")]
        [SerializeField] private Image[] logoImages;
        [Tooltip("Fade in duration in seconds (unscaled time), used for every logo.")]
        [SerializeField] private float logoFadeInDuration = 0.75f;
        [Tooltip("How long each logo stays fully visible between fade in and fade out, in seconds (realtime).")]
        [SerializeField] private float logoHoldDuration = 1f;
        [Tooltip("Fade out duration in seconds (unscaled time), used for every logo.")]
        [SerializeField] private float logoFadeOutDuration = 0.75f;

        private bool HasLogo => logoImages != null && logoImages.Length > 0;

        private void Start()
        {
            StartCoroutine(BootstrapRoutine());
        }

        private IEnumerator BootstrapRoutine()
        {
            if (HasLogo)
            {
                foreach (Image logo in logoImages)
                {
                    if (logo != null) logo.gameObject.SetActive(false);
                }
            }

            // Wait 1 frame so all Awake()/OnSingletonAwake() from other systems
            // (set earlier via Script Execution Order) finish first.
            yield return null;

            if (HasLogo)
            {
                foreach (Image logo in logoImages)
                {
                    if (logo == null) continue;

                    logo.gameObject.SetActive(true);
                    SetLogoAlpha(logo, 0f);
                    yield return FadeLogo(logo, 1f, logoFadeInDuration);

                    if (logoHoldDuration > 0f)
                        yield return new WaitForSecondsRealtime(logoHoldDuration);

                    yield return FadeLogo(logo, 0f, logoFadeOutDuration);

                    logo.gameObject.SetActive(false);
                }
            }

            // Initialize each system in order.
            foreach (var systemObj in systemsToWaitFor)
            {
                if (systemObj == null)
                {
                    Debug.LogWarning("[BootstrapLoader] Empty system slot in inspector, skipped.");
                    continue;
                }

                if (systemObj is IGameSystemLifecycle lifecycle)
                {
                    yield return StartCoroutine(lifecycle.Initialize());
                    lifecycle.PostInitialize();
                }
                else
                {
                    Debug.LogWarning($"[BootstrapLoader] {systemObj.GetType().Name} is not a GameSystem, skipped from lifecycle wait.");
                }
            }

            if (minimumDelay > 0f)
                yield return new WaitForSecondsRealtime(minimumDelay);

            Debug.Log($"[BootstrapLoader] All systems ready. Moving to scene '{targetScene}'.");

            if (targetScene == null)
            {
                Debug.LogError("[BootstrapLoader] targetScene is empty! Set it in the inspector.");
                yield break;
            }

            SceneSystem.Load(targetScene);
        }

        private IEnumerator FadeLogo(Image logo, float targetAlpha, float duration)
        {
            if (duration <= 0f)
            {
                SetLogoAlpha(logo, targetAlpha);
                yield break;
            }

            float startAlpha = logo.color.a;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                SetLogoAlpha(logo, Mathf.Lerp(startAlpha, targetAlpha, timer / duration));
                yield return null;
            }

            SetLogoAlpha(logo, targetAlpha);
        }

        private void SetLogoAlpha(Image logo, float alpha)
        {
            Color color = logo.color;
            color.a = alpha;
            logo.color = color;
        }
    }

    /// <summary>
    /// Helper interface so BootstrapLoader can call Initialize()/PostInitialize()
    /// without knowing the generic type T of GameSystem&lt;T&gt;.
    /// Implement this in GameSystem&lt;T&gt; (see note below).
    /// </summary>
    public interface IGameSystemLifecycle
    {
        IEnumerator Initialize();
        void PostInitialize();
    }
}