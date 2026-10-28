# Room and Mixer Audio Adjustments Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make shelf sounds simultaneous, pause room ambience while MIXER is visible, and synchronize transport visuals with transport state.

**Architecture:** `ClickToPlayAnimation` gains one isolated play-all-once mode. `ScreenNavigator` owns ambience transition state because it already changes ROOM and MIXER visibility. `MixerTransportController` owns the three mutually exclusive transport visuals; `MixerMuteToggle` remains track-only.

**Tech Stack:** Unity C#, Unity UI, `AudioSource`.

**Spec:** `docs/superpowers/specs/2026-10-23-room-mixer-audio-design.md`

## Global Constraints

- Do not alter Unity scene or Inspector data; provide field setup guidance only.
- Do not alter mute toggles, sliders, or existing track scheduling behavior.
- Do not add Unity test assets, commits, or pushes.

## Review Focus

- Empty or null shelf clips must be ignored without blocking a valid clip.
- Entering MIXER while ambience is already stopped must not start it on ROOM return.
- Repeated Play while tracks are already playing must preserve the Play visual.
- Pause before first Play must not change the selected transport visual.
- Null optional transport visual objects must not throw errors.

---

### Task 1: Simultaneous shelf clips

**Files:**
- Modify: `Assets/Scripts/ClickToPlayAnimation.cs`

**Interfaces:**
- Produces: `ClickAnimationMode.PlayAllOnce`, which starts every non-null `audioClips` entry using `AudioSource.PlayOneShot`.

- [ ] Add the new mode without altering `PlayOnce` or `SequentialPlaylist` behavior.
- [ ] Start all configured non-null clips together and complete when the source stops playing.
- [ ] Compile the affected scripts with Unity references; expect no C# errors.

### Task 2: Ambient transition control

**Files:**
- Modify: `Assets/Scripts/ScreenNavigator.cs`

**Interfaces:**
- Produces: optional serialized `AudioSource roomAmbientAudioSource`.

- [ ] Capture whether ambience is playing before `OpenMixer`, then pause it.
- [ ] Resume it from `OpenRoom` only when this navigator paused it.
- [ ] Compile the affected scripts with Unity references; expect no C# errors.

### Task 3: Exclusive transport visuals

**Files:**
- Modify: `Assets/Scripts/MixerTransportController.cs`

**Interfaces:**
- Produces: optional serialized `GameObject` references for Play, Pause, and Restart active visuals.

- [ ] Initialize all optional active visuals disabled.
- [ ] Activate exactly one visual after a successful Play, Pause, or Restart action; preserve existing audio control.
- [ ] Compile the affected scripts with Unity references; expect no C# errors.
