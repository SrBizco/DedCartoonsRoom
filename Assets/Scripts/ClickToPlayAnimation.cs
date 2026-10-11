using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ClickAnimationMode
{
    ToggleLoop,
    PlayOnce
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

public sealed class ClickToPlayAnimation : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Animator loopAnimator;
    [SerializeField] private Image animatedImage;
    [SerializeField] private ClickAnimationMode mode;

    private ClickAnimationState state;
    private Sprite baseSprite;

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

        state = new ClickAnimationState();

        if (animatedImage != null)
        {
            baseSprite = animatedImage.sprite;
        }

        StopAnimation();
    }

    private void Update()
    {
        if (mode != ClickAnimationMode.PlayOnce || !state.IsPlaying || loopAnimator == null)
        {
            return;
        }

        if (loopAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            state.Complete();
            StopAnimation();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (loopAnimator == null || animatedImage == null)
        {
            return;
        }

        var action = state.Click(mode);
        if (action == ClickAnimationAction.None)
        {
            return;
        }

        if (action == ClickAnimationAction.Start)
        {
            loopAnimator.enabled = true;
            loopAnimator.Rebind();
            loopAnimator.Update(0f);
        }
        else
        {
            StopAnimation();
        }
    }

    private void OnDisable()
    {
        state?.Reset();
        StopAnimation();
    }

    private void StopAnimation()
    {
        if (loopAnimator != null)
        {
            loopAnimator.enabled = false;
        }

        if (animatedImage != null)
        {
            animatedImage.sprite = baseSprite;
        }
    }
}
