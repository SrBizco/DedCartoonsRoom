using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PingPongFrameCursor
{
    private readonly int frameCount;
    private int direction = 1;

    public int CurrentIndex { get; private set; }

    public PingPongFrameCursor(int frameCount)
    {
        this.frameCount = Mathf.Max(1, frameCount);
        CurrentIndex = 0;
    }

    public int Advance()
    {
        if (frameCount == 1)
        {
            return 0;
        }

        CurrentIndex += direction;

        if (CurrentIndex >= frameCount - 1)
        {
            CurrentIndex = frameCount - 1;
            direction = -1;
        }
        else if (CurrentIndex <= 0)
        {
            CurrentIndex = 0;
            direction = 1;
        }

        return CurrentIndex;
    }

    public void Reset()
    {
        CurrentIndex = 0;
        direction = 1;
    }
}

public sealed class BatHoverPingPong : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image animatedImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float framesPerSecond = 12f;

    private PingPongFrameCursor sequence;
    private bool isHovered;
    private float elapsed;

    private void Awake()
    {
        sequence = new PingPongFrameCursor(frames == null ? 0 : frames.Length);
        ShowFirstFrame();
    }

    private void Update()
    {
        if (!isHovered || frames == null || frames.Length < 2 || animatedImage == null)
        {
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        var secondsPerFrame = 1f / framesPerSecond;

        while (elapsed >= secondsPerFrame)
        {
            elapsed -= secondsPerFrame;
            animatedImage.sprite = frames[sequence.Advance()];
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        elapsed = 0f;
        sequence.Reset();
        ShowFirstFrame();
    }

    private void OnDisable()
    {
        isHovered = false;
        elapsed = 0f;
        sequence?.Reset();
        ShowFirstFrame();
    }

    private void ShowFirstFrame()
    {
        if (animatedImage != null && frames != null && frames.Length > 0)
        {
            animatedImage.sprite = frames[0];
        }
    }
}
