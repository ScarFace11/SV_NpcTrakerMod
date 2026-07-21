<div align="center">

# 🌾 Stardew Valley NPC Tracker

### Advanced NPC Route Visualization Mod for Stardew Valley

![Platform](https://img.shields.io/badge/Game-Stardew%20Valley-8BC34A?style=for-the-badge)
![Language](https://img.shields.io/badge/C%23-7.3-239120?style=for-the-badge&logo=csharp)
![Framework](https://img.shields.io/badge/SMAPI-Latest-blue?style=for-the-badge)
![License](https://img.shields.io/badge/Status-Personal_Project-orange?style=for-the-badge)

*A developer tool for visualizing NPC schedules, routes and movement in real time.*

</div>

---

# 📖 About

**Stardew Valley NPC Tracker** is a C# mod for **Stardew Valley** built with **SMAPI**.

The mod visualizes NPC schedules and movement directly in-game, making it easier to inspect AI behavior, debug schedules, and understand how NPCs navigate between locations.

Unlike simple minimap trackers, this project parses schedule data, processes NPC routes, and renders movement overlays dynamically while the game is running.

---

# ✨ Features

✅ Real-time NPC tracking

✅ Visual tile overlay

✅ Route rendering

✅ Schedule parsing

✅ Location mapping

✅ Automatic schedule loading

✅ Modular architecture

✅ Unit tests for schedule parsing

---

# 🛠 Tech Stack

| Technology | Purpose |
|------------|---------|
| C# 7.3 | Main programming language |
| SMAPI | Stardew Valley modding API |
| MonoGame | Rendering |
| .NET Framework | Runtime |
| Visual Studio | Development |
| xUnit / Unit Tests | Testing |

---

# 📂 Project Structure

```
NpcTrackerMod
│
├── Core
│   ├── ModState
│   └── NpcPathStore
│
├── Rendering
│   ├── TileRenderer
│   └── RouteRenderer
│
├── Scheduling
│   ├── ScheduleProcessor
│   ├── ScheduleEntryParser
│   ├── LocationMapper
│   ├── JsonUtils
│   └── CustomScheduleLoader
│
├── Tracking
│   ├── NpcTracker
│   └── NpcRegistry
│
├── UI
│   ├── TrackingMenu
│   └── MenuComponents
│
├── ModEntry.cs
├── ModConfig.cs
└── ContentPatcher.cs
```

---

# ⚙ Architecture

The project follows a modular architecture where every subsystem has a dedicated responsibility.

```
               ModEntry
                   │
      ┌────────────┼────────────┐
      │            │            │
 Rendering     Scheduling    Tracking
      │            │            │
      └────────────┼────────────┘
                   │
               ModState
```

---

# 🚀 Installation

### Requirements

- Stardew Valley
- SMAPI
- .NET Framework

### Steps

1. Install SMAPI.
2. Download the latest release.
3. Copy the mod folder into:

```
Stardew Valley/
└── Mods/
```

4. Launch the game using SMAPI.

---

# 🎮 Functionality

The tracker performs several tasks during gameplay:

- Loads NPC schedules
- Parses route definitions
- Maps locations
- Tracks current NPC positions
- Draws movement paths
- Renders tile overlays
- Updates routes in real time

---

# 🧪 Testing

The solution includes a dedicated testing project.

Current tests cover:

- JSON utilities
- Schedule parsing
- Schedule entry validation

---

# 💡 Design Principles

- Separation of responsibilities
- Modular services
- Clean rendering pipeline
- Reusable schedule processing
- Easy extensibility
- Maintainable architecture

---

# 📈 Possible Future Improvements

- Minimap integration
- Performance optimizations
- Custom overlay colors
- Interactive debugging tools

---

# 📊 Repository Stats

```
Language:          C#
Architecture:      Modular
Game API:          SMAPI
Rendering:         MonoGame
Testing:           Unit Tests
Project Type:      Game Development / Tooling
```

---

# 🤝 Contributing

Suggestions and improvements are welcome.

Feel free to open an Issue or submit a Pull Request.

---

<div align="center">

### ⭐ If you found this project interesting, consider giving it a star!

Made with ❤️ using C#, SMAPI and Stardew Valley

</div>
