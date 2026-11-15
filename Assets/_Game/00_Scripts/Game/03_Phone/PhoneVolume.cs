using UnityEngine;

public class PhoneVolume : MonoBehaviour
{
    [SerializeField] private PhoneData _data;

    public static PhoneVolume Instance { get; private set; }

    public float Volume { get; private set; }

    public event System.Action<float> OnChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Volume = Mathf.Clamp01(_data.StartVolume);
        OnChanged?.Invoke(Volume);
    }

    public void SetVolume(float volume)
    {
        float clamped = Mathf.Clamp01(volume);
        if (Mathf.Approximately(clamped, Volume)) return;

        Volume = clamped;
        OnChanged?.Invoke(Volume);
    }
}
