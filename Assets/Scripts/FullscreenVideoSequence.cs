using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public sealed class FullscreenVideoSequenceState
{
    public bool IsPlaying { get; private set; }
    public bool IsVisualReady { get; private set; }

    public bool TryStart()
    {
        if (IsPlaying)
        {
            return false;
        }

        IsPlaying = true;
        IsVisualReady = false;
        return true;
    }

    public void MarkFirstFrameReady()
    {
        if (IsPlaying)
        {
            IsVisualReady = true;
        }
    }

    public void Finish()
    {
        IsPlaying = false;
        IsVisualReady = false;
    }
}

public sealed class PausedAudioResumeState
{
    private bool shouldResume;

    public void RememberPlayback(bool wasPlaying)
    {
        shouldResume = wasPlaying;
    }

    public bool TakeResumeRequest()
    {
        var resume = shouldResume;
        shouldResume = false;
        return resume;
    }
}

public sealed class FullscreenVideoSequence : MonoBehaviour, IPointerClickHandler
{
    [Header("Background loop")]
    [SerializeField] private VideoPlayer backgroundVideoPlayer;
    [SerializeField] private string webGlBackgroundVideoPath = "Videos/background.mp4";

    [Header("One-shot sequence")]
    [SerializeField] private VideoPlayer sequenceVideoPlayer;
    [SerializeField] private GameObject sequenceVisual;
    [SerializeField] private string webGlSequenceVideoPath = "Videos/dander-talk.mp4";

    [Header("Optional audio to pause")]
    [SerializeField] private AudioSource ambientAudioSource;

    private FullscreenVideoSequenceState state;
    private PausedAudioResumeState audioResumeState;
    private bool resumeBackgroundAfterSequence;

    private void Awake()
    {
        state = new FullscreenVideoSequenceState();
        audioResumeState = new PausedAudioResumeState();

        WebGLVideoUrl.Configure(backgroundVideoPlayer, webGlBackgroundVideoPath);
        WebGLVideoUrl.Configure(sequenceVideoPlayer, webGlSequenceVideoPath);

        if (sequenceVisual != null)
        {
            sequenceVisual.SetActive(false);
        }

        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.playOnAwake = false;
            sequenceVideoPlayer.isLooping = false;
            sequenceVideoPlayer.waitForFirstFrame = true;
            sequenceVideoPlayer.sendFrameReadyEvents = true;
        }
    }

    private void OnEnable()
    {
        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.loopPointReached += OnSequenceFinished;
            sequenceVideoPlayer.prepareCompleted += OnSequencePrepared;
            sequenceVideoPlayer.frameReady += OnSequenceFrameReady;
        }
    }

    private void OnDisable()
    {
        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.loopPointReached -= OnSequenceFinished;
            sequenceVideoPlayer.prepareCompleted -= OnSequencePrepared;
            sequenceVideoPlayer.frameReady -= OnSequenceFrameReady;
        }
    }

    public void PlaySequence()
    {
        if (state == null)
        {
            state = new FullscreenVideoSequenceState();
        }

        if (!state.TryStart())
        {
            return;
        }

        resumeBackgroundAfterSequence = backgroundVideoPlayer != null && backgroundVideoPlayer.isPlaying;

        if (resumeBackgroundAfterSequence)
        {
            backgroundVideoPlayer.Pause();
        }

        var shouldPauseAmbientAudio = ambientAudioSource != null && ambientAudioSource.isPlaying;
        audioResumeState.RememberPlayback(shouldPauseAmbientAudio);

        if (shouldPauseAmbientAudio)
        {
            ambientAudioSource.Pause();
        }

        if (sequenceVisual != null)
        {
            sequenceVisual.SetActive(false);
        }

        if (sequenceVideoPlayer == null)
        {
            FinishSequence();
            return;
        }

        sequenceVideoPlayer.Stop();
        sequenceVideoPlayer.time = 0d;
        sequenceVideoPlayer.Prepare();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySequence();
    }

    private void OnSequenceFinished(VideoPlayer _)
    {
        FinishSequence();
    }

    private void OnSequencePrepared(VideoPlayer player)
    {
        if (state == null || !state.IsPlaying || player != sequenceVideoPlayer)
        {
            return;
        }

        player.Play();
    }

    private void OnSequenceFrameReady(VideoPlayer player, long _)
    {
        if (state == null || !state.IsPlaying || player != sequenceVideoPlayer || state.IsVisualReady)
        {
            return;
        }

        state.MarkFirstFrameReady();

        if (sequenceVisual != null)
        {
            sequenceVisual.SetActive(true);
        }
    }

    private void FinishSequence()
    {
        if (state == null || !state.IsPlaying)
        {
            return;
        }

        state.Finish();

        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.Stop();
        }

        if (sequenceVisual != null)
        {
            sequenceVisual.SetActive(false);
        }

        if (resumeBackgroundAfterSequence && backgroundVideoPlayer != null)
        {
            backgroundVideoPlayer.Play();
        }

        if (ambientAudioSource != null && audioResumeState.TakeResumeRequest())
        {
            ambientAudioSource.UnPause();
        }

        resumeBackgroundAfterSequence = false;
    }
}
