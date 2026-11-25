using System.Collections;
using UnityEngine;
using Slafurry.Systems.InputHub;
using Slafurry.Systems.Scene;

/// <summary>
/// Ends the round: police arrived → ending scene (win); player health 0 → game over (lose).
/// </summary>
public class GameOutcome : MonoBehaviour
{
    [SerializeField] private SceneReference _winScene;
    [SerializeField] private SceneReference _loseScene;
    [Tooltip("Real-time seconds before the scene changes (lets the last hit / arrival read).")]
    [SerializeField] private float _delay = 1.5f;

    private bool _policeSubscribed;
    private bool _healthSubscribed;
    private bool _finished;

    public static bool IsFinished { get; private set; }

    private void Awake() => IsFinished = false;

    private void Update()
    {
        if (!_policeSubscribed && PoliceArrival.Instance != null)
        {
            PoliceArrival.Instance.OnArrived += OnWin;
            _policeSubscribed = true;
        }
        if (!_healthSubscribed && PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnDied += OnLose;
            _healthSubscribed = true;
        }
    }

    private void OnWin() => Finish(_winScene, "police arrived");

    private void OnLose() => Finish(_loseScene, "player died");

    private void Finish(SceneReference scene, string reason)
    {
        if (_finished) return;
        _finished = true;
        IsFinished = true;
        Debug.Log($"[GameOutcome] Round over: {reason}.", this);

        Controls.SetInputEnabled(false);
        StartCoroutine(LoadAfterDelay(scene));
    }

    private IEnumerator LoadAfterDelay(SceneReference scene)
    {
        yield return new WaitForSecondsRealtime(_delay);
        if (scene != null) SceneSystem.Load(scene);
        else Debug.LogError("[GameOutcome] No scene assigned.", this);
    }

    private void OnDestroy()
    {
        if (_policeSubscribed && PoliceArrival.Instance != null) PoliceArrival.Instance.OnArrived -= OnWin;
        if (_healthSubscribed && PlayerHealth.Instance != null) PlayerHealth.Instance.OnDied -= OnLose;
    }
}
