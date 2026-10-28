using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public static class WebGLVideoUrl
{
    public static void Configure(VideoPlayer videoPlayer, string relativePath)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (videoPlayer == null || string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = $"{Application.streamingAssetsPath}/{relativePath}";
#endif
    }
}

public sealed class IntroVideoState
{
    public bool HasStarted { get; private set; }
    private bool shouldResumeBackgroundAudio;
    private bool backgroundAudioWasMuted;

    public bool TryStart()
    {
        if (HasStarted)
        {
            return false;
        }

        HasStarted = true;
        return true;
    }

    public bool BeginBackgroundAudioPause(bool isBackgroundAudioPlaying)
    {
        shouldResumeBackgroundAudio = isBackgroundAudioPlaying;
        return shouldResumeBackgroundAudio;
    }

    public bool TryResumeBackgroundAudio()
    {
        if (!shouldResumeBackgroundAudio)
        {
            return false;
        }

        shouldResumeBackgroundAudio = false;
        return true;
    }

    public void CaptureBackgroundAudioMute(bool isBackgroundAudioMuted)
    {
        backgroundAudioWasMuted = isBackgroundAudioMuted;
    }

    public bool GetBackgroundAudioMuteToRestore()
    {
        return backgroundAudioWasMuted;
    }
}

public sealed class IntroVideoScreen : MonoBehaviour
{
    [SerializeField] private VideoPlayer introVideoPlayer;
    [SerializeField] private ScreenNavigator screenNavigator;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private string webGlVideoPath = "Videos/intro.mp4";

    private IntroVideoState state;

    private void Awake()
    {
        state = new IntroVideoState();

        if (introVideoPlayer != null)
        {
            WebGLVideoUrl.Configure(introVideoPlayer, webGlVideoPath);
            introVideoPlayer.playOnAwake = false;
            introVideoPlayer.isLooping = false;
            introVideoPlayer.waitForFirstFrame = true;
        }
    }

    private void OnEnable()
    {
        if (introVideoPlayer != null)
        {
            introVideoPlayer.prepareCompleted += OnIntroPrepared;
            introVideoPlayer.loopPointReached += OnIntroFinished;
        }

        MuteBackgroundAudio();
        StartIntro();
    }

    private IEnumerator Start()
    {
        yield return null;
        PauseBackgroundAudio();
    }

    private void OnDisable()
    {
        if (introVideoPlayer != null)
        {
            introVideoPlayer.prepareCompleted -= OnIntroPrepared;
            introVideoPlayer.loopPointReached -= OnIntroFinished;
        }

        RestoreBackgroundAudio();
    }

    private void StartIntro()
    {
        if (state == null)
        {
            state = new IntroVideoState();
        }

        if (!state.TryStart())
        {
            return;
        }

        if (introVideoPlayer == null)
        {
            OpenRoom();
            return;
        }

        introVideoPlayer.Stop();
        introVideoPlayer.time = 0d;
        introVideoPlayer.Prepare();
    }

    public void RestartIntroAfterUserGesture()
    {
        if (introVideoPlayer == null)
        {
            OpenRoom();
            return;
        }

        introVideoPlayer.Stop();
        introVideoPlayer.time = 0d;
        introVideoPlayer.Prepare();
    }

    private void OnIntroPrepared(VideoPlayer player)
    {
        if (state != null && state.HasStarted && player == introVideoPlayer)
        {
            player.Play();
        }
    }

    private void OnIntroFinished(VideoPlayer player)
    {
        if (player == introVideoPlayer)
        {
            OpenRoom();
        }
    }

    private void OpenRoom()
    {
        RestoreBackgroundAudio();

        if (screenNavigator != null)
        {
            screenNavigator.OpenRoom();
        }
    }

    private void PauseBackgroundAudio()
    {
        if (backgroundAudioSource != null && state.BeginBackgroundAudioPause(backgroundAudioSource.isPlaying))
        {
            backgroundAudioSource.Pause();
        }
    }

    private void MuteBackgroundAudio()
    {
        if (backgroundAudioSource == null || state == null)
        {
            return;
        }

        state.CaptureBackgroundAudioMute(backgroundAudioSource.mute);
        backgroundAudioSource.mute = true;
    }

    private void RestoreBackgroundAudio()
    {
        if (backgroundAudioSource == null || state == null)
        {
            return;
        }

        backgroundAudioSource.mute = state.GetBackgroundAudioMuteToRestore();

        if (state.TryResumeBackgroundAudio())
        {
            backgroundAudioSource.UnPause();
        }
    }
}
