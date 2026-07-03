<div align="center">

# NPC Tracker Mod

A SMAPI mod for Stardew Valley that visualizes NPC movement in real time.

![Platform](https://img.shields.io/badge/Stardew%20Valley-1.6-green)
![Framework](https://img.shields.io/badge/SMAPI-4.x-blue)
![Language](https://img.shields.io/badge/C%23-7.3-purple)

</div>

---

## Overview

NPC Tracker Mod is a developer-oriented tool for Stardew Valley that allows you to monitor NPC movement in real time.

The mod visualizes NPC paths, destinations, and current positions directly in-game, making it useful for:

- debugging NPC schedules;
- studying Stardew Valley pathfinding;
- developing NPC-related mods;
- analyzing movement behaviour.

---

## Features

- ✅ Display NPC positions
- ✅ Visualize movement paths
- ✅ Show destination tiles
- ✅ Toggle tracking using an in-game menu
- ✅ Support for tracking all NPCs or selected NPCs
- ✅ Location switching support
- ✅ Grid rendering for easier path visualization

---

## Screenshots

> Screenshots will be added later.

---

## Installation

1. Install SMAPI.
2. Download the latest release.
3. Extract the `NpcTrackerMod` folder into:

```
Stardew Valley/Mods/
```

4. Launch the game using SMAPI.

---

## Controls

| Action | Description |
|---------|-------------|
| Open menu | Configure tracker options |
| Toggle grid | Show or hide tile grid |
| Toggle NPC tracking | Enable or disable tracking |
| Change location | Switch displayed location |

---

## Project Structure

```
NpcTrackerMod/
│
├── ModEntry.cs           # Mod entry point
├── NpcTracker.cs         # Main tracking logic
├── NpcManager.cs         # NPC management
├── Draw_Tiles.cs         # Tile rendering
├── Tracking_Menu.cs      # User interface
├── CustomNpcPaths.cs     # Custom path handling
├── LocationsList.cs      # Location management
├── ContentPatcher.cs     # Content Patcher integration
└── manifest.json
```

---

## Technologies

- C#
- .NET Framework
- SMAPI
- Stardew Valley Modding API
- Harmony
- Content Patcher API

---

## Development Goals

Planned improvements:

- Better path prediction
- Performance optimizations
- Multiple visualization modes
- Configurable colors
- Search and filter for NPCs
- Export path information

---

## Building

Clone the repository:

```bash
git clone <repository>
```

Build using Visual Studio or MSBuild.

---

## Contributing

Suggestions, bug reports and pull requests are welcome.

---

## License

This project is licensed under the MIT License.

---

## Acknowledgements

- ConcernedApe
- SMAPI
- Stardew Valley Modding Community
