using UnityEngine;

public sealed class ScreenNavigationState
{
    public bool IsMixerVisible { get; private set; }

    public bool IsRoomVisible => !IsMixerVisible;

    public ScreenNavigationState(bool startWithMixerOpen)
    {
        IsMixerVisible = startWithMixerOpen;
    }

    public void OpenMixer()
    {
        IsMixerVisible = true;
    }

    public void OpenRoom()
    {
        IsMixerVisible = false;
    }
}

public sealed class ScreenNavigator : MonoBehaviour
{
    [SerializeField] private GameObject roomScreen;
    [SerializeField] private GameObject mixerScreen;
    [SerializeField] private bool startWithMixerOpen;

    private ScreenNavigationState state;

    private void Awake()
    {
        state = new ScreenNavigationState(startWithMixerOpen);
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
    }
}
