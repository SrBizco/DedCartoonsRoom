using NUnit.Framework;

public class MixerMuteToggleTests
{
    [Test]
    public void Toggle_WhenChannelStartsOn_MutesIt()
    {
        var state = new MixerMuteState(false);

        state.Toggle();

        Assert.IsTrue(state.IsMuted);
    }

    [Test]
    public void Toggle_WhenChannelStartsMuted_RestoresIt()
    {
        var state = new MixerMuteState(true);

        state.Toggle();

        Assert.IsFalse(state.IsMuted);
    }
}
