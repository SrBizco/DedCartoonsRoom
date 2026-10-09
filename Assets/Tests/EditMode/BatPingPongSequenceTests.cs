using NUnit.Framework;

public class BatPingPongSequenceTests
{
    [Test]
    public void Advance_ReversesAfterTheLastFrameWithoutJumpingToTheFirst()
    {
        var sequence = new PingPongFrameCursor(15);
        var frames = new int[16];

        for (var i = 0; i < frames.Length; i++)
        {
            frames[i] = sequence.Advance();
        }

        CollectionAssert.AreEqual(
            new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 13, 12 },
            frames);
    }

    [Test]
    public void Reset_ReturnsToTheFirstFrameAndForwardDirection()
    {
        var sequence = new PingPongFrameCursor(15);
        sequence.Advance();
        sequence.Advance();
        sequence.Reset();

        Assert.AreEqual(0, sequence.CurrentIndex);
        Assert.AreEqual(1, sequence.Advance());
    }

    [Test]
    public void ClickLoopState_TogglesTheLoopOnAndOff()
    {
        var state = new ClickLoopState();

        Assert.IsFalse(state.IsPlaying);
        Assert.IsTrue(state.Toggle());
        Assert.IsTrue(state.IsPlaying);
        Assert.IsFalse(state.Toggle());
        Assert.IsFalse(state.IsPlaying);
        Assert.IsTrue(state.Toggle());
        Assert.IsTrue(state.IsPlaying);
    }
}
