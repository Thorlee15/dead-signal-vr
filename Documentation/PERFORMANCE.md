# Performance Guidelines - Dead Signal VR

## Target Metrics

| Metric | Target | Headroom |
|--------|--------|----------|
| Frame Rate | 90 FPS | 0% variance |
| Frame Time | 11.1 ms | <1 ms overhead |
| CPU Time | ≤5.5 ms | ~0.5 ms buffer |
| GPU Time | ≤5.5 ms | ~0.5 ms buffer |
| Memory | ≤1.5 GB | 500 MB available |
| Thermal State | Normal | No throttling |

## Profiling Tools

### Unity Profiler (In-Editor)

Use to identify bottlenecks during development:

```
Window → Analysis → Profiler
```

Monitor these categories:
- **CPU**: Script execution, physics, rendering
- **GPU**: Draw calls, shader compilation, memory
- **Memory**: Heap allocation, GC pressure
- **Physics**: Collision detection, constraints

**Decision Point**: If frame time exceeds 12 ms, profile before pushing.

### OVR Metrics Tool (On Quest)

Measures *actual* device performance, not PC Link estimates:

```bash
# Launch while app runs on Quest
ovr-metrics-tool

# Key indicators:
# - Frame Time: Should be <11.1 ms consistently
# - GPU Time: Separate from CPU; both matter
# - Thermal State: Should stay "Normal"
# - Power: Should not exceed 10W sustained
```

**Decision Point**: If thermal state is "High" or "Critical", stop and optimize.

## CPU Optimization

### Script Performance Budgets

| System | Budget | Notes |
|--------|--------|-------|
| Input processing | 0.2 ms | Hand tracking, button detection |
| Physics | 0.5 ms | Kinematic hand, object gravity, snapping |
| Puzzle logic | 0.3 ms | State checks, event callbacks |
| Audio | 0.5 ms | Spatial audio updates |
| Animation | 0.4 ms | Hand poses, UI transitions |
| Misc (GC, profiler) | 3.1 ms | System overhead |
| **Total CPU** | **5.5 ms** | |

### Profiling Script Performance

```csharp
// Use Profiler.BeginSample for hot sections
Profiler.BeginSample("GrabLogic");
DetectHandProximity();
UpdateHandPose();
Profiler.EndSample();
```

### Common CPU Bottlenecks

1. **Physics queries every frame**
   ```csharp
   // ❌ Bad: Physics query in Update()
   void Update() { Physics.OverlapSphere(...); }
   
   // ✅ Good: Query on-demand or cache
   void OnGrabAttempt() { Physics.OverlapSphere(...); }
   ```

2. **String operations in loops**
   ```csharp
   // ❌ Bad: String concatenation per frame
   foreach (var item in items)
       label.text = "Item: " + item.name; // Allocation!
   
   // ✅ Good: Use StringBuilder or avoid per-frame updates
   label.text = StringFormat(item.name);
   ```

3. **Unoptimized coroutines**
   ```csharp
   // ❌ Bad: Allocates new WaitForSeconds
   yield return new WaitForSeconds(0.1f);
   
   // ✅ Good: Reuse or calculate exact frame count
   private WaitForSeconds wait010 = new(0.1f);
   yield return wait010;
   ```

## GPU Optimization

### Rendering Budget

| Component | Budget | Notes |
|-----------|--------|-------|
| Opaque geometry | 3.0 ms | Instruments, room, static props |
| Hands/Controllers | 1.2 ms | Detailed mesh, animated |
| Transparents (UI) | 1.0 ms | Subtitles, overlays |
| Post-processing | 0.8 ms | Minimal effects only |
| **Total GPU** | **6.0 ms** | Room for headroom |

### Draw Call Optimization

**Target**: ≤50 draw calls per frame on Quest

```csharp
// Check draw calls in Editor
// Window → Analysis → Profiler → GPU
```

**Strategies**:
1. **Static batching** (geometry never moves)
   ```csharp
   // Mark in inspector: Mesh Renderer → Optimize → Static
   // Or in code:
   StaticBatchingUtility.Combine(gameObjects, parent);
   ```

2. **Dynamic batching** (for moving objects with same material)
   ```
   Edit → Project Settings → Player → Rendering
   → Enable Dynamic Batching
   → Set Batch Threshold to 300 vertices
   ```

3. **Material atlasing** (combine textures)
   - Fuse box + radio dial + patch panel → single 2K atlas
   - Hands + tools → single texture atlas
   - Saves switching materials between draw calls

4. **LOD groups** (swap mesh quality by distance)
   ```csharp
   // Add LOD Group to complex meshes
   // Level 0: Full detail (<2m)
   // Level 1: Reduced detail (2-5m)
   // Level 2: Simple (5m+, usually off-screen)
   ```

### Shader Optimization

**URP shader guidelines**:
- Use built-in Universal shaders (already optimized)
- Avoid custom post-processing in ForwardRenderer
- No real-time GI, volumetric fog, or screen-space effects

**Example**: Radio dial glowing without expensive bloom
```hlsl
// Use emissive color instead of bloom
// Material property: Emission = (0.2, 0.8, 0.1)
// URP renders it directly; no full-screen pass needed
```

## Memory Optimization

### Budget: 1.5 GB Sustainable

