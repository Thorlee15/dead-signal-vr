# Interaction Design - Dead Signal VR

## Design Philosophy

Every interaction in Dead Signal prioritizes **tactile satisfaction** and **physical presence**. The player's hands are the primary interface; controllers fade into the background. Each interaction should feel intentional, responsive, and rewarding.

## Core Interaction Types

### 1. Grabbing (Pick Up Tool)

**Player Goal**: "I want to hold and manipulate this object"

**Interaction Flow**:
```
Look at object
→ Grab point highlights (subtle glow)
→ Reach out and close grip
→ Object follows hand
→ Use or place somewhere else
→ Release grip
```

**Technical Requirements**:
- **Grab radius**: 0.15 m (forgiving but precise)
- **Grab point**: Visually clear (highlight, contrasting material)
- **Feedback**:
  - Visual: Object tints when grabable, glows when gripped
  - Haptic: 20 ms pulse on grab, 50 ms on release
  - Audio: Subtle click on grab (woodsy/metallic depending on object)

**Hand Animation**:
- Fingers curl around handle
- Natural pose (not clenched fist)
- Wrist angle follows controller naturally

**Forgiveness**:
- Miss the grab point? Fingers extend toward nearest point
- Grab too far from handle? Still works within 0.15 m radius
- Object slipping? Haptic feedback signals slippage, player can re-grip

**Examples**:
- Fuse (small cylinder, easy grip)
- Cable (flexible, grabs anywhere along length)
- Dial handle (rotational grip)

---

### 2. Snapping / Socketing (Place Object in Slot)

**Player Goal**: "This object belongs in that slot; make it connect"

**Interaction Flow**:
```
Carry object toward slot
→ Snap-to preview appears (wireframe alignment)
→ Move to final position
→ Release
→ Magnetic snap + satisfying click
→ Visual + audio confirmation
```

**Technical Requirements**:
- **Snap range**: 0.15-0.2 m (generous but not sloppy)
- **Alignment**: Auto-rotate to correct orientation
- **Snap point**: Visual indicator (glow, crosshair)
- **Feedback**:
  - Visual: Object flies into place, fills with light
  - Haptic: Strong 100 ms pulse (satisfying)
  - Audio: "Thunk" sound (fuse) or "Click" (connector)

**Physics**:
- Object teleports to socket on release (no loose animation)
- Becomes kinematic (locked in place until unplugged)
- Can unplug by pulling away hard

**Visual Design**:
- Socket should "invite" placement (lit, glowing edge)
- Preview shows correct orientation before release
- Glow intensifies as you approach

**Examples**:
- Fuse into fuse box slots (vertical insertion)
- Cables into jacks (rotational alignment)
- Tools into holders

---

### 3. Rotation / Turning (Continuous Interaction)

**Player Goal**: "Turn this dial to find the right frequency"

**Interaction Flow**:
```
Grab rotatable handle
→ Constraints prevent non-rotational movement
→ Turn smoothly through range
→ Click-stops at key positions
→ Release
→ Continue spinning if momentum present
```

**Technical Requirements**:
- **Rotation axis**: Fixed (e.g., vertical for dial)
- **Range**: Typically 0–270° (or full 360°)
- **Click stops**: 3–5 detents per rotation (satisfying resistance)
- **Feedback**:
  - Visual: Pointer/label moves, position updates
  - Haptic: 30 ms pulse at each detent
  - Audio: Click sound (mechanical) at each position

**Hand Animation**:
- Wrist rotates naturally
- Fingers stay wrapped around handle
- No arm twist required (comfort for extended play)

**Momentum**:
- Allow slight over-spin past detent
- Snap back to nearest position with haptic feedback
- Prevents accidental detent skips

**Accessibility**:
- No minimum grip strength required
- Can turn slowly (no rushed input)
- Forgiving at boundaries (wrap-around at 360°)

**Examples**:
- Radio frequency dial (3–5 positions for story clues)
- Volume knob (smooth, no stops)
- Valve (quarter-turn, then full rotation)

---

### 4. Multi-Step Interactions (Puzzle Sequences)

**Player Goal**: "Complete this sequence to unlock the next step"

