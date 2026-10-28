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

    [Header("Optional transport visuals")]
    [SerializeField] private GameObject playOnVisual;
    [SerializeField] private GameObject pauseOnVisual;
    [SerializeField] private GameObject restartOnVisual;

    private MixerTransportState state;

    private void Awake()
    {
        state = new MixerTransportState();
        SetActiveTransportVisual(null);
    }

    public void PreloadTrackAudioData()
    {
        ForEachTrack(track =>
        {
            if (track.clip != null && track.clip.loadState != AudioDataLoadState.Loaded)
            {
                track.clip.LoadAudioData();
            }
        });
    }

    public void PlayAll()
    {
        var action = state.Play();

        if (action == MixerTransportAction.Start)
        {
            ForEachTrack(track =>
            {
                track.Stop();
                track.time = 0f;
                track.Play();
            });
        }
        else if (action == MixerTransportAction.Resume)
        {
            ForEachTrack(track => track.UnPause());
        }

        if (action != MixerTransportAction.None)
        {
            SetActiveTransportVisual(playOnVisual);
        }
    }

    public void PauseAll()
    {
        if (state.Pause() == MixerTransportAction.Pause)
        {
            ForEachTrack(track => track.Pause());
            SetActiveTransportVisual(pauseOnVisual);
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

        SetActiveTransportVisual(restartOnVisual);
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

    private void SetActiveTransportVisual(GameObject activeVisual)
    {
        SetVisualActive(playOnVisual, activeVisual == playOnVisual && activeVisual != null);
        SetVisualActive(pauseOnVisual, activeVisual == pauseOnVisual && activeVisual != null);
        SetVisualActive(restartOnVisual, activeVisual == restartOnVisual && activeVisual != null);
    }

    private static void SetVisualActive(GameObject visual, bool isActive)
    {
        if (visual != null)
        {
            visual.SetActive(isActive);
        }
    }
}
