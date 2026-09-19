# Architecture - Dead Signal VR

## System Overview

Dead Signal is built on modular, reusable components that separate interaction logic from visual presentation. This allows puzzle state to persist independently of animations, hand poses, or visual feedback.

## Core Systems

### 1. Interaction System

**Responsibility**: Detect player hands, grab objects, manage constraints

**Key Classes**:
- `GrabbableObject.cs` - Makes any object pickupable with haptic feedback
- `SocketZone.cs` - Defines where objects can be placed (fuse slots, cable jacks)
- `RotatableControl.cs` - Constrains objects to rotate around an axis (dial, valve)
- `InteractionFeedback.cs` - Unified haptic and audio response

**Flow**:
```
Hand proximity → Grab zone highlights
Player grabs → Object follows hand
Object near socket → Visual snap preview
Player releases over socket → Magnetic snap + haptic feedback
```

### 2. Puzzle System

**Responsibility**: Track puzzle state, check win conditions, trigger narrative events

**Key Classes**:
- `PuzzleState.cs` - Serializable data container (which items placed, which signals tuned)
- `RadioPuzzle.cs` - Logic for radio tuning puzzle (sequence, unlocks, audio playback)
- `PuzzleProgressionManager.cs` - Orchestrates unlock sequence across the room

**State Machine**:
```
[Initial] → Fuse in place → [Powered]
[Powered] → Cables connected → [SignalPath]
[SignalPath] → Radio tuned → [Clue Found]
[ClueFound] → Door unlocks, lights change, next area opens
```

**Checkpoint**: After each state transition, save to disk

### 3. Saving System

**Responsibility**: Persist puzzle state, allow mid-session resume

**Key Classes**:
- `CheckpointManager.cs` - High-level save/load interface
- `SaveData.cs` - Serializable data structure (what's placed, what's discovered)
- `SaveDataSerializer.cs` - JSON serialization (platform-agnostic)

**Checkpoint Format**:
```json
{
  "timestamp": 1695321600,
  "puzzleState": {
    "fuseInstalled": true,
    "cableConnected": [true, false],
    "radioTunedTo": 2,
    "clueDiscovered": true
  },
  "playerPosition": [0, 1.6, 0],
  "handPoses": {...}
}
```

**Restore Flow**:
```
Load save file → Deserialize → Restore object positions → Trigger puzzle callbacks → Player continues
```

### 4. VR Locomotion & Comfort

**Responsibility**: Safe, comfortable player movement and view

**Key Classes**:
- `VRSetup.cs` - XR Rig configuration, controller mappings
- `TeleportationLocomotion.cs` - Discrete movement (default for comfort)
- `SnapTurning.cs` - Discrete rotation (45° increments)
- `SeatedPlaySupport.cs` - Adjusts interaction reach for seated players

**No Forced Camera Movement**: 
- No head bob, camera shake, or rotation animations
- All movement is player-initiated or natural physics

### 5. UI & Accessibility

**Responsibility**: Convey critical information without relying solely on audio

**Key Classes**:
- `SubtitleDisplay.cs` - World-space subtitles (follow speaker or fix to screen edge)
- `VisualIndicator.cs` - Non-audio feedback (flashing light, on-screen icon)
- `AccessibilitySettings.cs` - Toggle subtitle size, indicator brightness

**Visual Alternatives**:
- Radio static → On-screen TV snow effect
- Distress call heard → Flashing red indicator + speaker icon
- Door unlocking → Light effect + haptic pulse

## Data Flow Diagram

```
Input (Controller/Hand)
    ↓
Interaction System (Grab, Place, Rotate)
    ↓
Puzzle System (Check state, trigger events)
    ↓
Save System (Persist checkpoint)
    ↓
UI System (Display feedback, subtitles)
    ↓
Audio & Haptics (Play sounds, vibrate)
    ↓
Visual Presentation (Animate objects, light changes)
```

## Component Communication

### Without coupling:

**Interaction System** sends events:
```csharp
public class GrabbableObject
{
    public event System.Action OnGrabbed;
    public event System.Action OnReleased;
    public event System.Action<SocketZone> OnSocketed;
}
```

**Puzzle System** listens:
```csharp
public class RadioPuzzle
{
    private void OnFuseSocketed()
    {
        puzzleState.fuseInstalled = true;
        CheckProgress();
    }
}
```

**Save System** observes puzzle state:
```csharp
private void OnPuzzleStateChanged()
{
    checkpointManager.SaveCheckpoint(puzzleState);
}
```

### Benefits:
- Puzzle logic doesn't care *how* fuse was placed (hand, robot, debug menu)
- Same puzzle data can be displayed differently (UI, AR, audio description)
- Easy to test puzzle logic independently of graphics

## Prefab Hierarchy

### FuseBox Prefab
```
FuseBox (root)
├── Body (mesh renderer - painted metal)
├── FuseSlot1 (SocketZone collider)
├── FuseSlot2 (SocketZone collider)
├── Light (glow increases when powered)
└── AudioSource (hum when powered)
```

