# 🏝️ iOS Dynamic Island for Windows + Control Center Desktop App

A fluid, hardware-accelerated **iOS Dynamic Island** built natively for Windows laptops and desktops, complete with a modern **Control Center Desktop App** to calibrate and maintain it.

---

## ✨ Features

### 1. 🏝️ The Floating Dynamic Island
- **Fluid Morphing Pill**:
  - Pure OLED Pitch-Black capsule shape with subtle gloss and drop shadow.
  - Centered at the top of your laptop screen (sits directly underneath your webcam / bezel).
  - Truly non-intrusive: transparent overlay, clicks pass through outside the pill, and notifications never steal keyboard focus (`WS_EX_NOACTIVATE`).
- **Dynamic States**:
  - **Idle Mode**: Discreet, minimal pill (~175px × 34px). Shows current time, live battery percentage, or animated mini equalizer bars when music is playing.
  - **Alert Popups**: Dynamic temporary expansion (~280px × 40px) that smoothly morphs in for 3.5 seconds on events:
    - ⚡ **Charger Plugged / Unplugged**: Green lightning bolt + "Fast Charging 89%".
    - 🔋 **Low Battery Warning**: Yellow alert when below 20%.
    - 🔤 **Caps Lock Indicator**: Immediate on-screen pill popup when toggled ON/OFF.
    - 🔊 **Volume Indicator**: Shows exact volume percentage bar.
    - 🎵 **Song Change**: Shows new track title and artist.
  - **Expanded Interactive Card**:
    - Expands smoothly on mouse hover or left-click into an interactive dashboard (~420px × 190px).
    - **🎵 Now Playing Tab**:
      - Album art gradient badge.
      - Track title and artist (Spotify, YouTube in Chrome/Edge/Brave, VLC, etc.).
      - Live bouncing 6-bar equalizer audio visualizer.
      - Previous, Play/Pause, Next controls.
      - Volume slider.
    - **📊 System Stats Tab**:
      - Real-time CPU % gauge meter.
      - Real-time RAM % gauge meter (Used / Total GB).
      - Battery health & charging status.
      - Laptop system uptime.
    - **⏱️ Focus Timer & Pomodoro Tab**:
      - Big digital timer display.
      - Quick `+1m` and `+5m` buttons.
      - Start, Pause, Reset.
      - *Bonus:* When minimized, the compact island pill displays the live countdown!
    - **⚡ Quick Tools Tab**:
      - Instant Mute toggle.
      - Lock PC (`Win + L` equivalent).
      - Screenshot Snipping Tool.
      - Task Manager launcher.

---

### 2. ⚙️ Dynamic Island Control Center (Desktop Maintenance App)
A companion desktop app designed with Windows 11 Fluent Dark aesthetics to maintain and customize the island:
- **📐 Live Position Calibration**:
  - Top Offset slider (0px to 100px): updates the island's vertical position in real-time as you drag the slider so you can align it perfectly with your laptop screen bezel!
  - Idle width & height adjustments.
  - Expanded width & height adjustments.
- **🎨 Accent Color Presets**:
  - 🔵 iOS Blue (`#0A84FF`)
  - 🟢 Emerald Green (`#30D158`)
  - 🔷 Cyan Glow (`#64D2FF`)
  - 🟣 Cyber Purple (`#BF5AF2`)
  - 🟡 Sunset Gold (`#FF9F0A`)
  - ⚪ Pure Apple Silver (`#E5E5EA`)
- **⚡ Module Toggles**:
  - Enable/disable Media Player, Battery alerts, Caps Lock HUD, Volume HUD, System Stats, or Pomodoro.
- **🧪 Live Notification Simulator**:
  - One-click buttons to test animations immediately on screen:
    - Test Power Connected
    - Test Low Battery
    - Test Caps Lock Toggle
    - Test Volume Pop
    - Test Song Change
    - Test Focus Timer Complete
    - Send Custom Notification with custom title & message!
- **🚀 System & Startup**:
  - "Launch on Windows Startup" toggle (auto-registers shortcut in Windows Startup folder).
  - Reset to Defaults and Restart Island buttons.

---

### 3. 🛡️ System Tray Integration
- Lives quietly in the Windows notification area (bottom-right near clock).
- **Left-Click / Double-Click**: Opens the Control Center Desktop App.
- **Right-Click Menu**:
  - ⚙️ Dynamic Island Control Center
  - ✨ Test Notification Alert
  - 👁️ Toggle Island Visibility
  - 🔄 Restart Island
  - ❌ Exit Application

---

## 🚀 How to Run

### Direct Launch
Double click either executable in `bin\`:
1. **`bin\DynamicIsland.exe`**: Launches the floating Dynamic Island overlay.
2. **`bin\DynamicIslandConfig.exe`**: Launches the Desktop Control Center.

### Batch Launchers
- **`run.bat`**: Starts the Dynamic Island.
- **`run_control_center.bat`**: Starts the Control Center Desktop App.
- **`create_shortcuts.bat`**: Creates shortcuts on your Desktop.

### To Recompile (if you make changes)
Run `build.bat`. It compiles instantly using native Windows C# compiler (`csc.exe`) with zero dependencies required.
