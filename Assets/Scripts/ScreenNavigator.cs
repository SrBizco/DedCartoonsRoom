using UnityEngine;

public sealed class ScreenNavigationState
{
    public bool IsMixerVisible { get; private set; }
    public bool IsIntroVisible { get; private set; }

    public bool IsRoomVisible => !IsMixerVisible && !IsIntroVisible;

    public ScreenNavigationState(bool startWithMixerOpen, bool startWithIntroOpen = false)
    {
        IsIntroVisible = startWithIntroOpen;
        IsMixerVisible = !startWithIntroOpen && startWithMixerOpen;
    }

    public void OpenIntro()
    {
        IsIntroVisible = true;
        IsMixerVisible = false;
    }

    public void OpenMixer()
    {
        IsIntroVisible = false;
        IsMixerVisible = true;
    }

    public void OpenRoom()
    {
        IsIntroVisible = false;
        IsMixerVisible = false;
    }
}

public sealed class ScreenNavigator : MonoBehaviour
{
    [SerializeField] private GameObject roomScreen;
    [SerializeField] private GameObject roomBackgroundScreen;
    [SerializeField] private GameObject mixerScreen;
    [SerializeField] private GameObject introScreen;
    [SerializeField] private AudioSource roomAmbientAudioSource;
    [SerializeField] private MixerTransportController mixerTransportController;
    [SerializeField] private bool startWithMixerOpen;
    [SerializeField] private bool startWithIntroOpen;

    private ScreenNavigationState state;
    private bool resumeRoomAmbientAfterMixer;

    private void Awake()
    {
        state = new ScreenNavigationState(startWithMixerOpen, startWithIntroOpen);
        SetCameraBackgroundToBlack();
        PreloadInteractiveAudio();
        mixerTransportController?.PreloadTrackAudioData();
        ApplyScreenState();

        if (state.IsMixerVisible)
        {
            PauseRoomAmbientForMixer();
        }
    }

    public void OpenIntro()
    {
        PauseMixerBeforeHiding();
        state.OpenIntro();
        ApplyScreenState();
    }

    public void OpenMixer()
    {
        PauseRoomAmbientForMixer();
        state.OpenMixer();
        ApplyScreenState();
    }

    public void OpenRoom()
    {
        PauseMixerBeforeHiding();
        state.OpenRoom();
        ApplyScreenState();
        ResumeRoomAmbientAfterMixer();
    }

    private void ApplyScreenState()
    {
        if (roomScreen != null)
        {
            roomScreen.SetActive(state.IsRoomVisible);
        }

        if (roomBackgroundScreen != null)
        {
            roomBackgroundScreen.SetActive(state.IsRoomVisible);
        }

        if (mixerScreen != null)
        {
            mixerScreen.SetActive(state.IsMixerVisible);
        }

        if (introScreen != null)
        {
            introScreen.SetActive(state.IsIntroVisible);
        }
    }

    private void PauseRoomAmbientForMixer()
    {
        if (roomAmbientAudioSource == null || !roomAmbientAudioSource.isPlaying)
        {
            return;
        }

        resumeRoomAmbientAfterMixer = true;
        roomAmbientAudioSource.Pause();
    }

    private void ResumeRoomAmbientAfterMixer()
    {
        if (!resumeRoomAmbientAfterMixer)
        {
            return;
        }

        resumeRoomAmbientAfterMixer = false;

        if (roomAmbientAudioSource != null)
        {
            roomAmbientAudioSource.UnPause();
        }
    }

    private void PauseMixerBeforeHiding()
    {
        if (state != null && state.IsMixerVisible)
        {
            mixerTransportController?.PauseAll();
        }
    }

    private static void SetCameraBackgroundToBlack()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
    }

    private static void PreloadInteractiveAudio()
    {
        var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var source in sources)
        {
            if (source.clip != null && source.clip.loadState != AudioDataLoadState.Loaded)
            {
                source.clip.LoadAudioData();
            }
        }

        var interactions = FindObjectsByType<ClickToPlayAnimation>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var interaction in interactions)
        {
            interaction.PreloadAudioData();
        }
    }
}