### RadioDial Prefab
```
RadioDial (root)
├── Dial (rotatable mesh, RotatableControl constraint)
│   ├── Handle (grabable, child of dial)
│   ├── Pointer (child indicator)
│   └── FrequencyLabel (UI canvas)
├── Frequency1 (audio/vibration point)
├── Frequency2 (audio/vibration point)
├── Frequency3 (audio/vibration point - clue broadcast)
└── Speaker (audio source)
```

### RadioRoom Scene
```
RadioRoom (root)
├── XRRig (player rig with controllers)
├── Environment (baked lighting, static geometry)
├── Instruments (FuseBox, RadioDial, PatchPanel instances)
├── UI Canvas (subtitles, hints, accessibility)
├── Audio Manager (spatial audio control)
├── Save Manager (checkpoint system)
└── Lighting (baked directional + spot lights)
```

## Performance Considerations

### GPU Budget (11.1 ms @ 90 FPS)
- **Rendering**: 5-6 ms (batched draws, URP optimizations)
- **Physics**: 1-2 ms (kinematic hands, no ragdoll)
- **Audio**: ~0.5 ms (spatial, compressed streams)

### Memory Budget (~2 GB on Quest 3)
- Scene: ~300 MB (geometry, baked textures)
- Code: ~100 MB (IL2CPP compiled)
- Audio: ~200 MB (speech, ambience, effects)
- Runtime: ~400 MB (frame buffers, physics, profiler overhead)

### Optimization Strategies
- **Baked lighting**: Pre-computed shadows, no real-time lights
- **Texture compression**: ETC2 for opaque, ASTC for complex alpha
- **Mesh optimization**: 15-20K tris per instrument, <50K total in view
- **Draw call batching**: Material atlasing, static batching where possible
- **No expensive effects**: No bloom, volumetric fog, or screen-space reflections

## Testing Strategy

### Unit Tests
```csharp
// Example: PuzzleProgressionTests.cs
[Test]
public void FuseInstallation_UnlocksPower()
{
    var puzzle = new RadioPuzzle();
    puzzle.InstallFuse();
    Assert.IsTrue(puzzle.IsPowered);
}

[Test]
public void SaveLoad_PreservesPuzzleState()
{
    var state = new PuzzleState { fuseInstalled = true };
    var saved = CheckpointManager.Save(state);
    var loaded = CheckpointManager.Load(saved);
    Assert.AreEqual(state.fuseInstalled, loaded.fuseInstalled);
}
```

### Integration Tests
1. Place fuse → Verify power icon lights up
2. Connect cables → Verify audio signal path
3. Tune radio → Verify clue plays and door unlocks

### Performance Tests
- Profiler: Record frame time before and after each feature
- Device profiler: OVR Metrics reports actual Quest performance
- Regression: Compare build-to-build FPS, draw calls, memory

### Comfort Tests (Headset Required)
- No nausea, dizziness, or discomfort after 15 minutes
- Clear grab affordances; no guessing where to grab
- Responsive haptic feedback on all interactions

## Future Expansion Points

These are maintained but *not* implemented in the vertical slice:

- **Multi-room navigation**: Teleportation between radio room, equipment room, antenna
- **Additional puzzles**: Shortwave tuning, antenna alignment, cryptography
- **Environmental narrative**: Photos, logs, correspondence around the room
- **Performance optimization**: LOD system, streaming, aggressive batching
- **Multiplayer**: Co-op puzzle solving (future scope, not MVP)

## Dependencies & Versions

Locked versions (update only after testing):

```json
{
  "com.unity.xr.openxr": "1.10.1",
  "com.unity.xr.interaction.toolkit": "3.0.4",
  "com.unity.render-pipelines.universal": "14.0.10",
  "com.meta.xr.sdk": "64.0.0",
  "com.unity.inputsystem": "1.7.0"
}
```

Update flow:
1. Research compatibility with Unity LTS release notes
2. Test in a branch
3. Run full performance suite
4. Only merge if 90 FPS maintained and no new bugs

---

## Quick Reference

**Add a new interaction (e.g., valve turning):**
1. Create rotatable prefab with RotatableControl script
2. Add interaction event to PuzzleState (valvePosition)
3. Listen for event in puzzle logic
4. Add checkpoint save/load
5. Test on Quest, commit

**Add visual feedback (e.g., new light):**
1. Add to scene; assign material
2. Bind to PuzzleState callback (e.g., `OnPowered`)
3. Animate smoothly (fade, not blink)
4. No frame rate regression check
5. Commit

**Add new audio clue:**
1. Record or generate, compress to MP3/OGG
2. Add AudioClip to Resources/Audio
3. Reference in dialogue system
4. Generate subtitles, add to SubtitleDisplay
5. Test on Quest with volume normalized
6. Commit
