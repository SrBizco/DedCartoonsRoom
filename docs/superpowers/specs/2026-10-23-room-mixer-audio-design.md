# Room and Mixer Audio Adjustments

## Intent

Make the shelf response immediate, silence ambient room audio while the mixer is open, and make the mixer transport buttons visibly reflect the active transport action.

## Scope

- The shelf plays every configured one-shot clip simultaneously on its sole interaction.
- Opening MIXER pauses the configured room ambience; returning to ROOM resumes it only when it had been playing before the transition.
- Play, Pause, and Restart own mutually exclusive optional `on` visuals in the transport controller.

## Non-goals

- Do not change the mute-toggle behavior, track sliders, track scheduling, room hitbox workflow, or Unity scene objects.
- Do not make editor, Inspector, commit, or push changes.

## Approach

Extend the existing generic click-animation mode with a dedicated simultaneous-one-shot mode, rather than changing the sequential `PlayOnce` behavior. Add a single ambience AudioSource reference to `ScreenNavigator`, which already owns ROOM/MIXER visibility. Keep transport visuals inside `MixerTransportController`, separate from `MixerMuteToggle`, so mute state cannot affect Play/Pause/Restart visuals.

## Acceptance checks

- A shelf click starts both assigned clips on the same audio frame.
- MIXER entry pauses the chosen ambience and ROOM return resumes it only if it was previously playing.
- Each transport action leaves only its own configured visual active.
