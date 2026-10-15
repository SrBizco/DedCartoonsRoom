using UnityEngine;

public sealed class MixerMuteState
{
    public bool IsMuted { get; private set; }

    public MixerMuteState(bool startsMuted)
    {
        IsMuted = startsMuted;
    }

    public void Toggle()
    {
        IsMuted = !IsMuted;
    }
}

public sealed class MixerMuteToggle : MonoBehaviour
{
    [Header("Visual state")]
    [SerializeField] private GameObject offVisual;
    [SerializeField] private bool startsMuted;

    [Header("Optional audio")]
    [SerializeField] private AudioSource trackAudioSource;

    private MixerMuteState state;

    private void Awake()
    {
        state = new MixerMuteState(startsMuted);
        ApplyState();
    }

    public void ToggleMute()
    {
        state.Toggle();
        ApplyState();
    }

    private void ApplyState()
    {
        if (offVisual != null)
        {
            offVisual.SetActive(state.IsMuted);
        }

        if (trackAudioSource != null)
        {
            trackAudioSource.mute = state.IsMuted;
        }
    }
}
