using UnityEngine;

public enum MixerTransportAction
{
    None,
    Start,
    Resume,
    Pause
}

public sealed class MixerTransportState
{
    private bool hasStarted;
    private bool isPaused;

    public MixerTransportAction Play()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            isPaused = false;
            return MixerTransportAction.Start;
        }

        if (isPaused)
        {
            isPaused = false;
            return MixerTransportAction.Resume;
        }

        return MixerTransportAction.None;
    }

    public MixerTransportAction Pause()
    {
        if (!hasStarted || isPaused)
        {
            return MixerTransportAction.None;
        }

        isPaused = true;
        return MixerTransportAction.Pause;
    }

    public void Reset()
    {
        hasStarted = false;
        isPaused = false;
    }
}

public sealed class MixerTransportController : MonoBehaviour
{
    [SerializeField] private AudioSource[] tracks;
    [SerializeField, Min(0f)] private float scheduledStartDelay = 0.1f;

    private MixerTransportState state;

    private void Awake()
    {
        state = new MixerTransportState();
    }

    public void PlayAll()
    {
        var action = state.Play();

        if (action == MixerTransportAction.Start)
        {
            var startTime = AudioSettings.dspTime + scheduledStartDelay;
            ForEachTrack(track =>
            {
                track.Stop();
                track.time = 0f;
                track.PlayScheduled(startTime);
            });
        }
        else if (action == MixerTransportAction.Resume)
        {
            ForEachTrack(track => track.UnPause());
        }
    }

    public void PauseAll()
    {
        if (state.Pause() == MixerTransportAction.Pause)
        {
            ForEachTrack(track => track.Pause());
        }
    }

    public void ResetAll()
    {
        state.Reset();
        ForEachTrack(track =>
        {
            track.Stop();
            track.time = 0f;
        });
    }

    private void ForEachTrack(System.Action<AudioSource> action)
    {
        if (tracks == null)
        {
            return;
        }

        foreach (var track in tracks)
        {
            if (track != null)
            {
                action(track);
            }
        }
    }
}
