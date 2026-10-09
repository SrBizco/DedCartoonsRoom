using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ClickLoopState
{
    public bool IsPlaying { get; private set; }

    public bool Toggle()
    {
        IsPlaying = !IsPlaying;
        return IsPlaying;
    }
}

public sealed class ClickToStartLoop : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Animator loopAnimator;

    private ClickLoopState state;

    private void Awake()
    {
        state = new ClickLoopState();

        if (loopAnimator != null)
        {
            loopAnimator.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (loopAnimator == null)
        {
            return;
        }

        loopAnimator.enabled = true;
        loopAnimator.Rebind();
        loopAnimator.Update(0f);
        loopAnimator.enabled = state.Toggle();
    }
}