| Category | Allocation | Notes |
|----------|-----------|-------|
| Code (IL2CPP) | 100 MB | Compiled C# + libraries |
| Scene assets | 300 MB | Geometry, baked textures |
| Audio streams | 200 MB | Compressed OGG/MP3 |
| Textures (GPU) | 400 MB | Compressed in VRAM |
| Buffers (framebuf) | 100 MB | Depth, color, post-fx |
| Runtime heap | 400 MB | Particles, objects, profiler |
| **Total** | **1500 MB** | With 500 MB headroom |

### Texture Compression

**Quest uses ETC2 / ASTC compression**:

| Texture Type | Compression | Size |
|--------------|-------------|------|
| Color (opaque) | ETC2 RGB | 0.5 MB per 2K |
| Normal maps | ETC2 RG | 0.25 MB per 2K |
| Specular/mask | ETC2 RGBA | 1 MB per 2K |
| Complex alpha | ASTC 6x6 | 0.5 MB per 2K |

**Import settings**:
```
Select texture → Inspector
Compression: ETC2 (Quest) or ASTC (Android)
Max Texture Size: 2048 (never 4096 on Quest)
```

### Audio Compression

```
Select audio clip → Inspector
Compression Format: Vorbis (OGG)
Quality: 50% (acceptable speech/FX)
Sample Rate: 48 kHz (native for Quest)
Channels: Mono (dialogue), Stereo (ambience)
```

## Thermal Throttling Prevention

### Monitor Thermal State
```bash
# While app runs
ovr-metrics-tool

# Watch "Thermal State" indicator
# Green = Normal
# Yellow = Warm (monitor, may throttle soon)
# Red = Critical (throttling, reduce workload immediately)
```

### Causes of Overheating
1. **Busy loop without sleep**
   ```csharp
   // ❌ Bad: Max CPU usage
   while (running) { DoWork(); }
   
   // ✅ Good: Frame-limited
   void Update() { DoWork(); }
   ```

2. **Inefficient physics simulation**
   ```csharp
   // Reduce default FixedTimestep
   // Edit → Project Settings → Physics
   // Set to 0.010 (90 FPS = 11.1 ms frame, 10 ms physics tick)
   ```

3. **Real-time lighting**
   - Use only baked lights for base illumination
   - Limit dynamic lights to 2-3 maximum

4. **Particles or expensive effects**
   - Limit particle count to <50K total
   - No bloom, motion blur, or full-screen effects

## Performance Testing Workflow

### Before Each Commit

1. **In-Editor test** (catch regressions early)
   ```
   Open scene → Play → Run Profiler
   Check: Frame time <12 ms, memory stable
   ```

2. **Build APK**
   ```bash
   File → Build Settings → Build (to Build/APK/)
   ```

3. **Deploy to Quest**
   ```bash
   adb install -r Build/APK/DeadSignal.apk
   adb shell am start -n com.deadsignal.vr/...MainActivity
   ```

4. **Measure real performance**
   ```bash
   # Terminal 1: Profiler
   ovr-metrics-tool
   
   # Terminal 2: Log stream (optional, for debugging)
   adb logcat -s UnityPlayer
   
   # Play through interaction for 30 seconds
   # Watch: Frame Time, Thermal, Frames
   ```

5. **Verify 90 FPS**
   - Screenshot OVR Metrics showing stable 11.1 ms frame time
   - No frame drops below 90 FPS
   - Thermal state stays Normal

6. **Commit only if passing**
   ```bash
   git add .
   git commit -m "Optimize interaction feedback performance"
   ```

## Performance Regression Detection

### Automated Checks

```csharp
// Assets/Tests/PerformanceTests.cs
[UnityTest, Performance]
public IEnumerator GrabbableObject_NoFrameTimeIncrease()
{
    var obj = Object.Instantiate(grabPrefab);
    
    using (Measure.Frames().Scope())
    {
        for (int i = 0; i < 300; i++)
            yield return null;
    }
    // Test fails if avg frame time > baseline + threshold
}
```

### Baselines to Track

After first successful build:
- Frame time: 10.5 ± 0.3 ms (your baseline ± variance)
- Draw calls: 45 ± 5
- Memory: 1200 ± 50 MB

If any new feature causes:
- Frame time +0.5 ms → Profile and optimize
- Draw calls +10 → Batch or reduce geometry
- Memory +100 MB → Check asset sizes, pooling

## Optimization Checklist

Before submitting build:

- [ ] Frame time stable at 11.1 ms ± 0.5 ms
- [ ] No frame drops below 90 FPS during 2-minute play
- [ ] Thermal state Normal throughout (no Yellow/Red)
- [ ] Memory usage <1.3 GB (leaves 200 MB buffer)
- [ ] Draw calls <50 per frame
- [ ] No memory spikes from GC allocations
- [ ] All audio plays without distortion at normal volume
- [ ] Hand tracking responsive (sub-10 ms latency)

---

## Quick Optimization Wins

1. **Disable shadows** on distant objects
   ```
   Light component → Disable Cast Shadows on non-critical lights
   ```

2. **Use object pooling** for effects
   ```csharp
   // Reuse vibration feedback objects instead of Instantiate()
   private Queue<HapticEffect> hapticPool = new();
   ```

3. **Cache physics queries**
   ```csharp
   // Calculate once per placement, not per frame
   private Collider[] socketCandidates;
   void UpdateSocketCandidates() { socketCandidates = Physics.OverlapSphere(...); }
   ```

4. **Profile with actual device metrics**
   - PC Link ≠ Quest standalone
   - Always verify on hardware before declaring success
