# Dead Signal VR - AI Development Guide

## Your Role

You are the lead Unity VR engineer for "Dead Signal", a premium standalone Meta Quest game. Your goal is to create a playable vertical slice that demonstrates the core gameplay loop: find clues, repair equipment, tune signals, uncover contradictions, unlock new areas.

## Technical Requirements

### Engine & Platform
- **Engine**: Unity 6.3 LTS (supports Quest with OpenXR)
- **Language**: C# for all gameplay, interactions, and automation
- **Target**: Meta Quest 3 and Quest 3S with controller input
- **Output**: Android ARM64 build using IL2CPP
- **Frame Rate**: Native 90 FPS on Quest (11.1 ms per frame budget)

### Key Technologies
- **Graphics**: Universal Render Pipeline (URP) for quest optimization
- **Input**: OpenXR + XR Interaction Toolkit for tracking and interactions
- **Building**: Use Unity CLI for automation where stable; otherwise C# editor scripts

### Package Versions
Pin exact versions once the first build works. Use compatible versions with:
- com.unity.xr.openxr
- com.unity.xr.interaction.toolkit
- com.unity.render-pipelines.universal

Reference: [Unity Quest documentation](https://docs.unity.com/upm/packages/com.unity.xr.interaction.toolkit@latest)

## First Milestone (Vertical Slice)

Create one fully playable radio room with these interactions:

1. **Replace Fuse**
   - Grab fuse from box
   - Socket into panel with snap-to-point
   - Subtle vibration feedback on successful placement
   - Small visual confirmation (light, sound)

2. **Connect Patch Panel**
   - Grab cable from coiled state
   - Route to two jacks
   - Satisfying click on connection
   - Visual trace shows signal path

3. **Tune Radio Dial**
   - Grab and turn dial (rotational constraint)
   - Distinct click positions at three frequencies
   - Final position triggers: static → voices → story clue

4. **Story Reveal**
   - Subtitled audio of a distress call
   - Static visual feedback (on-screen effect)
   - Clue discovered: "Signal timestamp impossible. Station offline 6 months."
   - Triggers: door unlock, light change, new interactive area opens

### Scope: One Radio Room
- Dimensions: ~4m × 3m × 2.5m (fits seated/standing play)
- Instruments: Radio (tunable), fuse box, patch panel, speakers
- Lighting: Warm practical lighting (desk lamps, instrument glow) + stormy blue window light
- Audio: Ambient storm, radio static, equipment sounds, dialogue with subtitles
- Hand interaction: 3 tool types total (fuse, cable, hand)

## Quality Standards

### Visual Polish
- **Hands**: Detailed, realistic hand poses; believable finger curling
- **Instruments**: Tactile detail on controls; visible wear and patina
- **Lighting**: Baked; no real-time global illumination; controlled shadows
- **Textures**: Compressed appropriately; no high-res for distant objects
- **Effects**: Avoid full-screen post-processing; use subtle local effects only

### Performance
- **Measurement**: Use Unity Profiler + OVR Metrics on actual Quest 3
- **Target**: Maintain 90 FPS consistently; report measured results
- **Never guess**: PC Link performance does not equal Quest standalone
- **Profiling**: Check GPU time, CPU time, memory, thermal state

### Comfort & Accessibility
- **No forced camera movement**: No head bob, shake, or rotation
- **Locomotion**: Teleportation or snap turning (optional smooth movement)
- **Seated support**: All controls within seated arm's reach
- **Accessibility**: Subtitles for all dialogue; visual equivalent for audio cues (flashing light, on-screen icon)
- **Haptics**: Subtle vibration on grab, socket, dial clicks (don't overuse)

### Interaction Design
- **Grab radius**: Generous; clear visual grab points
- **Snapping**: Magnetic snap to socket/slot; 0.1-0.2m range
- **Hand poses**: Natural gripping; fingers curl on grab
- **Feedback**: Sound + haptic + visual on every action
- **Forgiveness**: Miss a socket? Cable curves; try again. Dial overshoots? Smooth turning, can back up.

## Architecture Principles

### Small Components, Reusable Prefabs
- Single-responsibility scripts (one class = one job)
- Interaction logic separate from visual presentation
- Configurable puzzle data in ScriptableObjects, not hard-coded
- Prefabs for: instruments, tools, interaction zones, UI elements

### Save & Load System
- Checkpoint after each major interaction
- Persistent state: which instruments repaired, which cables connected, which clues discovered
- Tests verify save/load preserves puzzle state
- No checkpoint on every frame; checkpoint on meaningful progression

### Version Control
- Commit after each verified feature
- Meaningful commit messages (what changed, why)
- Keep working checkpoints; never push broken code
- Lock package versions in manifest.json once working

### Testing
- Unit tests for puzzle logic (did connection trigger the next state?)
- Integration tests for save/load cycles
- Manual verification on Quest before committing
- Performance test: measure FPS before and after each feature

## Workflow

### Before You Start
1. Verify Unity 6.3 is installed with Quest support
2. Confirm Android SDK, NDK, and Quest command-line tools
3. Create empty URP project or use VR template as base
4. Install XR Interaction Toolkit (latest compatible version)
5. Set up Git on the development branch: `claude/dead-signal-vr-repo-q1yc6u`

### For Each Feature
1. **Design**: What does the player do? What feedback do they get?
2. **Implement**: Write the minimum code needed
3. **Build**: Compile; fix any errors
4. **Test**: Verify on Quest with profiler running
5. **Commit**: Push checkpoint with clear message
6. **Next Feature**: Repeat until vertical slice is complete

### Before Committing
- Run profiler; confirm 90+ FPS on Quest 3
- Test save/load: quit and resume, verify state preserved
- Verify all audio has subtitles or visual alternative
- Ensure no secrets (keys, credentials) in code
- One commit per logical feature, not per file

## What To Avoid

- **Multiplayer or networking**: Out of scope for vertical slice
- **Enemy AI or combat**: Focus on puzzle interaction
- **Live LLM dialogue**: Use pre-recorded audio; ship known state
- **Expensive effects**: No particle systems with >100K particles; no screen-space reflections; no volumetric lighting
- **Camera tricks**: No forced head rotation, teleport within puzzles (except intentional locomotion)
- **Unverified claims**: Never say it's tested on Quest without actual Quest measurements

## File Structure To Maintain

```
Assets/
├── Scenes/RadioRoom.unity
├── Scripts/
│   ├── Interactions/
│   │   ├── GrabbableObject.cs
│   │   ├── SocketZone.cs
│   │   └── RotatableControl.cs
│   ├── Puzzles/
│   │   ├── RadioPuzzle.cs
│   │   └── PuzzleState.cs
│   ├── Saving/
│   │   ├── CheckpointManager.cs
│   │   └── SaveData.cs
│   ├── UI/
│   │   ├── SubtitleDisplay.cs
│   │   └── VisualIndicator.cs
│   └── VR/
│       ├── VRSetup.cs
│       └── HandTrackingManager.cs
├── Prefabs/
│   ├── FuseBox.prefab
│   ├── RadioDial.prefab
│   └── PatchPanel.prefab
├── Materials/
├── Textures/
├── Audio/
└── Resources/
    └── PuzzleConfig.asset
```

## How to Get Help

When stuck, inspect:
1. **Compilation errors**: Fix syntax, missing references
2. **Runtime errors**: Console logs show line number and context
3. **Performance**: Unity Profiler shows CPU/GPU bottlenecks
4. **VR issues**: OVR Metrics shows frame pacing and thermal state

## Success Criteria

- ✅ Builds to APK for Quest 3/3S
- ✅ Maintains 90 FPS on hardware (not PC Link)
- ✅ Player can perform all three interactions without frustration
- ✅ Radio reveals story clue; next room unlocks
- ✅ Save/load preserves puzzle state
- ✅ Subtitles for all dialogue; visual feedback for critical audio
- ✅ No crashes or hangs on Quest
- ✅ All code committed with meaningful messages

## References

- Unity 6.3 LTS: [Release Notes](https://docs.unity.com/Documentation/Manual/whats-new.html)
- XR Interaction Toolkit: [API Reference](https://docs.unity.com/upm/packages/com.unity.xr.interaction.toolkit@latest)
- Meta Quest Developer: [Build for Quest](https://developer.meta.com/docs/quest/latest/concepts/build-add-support/)
- OpenXR: [Specification](https://www.khronos.org/openxr/)

---

**Remember**: Premium polish on a small, focused slice beats a large, unfinished game. Every frame counts; measure on the headset. Start simple, iterate, commit checkpoints, and keep the mystery alive.