**Example: Radio Tuning Puzzle**
```
Step 1: Insert fuse → Power lights up
Step 2: Connect cables → Signal path visible
Step 3: Tune dial → Static clears, voice heard
Step 4: Listen to clue → New door opens
```

**Design Principles**:
- **Clear progression**: Visual feedback shows what's done, what's next
- **No dead ends**: Every step reversible (can undo, retry)
- **Pacing**: Each step takes 5–20 seconds of focused interaction
- **Reward**: Immediate feedback (light, sound, visual change)

**Feedback at Each Step**:
1. **Step complete**: Glow, chime, haptic pulse
2. **Next step unlocked**: Door light, next socket highlights
3. **Sequence complete**: Ambient music changes, room lighting shifts

---

## Comfort Guidelines

### Camera Movement
- **No forced rotation**: Player turns head, camera follows
- **No bob/sway**: Stable world, even during hand movement
- **No teleport-spin**: Discrete turns are fine (snap turning), but no animated rotation

### Interaction Reach
- **Seated play**: All controls within arm's reach (1.2 m)
- **Standing play**: Controls at natural hand height (0.8–2.0 m)
- **No stretching**: Avoid forcing players to reach above head or behind back

### Speed & Responsiveness
- **<20 ms latency**: Hand tracking to visual update
- **Immediate haptic**: Feedback within 10 ms of interaction
- **No lag on grab**: Object follows hand instantly

### Nausea Prevention
- **No rapid camera rotation**: < 45°/second
- **Smooth locomotion optional**: Teleportation is default (safer)
- **Seated support**: All controls work from chair

---

## Visual Feedback

### Interaction Affordances

| State | Visual | Sound | Haptic |
|-------|--------|-------|--------|
| **Idle** | Object at rest, no glow | Ambient | None |
| **Grabable** | Subtle glow, highlight on near | Slight beep | None |
| **Grabbed** | Tint color shift, bright glow | Pick-up click | 20ms pulse |
| **Carrying** | Bright highlight, particle trail | None | None |
| **Snapable** | Target socket glows, preview wireframe | Approach tone | None |
| **Snapped** | Fill with light, satisfaction glow | Deep thunk | 100ms pulse |
| **In place** | Soft glow, integrated lighting | Hum (if powered) | None |
| **Unplugging** | Glow dims, mechanical resistance | Unplug pop | 50ms pulse |

### Lighting Design

Use **dynamic materials** to show state changes:

```csharp
// Example: GrabbableObject state indicator
private void UpdateMaterialState(InteractionState state)
{
    switch (state)
    {
        case Idle:
            material.SetColor("_EmissionColor", Color.black);
            break;
        case Grabable:
            material.SetColor("_EmissionColor", Color.green * 0.5f);
            break;
        case Grabbed:
            material.SetColor("_EmissionColor", Color.green);
            break;
    }
}
```

### Audio Design

Each interaction has a distinct, brief sound (<300 ms):

