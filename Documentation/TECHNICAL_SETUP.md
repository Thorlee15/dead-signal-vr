# Technical Setup - Dead Signal VR

## System Requirements

### Development Machine
- **OS**: Windows 11+, macOS 12+, or Linux (Ubuntu 20.04+)
- **RAM**: 16 GB minimum (32 GB recommended)
- **Disk**: 100+ GB free (for Unity, Android SDK, build outputs)
- **GPU**: Dedicated GPU recommended (for editor performance)

### Meta Quest Hardware
- **Target**: Quest 3 or Quest 3S
- **Firmware**: Latest stable version

## Installation Checklist

### 1. Unity 6.3 LTS

Download and install Unity 6.3 LTS via [Unity Hub](https://unity.com/download):

```bash
# Verify installation
unity --version
```

When installing, include:
- Android SDK
- Android NDK (r26+)
- OpenJDK

### 2. Android Development Tools

#### Via Unity Hub (Recommended)
Unity's Android setup installs everything needed.

#### Manual Setup (Linux/macOS)
```bash
# Install Android SDK Platform Tools
# macOS: brew install android-sdk
# Linux: Follow platform-specific guides

# Set environment variables
export ANDROID_SDK_ROOT=~/Library/Android/sdk  # macOS
# or
export ANDROID_SDK_ROOT=~/Android/sdk  # Linux

# Install NDK r26 or later
# Add to PATH: $ANDROID_SDK_ROOT/ndk/26.0.10665212/bin
```

#### Verify Setup
```bash
# Check adb is available
adb --version

# Check NDK path
echo $ANDROID_NDK_ROOT
```

### 3. Meta Quest SDK

Download from [Meta Developer Dashboard](https://developer.meta.com/):

- **Meta Quest Developer Hub** (optional, for wireless building)
- **Command-line tools** for building and deploying APKs

```bash
# Add to PATH
export PATH="$PATH:~/Android/OVR/tools/bin"
```

### 4. Git Configuration

```bash
# Clone or navigate to the project
git clone <repository-url> dead-signal-vr
cd dead-signal-vr

# Create feature branch
git checkout -b claude/dead-signal-vr-repo-q1yc6u

# Configure git for this project
git config user.name "Claude Code"
git config user.email "noreply@anthropic.com"
```

### 5. Create Unity Project

If starting fresh:

```bash
# Use Unity VR template for Quest
unity -createProject \
  -templatePath "Built-in VR" \
  ./DeadSignalVR \
  -unityVersion 2023.2.0f1
```

Or open existing project and add VR support via **Edit → Project Settings → XR Plug-in Management**.

## Project Configuration

### Unity Editor Settings

**Edit → Project Settings:**

1. **Player**
   - Default Is Native (checked)
   - Target Architectures: ARM64 only
   - Scripting Backend: IL2CPP
   - API Level: API 29+ (Android 10+)

2. **XR Plug-in Management**
   - Enable OpenXR
   - Add Meta Quest support
   - Select Quest 3 / 3S as target

3. **Graphics**
   - Rendering Path: Forward (not Deferred)
   - Use Universal Render Pipeline (URP)

4. **Quality**
   - MSAA: 2x or 4x (not 8x for Quest)
   - Anisotropic Filtering: Per texture
   - Disable: Bloom, Ambient Occlusion, Screen Space Shadows

5. **Editor → Preferences → External Tools**
   - Android NDK: Set to installed NDK location
   - Android SDK: Set to installed SDK location
   - Java Development Kit: Set to JDK location

### Package Dependencies

Edit `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.unity.xr.openxr": "1.10.1",
    "com.unity.xr.interaction.toolkit": "3.0.4",
    "com.unity.render-pipelines.universal": "14.0.10",
    "com.meta.xr.sdk": "64.0.0",
    "com.unity.inputsystem": "1.7.0"
  }
}
```

**Important**: Lock these versions once the first build works. Do not auto-update.

## Building for Quest

### Step 1: Connect Quest
```bash
# Enable Developer Mode on Quest
# (Settings → System → Developer)

# Connect via USB
adb devices

# Expected output:
# d1a2b3c4e5f6    device
```

### Step 2: Build APK from Editor

**File → Build Settings:**

1. Select "Android" platform
2. Scenes: Add `Assets/Scenes/RadioRoom.unity`
3. Player Settings → Identification:
   - Package Name: `com.deadsignal.vr`
   - Version Code: 1
   - Version: 1.0

4. Click **Build** (not Build & Run, initially)
5. Output: `Build/APK/DeadSignal.apk`

### Step 3: Deploy & Run

```bash
adb install -r Build/APK/DeadSignal.apk

# Launch the app
adb shell am start -n com.deadsignal.vr/com.deadsignal.vr.MainActivity
```

### Step 4: Monitor Performance

While running, in one terminal:

```bash
# Monitor FPS and thermal state
ovr-metrics-tool
```

In another terminal:

```bash
# Stream logs from Quest
adb logcat -s "UnityPlayer"
```

## Development Loop

### After Each Code Change

1. **Editor Build & Run** (in editor, to catch syntax errors quickly)
2. **Build APK** (if editor test passes)
3. **Deploy to Quest** (if APK builds successfully)
4. **Measure Performance** (with OVR Metrics)
5. **Commit to Git** (if 90 FPS maintained and feature works)

### Example Workflow

```bash
# Build in Unity Editor first (catch compile errors)
# Open Build → Run in Editor, test scene

# If successful, build for Android:
# File → Build Settings → Build (to Build/APK/)

# Deploy:
adb install -r Build/APK/DeadSignal.apk
adb shell am start -n com.deadsignal.vr/com.deadsignal.vr.MainActivity

# Monitor:
ovr-metrics-tool  # In one terminal
adb logcat -s UnityPlayer  # In another

# After verifying performance and behavior:
git add .
git commit -m "Add fuse interaction system"
git push -u origin claude/dead-signal-vr-repo-q1yc6u
```

## Troubleshooting

### Build Fails with NDK Error
```bash
# Verify NDK is installed
ls $ANDROID_NDK_ROOT

# If missing, Unity's Android setup can install it
# Or manually download from Android Developers site
```

### APK Won't Install
```bash
# Check device is connected and in developer mode
adb devices

# Clear old app data if reinstalling
adb uninstall com.deadsignal.vr

# Try install again
adb install -r Build/APK/DeadSignal.apk
```

### Low FPS on Quest
1. Open OVR Metrics while app runs
2. Check CPU time (should be <5.5 ms per frame)
3. Check GPU time (should be <5.5 ms per frame)
4. If either exceeds budget, profile in Unity Profiler
5. Common culprits: too many draw calls, unoptimized shaders, memory pressure

### Input Not Working
1. Ensure XR Interaction Toolkit is installed
2. Verify **Edit → Project Settings → XR Plug-in Management → OpenXR** is enabled
3. Check **Edit → Project Settings → Input System** (old input system may conflict)
4. In-editor testing: Use XR Device Simulator to simulate controllers

## Next Steps

1. ✅ Complete this setup
2. Open `Assets/Scenes/RadioRoom.unity` (or create it)
3. Add XR Rig prefab from XR Interaction Toolkit
4. Add one grabbable object to the scene
5. Test in editor, then on Quest
6. Follow the development milestones in CLAUDE.md

## Resources

- [Unity 6.3 LTS Release Notes](https://docs.unity.com/upm/packages/com.unity.6@latest)
- [XR Interaction Toolkit Documentation](https://docs.unity.com/upm/packages/com.unity.xr.interaction.toolkit@latest)
- [Meta Quest Developer Documentation](https://developer.meta.com/docs/quest/)
- [OVR Metrics Tool Guide](https://developer.meta.com/documentation/quest/latest/concepts/pc-ovrmetricstool/)
- [OpenXR Quick Start](https://www.khronos.org/openxr/)
