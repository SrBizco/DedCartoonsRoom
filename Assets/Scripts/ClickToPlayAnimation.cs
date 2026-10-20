using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ClickAnimationMode
{
    ToggleLoop,
    PlayOnce,
    SequentialPlaylist
}

public enum ClickAnimationAction
{
    None,
    Start,
    Stop
}

public sealed class ClickAnimationState
{
    public bool IsPlaying { get; private set; }

    public ClickAnimationAction Click(ClickAnimationMode mode)
    {
        if (mode == ClickAnimationMode.PlayOnce && IsPlaying)
        {
            return ClickAnimationAction.None;
        }

        IsPlaying = !IsPlaying;
        return IsPlaying ? ClickAnimationAction.Start : ClickAnimationAction.Stop;
    }

    public void Complete()
    {
        IsPlaying = false;
    }

    public void Reset()
    {
        IsPlaying = false;
    }
}

public sealed class SequentialPlaylistState
{
    private readonly int trackCount;
    private int nextIndex;

    public bool IsPlaying { get; private set; }

    public SequentialPlaylistState(int trackCount)
    {
        this.trackCount = Mathf.Max(0, trackCount);
    }

    public int TakeNextIndex()
    {
        if (trackCount == 0)
        {
            return -1;
        }

        var selectedIndex = nextIndex;
        nextIndex = (nextIndex + 1) % trackCount;
        return selectedIndex;
    }

    public ClickAnimationAction Click()
    {
        if (trackCount == 0)
        {
            return ClickAnimationAction.None;
        }

        IsPlaying = !IsPlaying;
        return IsPlaying ? ClickAnimationAction.Start : ClickAnimationAction.Stop;
    }

    public void Complete()
    {
        IsPlaying = false;
    }

    public void Reset()
    {
        nextIndex = 0;
        IsPlaying = false;
    }
}

public sealed class OneShotAudioSequenceState
{
    private readonly int clipCount;
    private int nextIndex;

    public OneShotAudioSequenceState(int clipCount)
    {
        this.clipCount = Mathf.Max(0, clipCount);
    }

    public int TakeNextIndex()
    {
        if (nextIndex >= clipCount)
        {
            return -1;
        }

        return nextIndex++;
    }

    public void Reset()
    {
        nextIndex = 0;
    }
}

public sealed class ClickToPlayAnimation : MonoBehaviour, IPointerClickHandler
{
    [Header("Optional animation")]
    [SerializeField] private Animator loopAnimator;
    [SerializeField] private Image animatedImage;

    [SerializeField] private bool restartAnimationOnEachClick = true;

    [Header("Optional audio")]
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip[] audioClips;

    [Header("Behaviour")]
    [SerializeField] private ClickAnimationMode mode;
    [SerializeField] private bool hideVisualAfterClick;
    [SerializeField] private Image visualToHide;

    private ClickAnimationState state;
    private SequentialPlaylistState playlist;
    private OneShotAudioSequenceState oneShotAudioSequence;
    private Sprite baseSprite;
    private bool interactionConsumed;

    private void Awake()
    {
        if (loopAnimator == null)
        {
            loopAnimator = GetComponentInParent<Animator>();
        }

        if (animatedImage == null && loopAnimator != null)
        {
            animatedImage = loopAnimator.GetComponent<Image>();
        }

        if (soundSource == null)
        {
            soundSource = GetComponent<AudioSource>();
        }

        if (soundSource != null)
        {
            soundSource.playOnAwake = false;
            soundSource.loop = false;
        }

        state = new ClickAnimationState();
        playlist = new SequentialPlaylistState(audioClips == null ? 0 : audioClips.Length);
        oneShotAudioSequence = new OneShotAudioSequenceState(audioClips == null ? 0 : audioClips.Length);

        if (animatedImage != null)
        {
            baseSprite = animatedImage.sprite;
        }

        StopPlayback();
    }

