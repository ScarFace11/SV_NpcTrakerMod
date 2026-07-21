# Stardew Valley NPC Tracker Mod

A C# SMAPI mod for Stardew Valley that visualizes NPC schedules, routes, and movement in real time as an in-game overlay.

## Project Overview

- **Language:** C# 7.3
- **Framework:** SMAPI (Stardew Valley Modding API) + MonoGame for rendering
- **Runtime:** .NET Framework
- **Testing:** xUnit unit tests

## Project Structure

```
NpcTrackerMod/              # Main mod project
├── ModEntry.cs             # SMAPI mod entry point
├── ModConfig.cs            # Player-configurable settings
├── ContentPatcher.cs       # Content Patcher integration
├── Core/
│   ├── ModState.cs         # Global mod state
│   └── NpcPathStore.cs     # Stores NPC path data
├── Rendering/
│   ├── TileRenderer.cs     # Draws tile overlays on the map
│   └── RouteRenderer.cs    # Draws NPC route paths
├── Scheduling/
│   ├── ScheduleProcessor.cs      # Processes raw schedule data
│   ├── ScheduleEntryParser.cs    # Parses individual schedule entries
│   ├── LocationMapper.cs         # Maps location names to tile coords
│   ├── JsonUtils.cs              # JSON helpers
│   └── CustomScheduleLoader.cs  # Loads custom schedules from files
├── Tracking/
│   ├── NpcTracker.cs       # Tracks NPC positions each game tick
│   └── NpcRegistry.cs      # Registry of tracked NPCs
└── UI/
    ├── TrackingMenu.cs     # In-game tracking menu
    └── MenuComponents.cs   # Reusable menu UI components

NpcTrackerMod.Tests/        # xUnit test project
├── JsonUtilsTests.cs
└── ScheduleEntryParserTests.cs
```

## How to Use This on Replit

This is a **game mod** — it runs inside Stardew Valley on your local machine, not as a standalone web app. Replit is used here for browsing and editing the source code.

To build and run the mod locally:
1. Install [SMAPI](https://smapi.io/) and Stardew Valley on your machine
2. Open `NpcTrackerMod.sln` in Visual Studio
3. Add references to the game DLLs (not included — they come from your game install)
4. Build and copy the output to your Stardew Valley `Mods/` folder

## User Preferences

_No preferences recorded yet._
