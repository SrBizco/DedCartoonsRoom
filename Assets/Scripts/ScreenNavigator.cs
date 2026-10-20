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
    [SerializeField] private GameObject mixerScreen;
    [SerializeField] private GameObject introScreen;
    [SerializeField] private bool startWithMixerOpen;
    [SerializeField] private bool startWithIntroOpen;

    private ScreenNavigationState state;

    private void Awake()
    {
        state = new ScreenNavigationState(startWithMixerOpen, startWithIntroOpen);
        ApplyScreenState();
    }

    public void OpenIntro()
    {
        state.OpenIntro();
        ApplyScreenState();
    }

    public void OpenMixer()
    {
        state.OpenMixer();
        ApplyScreenState();
    }

    public void OpenRoom()
    {
        state.OpenRoom();
        ApplyScreenState();
    }

    private void ApplyScreenState()
    {
        if (roomScreen != null)
        {
            roomScreen.SetActive(state.IsRoomVisible);
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
}