    private void Update()
    {
        if (mode == ClickAnimationMode.SequentialPlaylist)
        {
            StopSequentialPlaybackWhenAudioEnds();
            return;
        }

        if (mode != ClickAnimationMode.PlayOnce || !state.IsPlaying)
        {
            return;
        }

        if (TryPlayNextOneShotClip())
        {
            return;
        }

        if (HasPlayOnceFinished())
        {
            state.Complete();
            StopPlayback();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (interactionConsumed)
        {
            return;
        }

        if (mode == ClickAnimationMode.SequentialPlaylist)
        {
            var playlistAction = playlist.Click();
            if (playlistAction == ClickAnimationAction.Start)
            {
                StartSequentialTrack();
                ConsumeInteractionIfConfigured();
            }
            else if (playlistAction == ClickAnimationAction.Stop)
            {
                StopPlayback();
            }

            return;
        }

        var action = state.Click(mode);
        if (action == ClickAnimationAction.None)
        {
            return;
        }

        if (action == ClickAnimationAction.Start)
        {
            StartPlayback(mode == ClickAnimationMode.PlayOnce ? GetFirstOneShotClip() : GetSingleAudioClip());
            ConsumeInteractionIfConfigured();
        }
        else
        {
            StopPlayback();
        }
    }

    private void OnDisable()
    {
        state?.Reset();
        playlist?.Reset();
        oneShotAudioSequence?.Reset();
        StopPlayback();
    }

    private void StartSequentialTrack()
    {
        var clipIndex = playlist.TakeNextIndex();
        var clip = clipIndex >= 0 ? audioClips[clipIndex] : GetSingleAudioClip();
        StartPlayback(clip);
    }

    private void StopSequentialPlaybackWhenAudioEnds()
    {
        if (!playlist.IsPlaying || soundSource == null || soundSource.clip == null || soundSource.isPlaying)
        {
            return;
        }

        playlist.Complete();
        StopPlayback();
    }

    private AudioClip GetSingleAudioClip()
    {
        if (audioClips != null && audioClips.Length > 0)
        {
            return audioClips[0];
        }

        return soundSource != null ? soundSource.clip : null;
    }

    private AudioClip GetFirstOneShotClip()
    {
        var clipIndex = oneShotAudioSequence.TakeNextIndex();
        return clipIndex >= 0 ? audioClips[clipIndex] : GetSingleAudioClip();
    }

    private bool TryPlayNextOneShotClip()
    {
        if (soundSource == null || soundSource.isPlaying)
        {
            return false;
        }

        var clipIndex = oneShotAudioSequence.TakeNextIndex();
        if (clipIndex < 0)
        {
            return false;
        }

        PlayAudio(audioClips[clipIndex]);
        return true;
    }

    private void StartPlayback(AudioClip clip)
    {
        if (loopAnimator != null)
        {
            var shouldRestart = restartAnimationOnEachClick || !loopAnimator.enabled;
            loopAnimator.enabled = true;

            if (shouldRestart)
            {
                loopAnimator.Rebind();
                loopAnimator.Update(0f);
            }
        }

        PlayAudio(clip);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (soundSource != null && clip != null)
        {
            soundSource.Stop();
            soundSource.clip = clip;
            soundSource.Play();
        }
    }

    private bool HasPlayOnceFinished()
    {
        if (loopAnimator != null && loopAnimator.enabled)
        {
            return loopAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
        }

        return soundSource == null || soundSource.clip == null || !soundSource.isPlaying;
    }

    private void StopPlayback()
    {
        if (loopAnimator != null)
        {
            loopAnimator.enabled = false;
        }

        if (soundSource != null)
        {
            soundSource.Stop();
        }

        if (animatedImage != null)
        {
            animatedImage.sprite = baseSprite;
        }
    }

    private void ConsumeInteractionIfConfigured()
    {
        if (!hideVisualAfterClick)
        {
            return;
        }

        interactionConsumed = true;

        if (visualToHide != null)
        {
            visualToHide.enabled = false;
        }

        var hitboxGraphic = GetComponent<Graphic>();
        if (hitboxGraphic != null)
        {
            hitboxGraphic.raycastTarget = false;
        }
    }
}
