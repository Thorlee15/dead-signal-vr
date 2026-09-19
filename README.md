# Dead Signal - VR Mystery Game for Meta Quest

A premium first-person mystery game for Meta Quest 3 and Quest 3S. The player arrives at an isolated Australian coastal radio station during a storm, repairs equipment, and intercepts an impossible distress call.

## Technical Stack

| Component | Technology |
|-----------|-----------|
| Engine | Unity 6.3 LTS (through December 2027) |
| Language | C# |
| Graphics | Universal Render Pipeline (URP) |
| VR Framework | OpenXR + XR Interaction Toolkit |
| Target Hardware | Meta Quest 3 and 3S |
| Build Target | Android ARM64 (IL2CPP) |
| Frame Rate Target | Native 90 FPS (~11.1 ms per frame) |

## Project Structure

```
dead-signal-vr/
├── Assets/
│   ├── Scenes/
│   │   └── RadioRoom/          # First playable vertical slice
│   ├── Scripts/
│   │   ├── Interactions/       # Grabbing, socketing, turning
│   │   ├── Puzzles/            # Puzzle logic and state
│   │   ├── UI/                 # Subtitles and visual alternatives
│   │   ├── Saving/             # Save/load system
│   │   └── VR/                 # VR setup and configuration
│   ├── Prefabs/
│   │   ├── Instruments/        # Radio, fuse box, patch panel
│   │   ├── Tools/              # Fuse, cables, connectors
│   │   └── UI/                 # In-world UI elements
│   ├── Materials/              # URP materials and effects
│   ├── Textures/               # Compressed and baked assets
│   ├── Audio/                  # Dialogue, ambience, sound effects
│   ├── Animations/             # Hand poses and interactions
│   └── Resources/              # Puzzle configuration data
├── ProjectSettings/
│   └── ProjectVersion.txt      # Unity 6.3 LTS
├── Build/
│   ├── Scripts/                # Build automation (C# and shell)
│   └── APK/                    # Output directory for builds
├── Documentation/
│   ├── TECHNICAL_SETUP.md      # Environment and toolchain setup
│   ├── ARCHITECTURE.md         # System design and components
│   ├── PERFORMANCE.md          # Optimization guidelines
│   ├── INTERACTION_DESIGN.md   # VR interactions and comfort
│   └── BUILD_INSTRUCTIONS.md   # How to build for Quest
├── Tests/
│   └── PuzzleProgressionTests/ # Unit tests for game logic
├── .github/
│   └── workflows/              # CI/CD pipelines
├── CLAUDE.md                   # Instructions for AI agents
└── package-lock.json           # Locked dependency versions
```

## First Milestone

Create one playable radio room with:

- **Interaction System**: Tracked controllers, grabbing, socketing and turning controls
- **Core Loop**: Replace fuse → Connect patch panel → Tune radio → Reveal story clue
- **Polish**: Instrument detail, spatial audio, warm lighting, realistic hand interaction
- **Accessibility**: Subtitles and visual alternatives for essential audio clues
- **Functionality**: Checkpoint saving, restart capability, introduction and completion states

## Development Workflow

1. **Verify toolchain** - Confirm Unity 6.3, Android NDK, and Quest SDK are installed
2. **Build minimal VR room** - Empty scene with tracked controllers and locomotion
3. **Implement one feature at a time** - Single interaction, test, verify, commit
4. **Compile and inspect** - Watch console for errors; use profiler frequently
5. **Checkpoint in Git** - Create meaningful commits after each verified change
6. **Performance first** - Target 90 FPS on Quest 3; use profiler data, not estimates

## Performance Requirements

- **Frame rate**: Native 90 FPS (11.1 ms per frame budget)
- **Graphics**: Baked lighting, efficient materials, compressed textures
- **Camera**: No shake, head bob, or forced rotation
- **Interaction**: Forgiving grab points, sensible snapping, subtle vibration feedback
- **Accessibility**: Teleportation, snap turning, seated play, adjustable reach

## Testing

- **Headset testing** - Measure actual Quest 3 performance with Unity Profiler and OVR Metrics
- **PC Link** - Does not prove standalone performance; always test on actual hardware
- **Puzzle progression** - Automated tests for save/load and state transitions
- **Comfort** - No rapid camera movement, responsive hand tracking, clear audio

## Important Notes

- **Avoid for now**: Multiplayer, enemy AI, live LLM dialogue
- **Credentials**: Keep signing secrets outside source control
- **Assets**: Inspect geometry, collisions, texture sizes and usage rights before shipping
- **Git**: Lock package versions once first build works; keep reproducible build scripts

## Getting Started

See [TECHNICAL_SETUP.md](Documentation/TECHNICAL_SETUP.md) for environment setup instructions.

See [BUILD_INSTRUCTIONS.md](Documentation/BUILD_INSTRUCTIONS.md) for building and deploying to Quest.

## References

- [Unity VR Template Documentation](https://docs.unity.com/upm/packages/com.unity.xr.interaction.toolkit@latest)
- [Meta Quest Developer Documentation](https://developer.meta.com/docs/quest/)
- [OpenXR Specification](https://www.khronos.org/openxr/)
- [OVR Metrics Tool](https://developer.meta.com/documentation/quest/latest/concepts/pc-ovrmetricstool/)
