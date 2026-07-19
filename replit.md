# NpcTrackerMod

A **Stardew Valley SMAPI mod** (C# / .NET 4.8) that tracks NPC paths and overlays them visually in-game.

## Project layout

```
NpcTrackerMod/NpcTrackerMod/NpcTrackerMod/   ← all source files
  ModEntry.cs          event handlers (button presses, day events)
  NpcTracker.cs        _modInstance — the SMAPI Mod entry point
  NpcManager.cs        NPC data management
  NpcList.cs           list of tracked NPCs
  LocationsList.cs     known game locations
  CustomNpcPaths.cs    custom / modded NPC path support
  Draw_Tiles.cs        rendering overlay tiles on the map
  Tracking_Menu.cs     in-game menu UI
  ContentPatcher.cs    Content Patcher integration
  manifest.json        SMAPI mod manifest (name, version, dependencies)
NpcTrackerMod.sln      Visual Studio solution file
```

## How to build

This is a **SMAPI mod** — it cannot run on Replit itself. To compile and test it:

1. Open `NpcTrackerMod/NpcTrackerMod/NpcTrackerMod.sln` in Visual Studio (Windows) or Rider.
2. Make sure Stardew Valley is installed (the build config auto-detects it via `Pathoschild.Stardew.ModBuildConfig`).
3. Build → the DLL is output to `bin/Debug/` and deployed to your `Mods/` folder automatically.

Alternatively, compile from the command line with `msbuild` on a machine that has .NET Framework 4.8 and the game installed.

## Dependencies

- [SMAPI](https://smapi.io/) ≥ 3.0.0
- [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) (optional)
- NuGet packages restored via `packages.config` (Newtonsoft.Json, Pathoschild.Stardew.ModBuildConfig, System.ValueTuple)

## User preferences

_(none recorded yet)_
