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
    public void ToggleLoop_SecondClickStopsPlayback()
    {
        var state = new ClickAnimationState();

        Assert.IsFalse(state.IsPlaying);
        Assert.AreEqual(ClickAnimationAction.Start, state.Click(ClickAnimationMode.ToggleLoop));
        Assert.IsTrue(state.IsPlaying);
        Assert.AreEqual(ClickAnimationAction.Stop, state.Click(ClickAnimationMode.ToggleLoop));
        Assert.IsFalse(state.IsPlaying);
        Assert.AreEqual(ClickAnimationAction.Start, state.Click(ClickAnimationMode.ToggleLoop));
        Assert.IsTrue(state.IsPlaying);
    }

    [Test]
    public void PlayOnce_StopsOnCompletionAndCanBeStartedAgain()
    {
        var state = new ClickAnimationState();

        Assert.AreEqual(ClickAnimationAction.Start, state.Click(ClickAnimationMode.PlayOnce));
        Assert.AreEqual(ClickAnimationAction.None, state.Click(ClickAnimationMode.PlayOnce));
        Assert.IsTrue(state.IsPlaying);
        state.Complete();
        Assert.IsFalse(state.IsPlaying);
        Assert.AreEqual(ClickAnimationAction.Start, state.Click(ClickAnimationMode.PlayOnce));
    }

    [Test]
    public void SequentialPlaylist_ReturnsEachTrackThenStartsAgainFromTheFirst()
    {
        var playlist = new SequentialPlaylistState(3);

        CollectionAssert.AreEqual(
            new[] { 0, 1, 2, 0, 1 },
            new[]
            {
                playlist.TakeNextIndex(),
                playlist.TakeNextIndex(),
                playlist.TakeNextIndex(),
                playlist.TakeNextIndex(),
                playlist.TakeNextIndex()
            });
    }

    [Test]
    public void SequentialPlaylist_WithNoTracks_ReturnsNoTrack()
    {
        var playlist = new SequentialPlaylistState(0);

        Assert.AreEqual(-1, playlist.TakeNextIndex());
    }

    [Test]
    public void SequentialPlaylist_SecondClickStopsWithoutConsumingTheNextTrack()
    {
        var playlist = new SequentialPlaylistState(3);

        Assert.AreEqual(ClickAnimationAction.Start, playlist.Click());
        Assert.AreEqual(0, playlist.TakeNextIndex());
        Assert.AreEqual(ClickAnimationAction.Stop, playlist.Click());
        Assert.AreEqual(ClickAnimationAction.Start, playlist.Click());
        Assert.AreEqual(1, playlist.TakeNextIndex());
    }

    [Test]
    public void SequentialPlaylist_CompletionStopsPlaybackWithoutResettingTheNextTrack()
    {
        var playlist = new SequentialPlaylistState(2);

        playlist.Click();
        playlist.TakeNextIndex();
        playlist.Complete();

        Assert.IsFalse(playlist.IsPlaying);
        Assert.AreEqual(ClickAnimationAction.Start, playlist.Click());
        Assert.AreEqual(1, playlist.TakeNextIndex());
    }

}
