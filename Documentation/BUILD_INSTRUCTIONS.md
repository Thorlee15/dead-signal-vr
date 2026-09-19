# Build Instructions - Dead Signal VR

## Quick Start (5 minutes)

### Prerequisites
- Unity 6.3 LTS installed with Android support
- Android SDK/NDK (installed via Unity Hub)
- Meta Quest 3 or 3S connected via USB
- Developer Mode enabled on headset

### Build Steps

```bash
# 1. Connect Quest and verify connection
adb devices
# Expected: device ID listed as "device"

# 2. In Unity Editor:
#    File → Build Settings → Android
#    Add scene: Assets/Scenes/RadioRoom.unity
#    Player Settings → Identification:
#      Package Name: com.deadsignal.vr
#      Version Code: 1
#      Version: 1.0
#    Click "Build"

# 3. Output appears at Build/APK/DeadSignal.apk

# 4. Deploy to Quest
adb install -r Build/APK/DeadSignal.apk

# 5. Launch app
adb shell am start -n com.deadsignal.vr/com.deadsignal.vr.MainActivity

# 6. Monitor performance
ovr-metrics-tool
```

## Step-by-Step Build Guide

### 1. Prepare Development Environment

Verify all tools are installed:

```bash
# Check Java (required for Android build)
java -version
# Expected: OpenJDK or JDK 11+

# Check Android SDK
adb --version
# Expected: Android Debug Bridge version

# Check Android NDK (Unity uses it)
ls $ANDROID_NDK_ROOT/bin
# Expected: List of build tools
```

If any missing, see [TECHNICAL_SETUP.md](TECHNICAL_SETUP.md).

### 2. Verify Quest Connection

```bash
# Enable Developer Mode on Quest
# Settings → System → About → Build Number (tap 7 times)
# Settings → System → Developer → USB Debugging (toggle on)

# Connect via USB cable

# Verify in terminal
adb devices
# Expected output:
# List of attached devices
# d1a2b3c4e5f6    device
```

If Quest not listed:
- Unplug and reconnect USB
- Accept "Allow USB debugging?" prompt on headset
- Try: `adb kill-server && adb start-server`

### 3. Configure Unity Project for Android

Open project in Unity Editor:

**File → Build Settings:**

1. Click "Android" platform (left panel)
2. Click "Switch Platform" (may take 1-2 minutes)
3. **Scenes in Build**: 
   - Remove any existing scenes
   - Add: `Assets/Scenes/RadioRoom.unity`

4. **Player Settings** (gear icon):
   - **Resolution and Presentation**
     - Default Screen Width: 1080
     - Default Screen Height: 1200
     - Default Screen Orientation: Portrait
   
   - **Identification**
     - Package Name: `com.deadsignal.vr` (must be unique on device)
     - Version Code: `1` (increment for each build)
     - Version: `1.0`
     - API Level: `API 29` (Android 10+)
   
   - **XR Settings**
     - Graphics APIs: OpenGL ES 3.0 + Vulkan
     - Rendering Path: Forward
     - GPU Instancing: Enabled

5. **Publishing Settings** (if signing):
   - Keystore: Set path to signing key (see below)
   - Key Alias: Select key
   - Key Password: (stored securely, not in code)

### 4. Build APK

#### Unsigned Build (Testing Only)

```bash
# In Editor: File → Build Settings → Build
# Choose location: Build/APK/
# Filename: DeadSignal.apk
# Wait for compilation (~2-5 minutes depending on project size)
```

Output: `Build/APK/DeadSignal.apk` (unsigned, debug)

#### Signed Build (Required for Release)

```bash
# 1. Create signing key (one-time)
keytool -genkey -v -keystore build_secrets/deadsignal.keystore \
  -keyalg RSA -keysize 2048 \
  -validity 10000 \
  -alias deadsignal_key \
  -storetype JKS

# 2. Configure Player Settings
#    Player Settings → Publishing Settings
#    Keystore: build_secrets/deadsignal.keystore
#    Key Alias: deadsignal_key
#    Store Password: (from step 1)
#    Key Password: (from step 1)

# 3. Build in Editor (builds signed automatically)
#    File → Build Settings → Build
```

**⚠️ Important**: Keep `build_secrets/` directory in `.gitignore`. Never commit keystore or passwords.

### 5. Deploy to Quest

```bash
# Install APK
adb install -r Build/APK/DeadSignal.apk
# Wait for "Success" message

# Verify installation
adb shell pm list packages | grep deadsignal
# Expected: com.deadsignal.vr

# Launch app
adb shell am start -n com.deadsignal.vr/com.deadsignal.vr.MainActivity

# Put on headset and test
```

### 6. Monitor Performance

**While app runs on Quest:**

Terminal 1 - Performance profiler:
```bash
ovr-metrics-tool
```

Watch these values:
- **Frame Time**: Should be ~11.1 ms (90 FPS)
- **CPU Time**: Should be <5.5 ms
- **GPU Time**: Should be <5.5 ms
- **Thermal State**: Should be "Normal" (green)
- **Power**: Should stay <10W

