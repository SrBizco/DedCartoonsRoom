using NUnit.Framework;

public class ScreenNavigatorTests
{
    [Test]
    public void OpenMixer_HidesTheRoomAndShowsTheMixer()
    {
        var state = new ScreenNavigationState(false);

        state.OpenMixer();

        Assert.IsFalse(state.IsRoomVisible);
        Assert.IsTrue(state.IsMixerVisible);
    }

    [Test]
    public void OpenRoom_HidesTheMixerAndShowsTheRoom()
    {
        var state = new ScreenNavigationState(true);

        state.OpenRoom();

        Assert.IsTrue(state.IsRoomVisible);
        Assert.IsFalse(state.IsMixerVisible);
    }
}
