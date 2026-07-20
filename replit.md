# NpcTrackerMod

A **Stardew Valley SMAPI mod** written in C# that tracks NPC schedules and renders their paths as tile overlays in-game.

## Project Overview

- **Stack**: C# (.NET Framework 4.8), SMAPI mod framework
- **Solution**: `NpcTrackerMod.sln`
- **Main mod**: `NpcTrackerMod/NpcTrackerMod/`
- **Tests**: `NpcTrackerMod.Tests/`

## Architecture

The mod is service-oriented with `ModState` as a central data hub:

| Folder | Purpose |
|---|---|
| `ModEntry.cs` | SMAPI entry point — hooks events, wires up services |
| `ModConfig.cs` | User settings persisted to `config.json` |
| `Core/` | State (`ModState`) and path data store (`NpcPathStore`) |
| `Scheduling/` | Parses NPC schedules and computes tile-level paths via pathfinding |
| `Tracking/` | Registry of trackable NPCs; coordinates per-frame tracking |
| `Rendering/` | Draws path tiles and route overlays onto the game world |
| `UI/` | In-game menu (SMAPI `IClickableMenu`), buttons, and checkboxes |

## How it Works

1. On `DayStarted`, `ScheduleProcessor` reads each NPC's schedule and pathfinds between entries, storing `HashSet<Point>` per location in `NpcPathStore`.
2. On `RenderedWorld`, `RouteRenderer` fetches stored paths and submits them to `TileRenderer` for drawing via `SpriteBatch`.
3. The `TrackingMenu` lets players toggle NPCs, filter by time, and adjust settings.

## Building

This project requires:
- **.NET Framework 4.8** SDK (Mono on Linux)
- **Stardew Valley** game files (referenced by `Pathoschild.Stardew.ModBuildConfig`)
- **SMAPI** installed alongside the game

To compile locally (with game files present):
```
msbuild NpcTrackerMod.sln /p:Configuration=Release
```

The output `NpcTrackerMod.dll` + `manifest.json` go in your game's `Mods/NpcTrackerMod/` folder.

## Dependencies (NuGet)

- `Newtonsoft.Json 13.0.4`
- `Pathoschild.Stardew.ModBuildConfig 4.4.0` (requires game path)
- `System.ValueTuple 4.6.2`

## User Preferences

_No preferences recorded yet._