Terminal 2 - Live logs (optional):
```bash
adb logcat -s "UnityPlayer" | grep -E "ERROR|Exception"
```

### 7. Verify Functionality

On headset:
1. Grab and place fuse (should snap into slot)
2. Connect cables to patch panel (should click into place)
3. Turn radio dial (should click at three positions)
4. Final position triggers distress call audio
5. Door unlocks, lights change, scene is complete

If something doesn't work:
- Check logcat output (Terminal 2 above)
- Profiler shows frame drops → optimize (see PERFORMANCE.md)
- Interaction not registering → verify XR Interaction Toolkit installed

## Automated Build Scripts

### Shell Script (Linux/macOS)

```bash
#!/bin/bash
# build_quest.sh - Automated APK build and deploy

set -e

echo "Building APK for Meta Quest..."
unity -projectPath . \
  -executeMethod BuildPipeline.BuildPlayer \
  -customBuildPath Build/APK/DeadSignal.apk \
  -buildTarget Android \
  -quit

echo "APK built successfully."
echo "Installing on Quest..."
adb install -r Build/APK/DeadSignal.apk

echo "Launching app..."
adb shell am start -n com.deadsignal.vr/com.deadsignal.vr.MainActivity

echo "App started. Check headset."
```

### C# Editor Script

```csharp
// Assets/Editor/BuildMenu.cs
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildMenu
{
    [MenuItem("Build/Build APK for Quest")]
    public static void BuildAPK()
    {
        var scenes = new[] { "Assets/Scenes/RadioRoom.unity" };
        var outputPath = "Build/APK/DeadSignal.apk";
        
        var buildResult = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);
        
        if (buildResult.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {outputPath}");
        }
        else
        {
            Debug.LogError($"Build failed: {buildResult.summary.totalErrors} errors");
        }
    }
}
```

To use:
1. Save as `Assets/Editor/BuildMenu.cs`
2. In Editor: **Build → Build APK for Quest**
3. Automatically deploys to Quest if connected

## Troubleshooting

### Build Fails: "NDK not found"
```bash
# Verify NDK is installed
ls $ANDROID_NDK_ROOT

# If missing, Unity can install it
# Edit → Preferences → External Tools
# Android NDK: Click "Install"
```

### Build Fails: "No scenes in build"
```bash
# Add scene to build
File → Build Settings
Drag scene or click "Add Open Scene"
```

### APK Won't Install
```bash
# Clear previous installation
adb uninstall com.deadsignal.vr

# Try install again
adb install -r Build/APK/DeadSignal.apk

# Check device storage
adb shell df /data
# If full, uninstall other apps from Quest
```

### App Crashes on Launch
```bash
# Check logcat
adb logcat -s "UnityPlayer" | grep -i "error\|exception"

# Common issues:
# - XR Interaction Toolkit not installed
# - Scene missing or misconfigured
# - Missing required assets

# Rebuild scene and try again
```

### Low FPS on Quest
See [PERFORMANCE.md](PERFORMANCE.md) for optimization guide.

Common culprits:
- Too many draw calls (>100)
- GPU shaders not optimized
- Real-time shadows enabled
- High-res textures not compressed

### Thermal Throttling
```bash
# Monitor with OVR Metrics
ovr-metrics-tool

# If Thermal State turns yellow/red:
# 1. Close background apps on Quest
# 2. Let device cool (5-10 minutes)
# 3. Optimize performance (reduce draw calls, lower shader complexity)
# 4. Rebuild and test
```

## Continuous Integration (GitHub Actions)

Example workflow to auto-build on push:

```yaml
# .github/workflows/build-quest.yml
name: Build for Quest

on: [push]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: game-ci/unity-builder@v4
        with:
          unityVersion: 2023.2.0f1
          targetPlatform: Android
          buildName: DeadSignal
          buildPath: Build/APK
      - uses: actions/upload-artifact@v3
        with:
          name: APK
          path: Build/APK/DeadSignal.apk
```

After push, APK is built automatically and available as artifact.

## Checklist Before Shipping

- [ ] Builds successfully without warnings
- [ ] All scenes included in build settings
- [ ] APK signed with production key (if shipping)
- [ ] No debug logs left in code
- [ ] 90 FPS verified on actual Quest 3
- [ ] No thermal throttling after 15-minute play
- [ ] All interactions tested and responsive
- [ ] Subtitles and accessibility verified
- [ ] Save/load checkpoint works
- [ ] Version code incremented
- [ ] Git tags created for release

## Release Build Example

```bash
# 1. Update version
# File → Build Settings → Player Settings
# Version Code: 2
# Version: 1.1

# 2. Tag commit
git tag -a v1.1 -m "Release: improved hand tracking"
git push origin v1.1

# 3. Create signed build
# (Configure signing in Player Settings first)
# File → Build Settings → Build

# 4. Verify on actual device
adb install -r Build/APK/DeadSignal.apk

# 5. Submit to Meta Quest app store (manual process)
# (Requires Meta developer account and store setup)
```

---

For more details on setup, see [TECHNICAL_SETUP.md](TECHNICAL_SETUP.md).
