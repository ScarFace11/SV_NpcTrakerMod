# Stardew Valley NPC Tracker Mod

A C# SMAPI mod for Stardew Valley that visualizes NPC schedules, routes, and real-time movement as tile overlays in-game.

## Stack

- **Language**: C# 7.3 / .NET Framework
- **Mod API**: SMAPI (Stardew Modding API)
- **Rendering**: MonoGame / XNA (`SpriteBatch`)
- **Parsing**: Newtonsoft.Json 13.0.4
- **Build**: Pathoschild.Stardew.ModBuildConfig 4.4.0
- **Tests**: xUnit (`NpcTrackerMod.Tests`)

## Project Structure

```
NpcTrackerMod/
  NpcTrackerMod/
    Core/           — ModState (centralized state), NpcPathStore (path data)
    Scheduling/     — ScheduleProcessor, ScheduleEntryParser, LocationMapper,
                      CustomScheduleLoader (Content Patcher support), JsonUtils
    Tracking/       — NpcTracker (frame orchestrator), NpcRegistry (NPC lists)
    Rendering/      — TileRenderer (primitive drawing), RouteRenderer (path logic)
    UI/             — TrackingMenu (SMAPI tabbed menu), MenuComponents
    ModEntry.cs     — Entry point, lifecycle & event management
    ModConfig.cs    — Persisted hotkey config (Menu: G, Debug: Z, Select: MiddleClick)
    manifest.json   — SMAPI mod manifest
  NpcTrackerMod.Tests/
    ScheduleEntryParserTests.cs
    JsonUtilsTests.cs
NpcTrackerMod.sln
```

## How to Run

This mod **cannot run on Replit** — SMAPI mods require Stardew Valley to be installed locally.

To deploy:
1. Build the project locally (or compile here with `dotnet build`)
2. Copy the output `NpcTrackerMod.dll` + `manifest.json` to your game's `Mods/NpcTrackerMod/` folder
3. Launch the game via SMAPI

## Running Tests on Replit

```bash
cd NpcTrackerMod.Tests
dotnet test
```

## Key Dependencies (NuGet)

- `Newtonsoft.Json` 13.0.4
- `Pathoschild.Stardew.ModBuildConfig` 4.4.0 (build-time only)
- SMAPI and MonoGame provided by the build config

## User Preferences

- Keep existing project structure and architecture (service-based, centralized ModState)
- Preserve C# 7.3 compatibility