| Interaction | Sound | Duration | Tone |
|-------------|-------|----------|------|
| Grab | Soft click | 50 ms | Metallic click |
| Place in socket | Thunk or snap | 100 ms | Deep, satisfying |
| Rotate/click | Mechanical tick | 50 ms | Spring-loaded click |
| Unlock/reveal | Whoosh or chime | 200 ms | Ascending tone |
| Error (can't place) | Buzz | 100 ms | Raspberry/negative |

**Audio Spatialization**:
- Grab sound comes from object location
- Click sounds are near hand
- Ambient room sounds (storm, radio static) fill background

---

## Accessibility Features

### Subtitle System

**Placement**:
- Speaker-relative positioning (float above speaking object)
- World-space, visible from all angles
- High contrast (white text, dark background)

**Content**:
- Every spoken line has subtitle
- Sound direction indicated (arrow pointing to source)
- Emotional tone noted ([whispered], [angry], [static])

**Settings**:
- Font size adjustment (small/medium/large)
- Background opacity control
- Persist duration extension (1–5 seconds)

### Visual Alternatives for Audio Cues

| Audio Cue | Visual Alternative |
|-----------|-------------------|
| Radio static | On-screen TV snow effect (top-right) |
| Distress call | Flashing red indicator + speaker icon |
| Door unlock | Light pulse + haptic feedback |
| Equipment powered | Glow intensifies + hum audio confirmed by subtitle |

### Motor Accessibility

- **One-handed play**: All interactions work with single hand
- **Seated play**: No reaching above shoulders or behind head
- **Adjustable grip**: Loose grip sufficient (no clenching required)
- **Pause anytime**: Can drop tools, step back, no time pressure
- **Undo available**: Can unplug, start over, no lock-outs

---

## Interaction Sequences by Device

### Meta Quest 3 Controllers

**Grab**: Grip button (analog 0–1)
```csharp
public void UpdateGrip(float gripValue)
{
    if (gripValue > 0.8f && !isGrabbed)
        Grab();
    else if (gripValue < 0.2f && isGrabbed)
        Release();
}
```

**Interact**: Trigger button (0–1)
```csharp
public void UpdateTrigger(float triggerValue)
{
    // Optional: secondary interaction (e.g., activate hint)
    if (triggerValue > 0.8f)
        ShowHint();
}
```

**Menu**: Menu button
```csharp
public void OnMenuPressed()
{
    pauseMenu.SetActive(!pauseMenu.activeSelf);
}
```

---

## Interaction Polishing Checklist

For each interaction, verify:

- [ ] **Grab**
  - ✓ Visual highlight appears
  - ✓ Haptic feedback on grab/release
  - ✓ Object follows hand instantly
  - ✓ Audio plays (not too loud)
  
- [ ] **Socket**
  - ✓ Preview shows alignment
  - ✓ Snap occurs within range
  - ✓ Strong haptic pulse on placement
  - ✓ Object stays locked until unplugged

- [ ] **Rotation**
  - ✓ Smooth continuous turn
  - ✓ Click-stops are satisfying
  - ✓ Momentum decay is natural
  - ✓ Can't exceed range

- [ ] **Accessibility**
  - ✓ Subtitles appear for all speech
  - ✓ Visual indicators for audio cues
  - ✓ Works one-handed
  - ✓ All controls within seated reach

- [ ] **Performance**
  - ✓ No frame drops during interaction
  - ✓ Haptic feedback latency <20 ms
  - ✓ Audio plays without crackling
  - ✓ Memory stable over 10-minute play

---

## Common Mistakes to Avoid

❌ **Too tight grab radius** - Players miss objects they're aiming for
✅ **Generous 0.15+ m radius** - Forgiving, natural

❌ **Slow object following hand** - Feels disconnected
✅ **Instant follow, no smoothing** - Responsive, precise

❌ **No feedback on grab** - Player unsure if they grabbed
✅ **Visual + haptic + audio at grab** - Clear, satisfying

❌ **Socket requires perfect alignment** - Frustrating
✅ **Auto-rotate + snap preview** - Smooth, discoverable

❌ **Hard limit on rotation range** - Feels like hitting wall
✅ **Soft boundaries + snap-back** - Natural, forgiving

❌ **Audio without subtitles** - Inaccessible to deaf players
✅ **Subtitles + visual indicators** - Inclusive by design

---

## Testing Interaction Quality

### Heuristic Evaluation

Play through each interaction and rate (1–5):
- **Responsiveness**: Does it feel instant? (target: 5/5)
- **Clarity**: Is intent obvious without tutorial? (target: 4–5/5)
- **Feedback**: Is success confirmed? (target: 5/5)
- **Forgiveness**: Can you recover from mistakes? (target: 4–5/5)
- **Comfort**: No strain or fatigue after 10 minutes? (target: 5/5)

### Playtesting with Users

Recruit 3–5 unfamiliar players and observe:
1. Can they grab objects without guidance?
2. Do they find the socket without hints?
3. Does the rotation feel natural?
4. Do they understand when something is locked vs. unlocked?
5. Do they notice audio cues? Visual alternatives?
6. Any moments of frustration or confusion?

---

## Future Interaction Types (Not in Slice)

Documented for reference; implement only after vertical slice is complete:

- **Cable routing**: Drag cables through cable trays
- **Component swapping**: Unscrew panels, replace circuits
- **Antenna aiming**: Rotate antenna dish using multi-step locks
- **Cryptography puzzle**: Align cipher wheels to decode message
- **Microscope examination**: Inspect details of objects

Each would follow the same design philosophy: tactile, forgiving, immediately rewarding.
