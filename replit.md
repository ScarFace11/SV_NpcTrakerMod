# Stardew Valley NPC Tracker Mod

A C# SMAPI mod for Stardew Valley that visualizes NPC schedules, routes, and movement in real time as an in-game overlay.

## Project Overview

- **Language:** C# (net6.0)
- **Framework:** SMAPI (Stardew Valley Modding API) + MonoGame for rendering
- **Testing:** xUnit unit tests

## Active Branch

Working on **Test** — this is the up-to-date branch. `master` contains older code.

## Project Structure

```
NpcTrackerMod/              # Main mod project
├── ModEntry.cs             # SMAPI entry point + event handling
├── ModConfig.cs            # Player-configurable keybindings (config.json)
├── ContentPatcher.cs       # Content Patcher integration
├── Core/
│   ├── ModState.cs         # Global runtime state
│   └── NpcPathStore.cs     # NPC path data (day / timed / global)
├── Rendering/
│   ├── TileRenderer.cs     # Tile overlay + grid drawing
│   └── RouteRenderer.cs    # NPC route & position tile logic
├── Scheduling/
│   ├── ScheduleProcessor.cs      # Builds routes from schedule strings
│   ├── ScheduleEntryParser.cs    # Parses individual schedule entries
│   ├── LocationMapper.cs         # Warp/door → tile coordinate map
│   ├── JsonUtils.cs              # JSON helpers
│   └── CustomScheduleLoader.cs  # Loads mod schedules via Content API
├── Tracking/
│   ├── NpcTracker.cs       # Per-frame render orchestration
│   └── NpcRegistry.cs      # NPC lists, SelectedNpcNames (multi-select)
└── UI/
    ├── TrackingMenu.cs     # Tabbed in-game menu (Main / NPC / Settings / Info)
    └── MenuComponents.cs   # ClickableCheckbox widget

NpcTrackerMod.Tests/        # xUnit test project
├── JsonUtilsTests.cs
└── ScheduleEntryParserTests.cs
```

## Implemented Features

### Core tracker
- Real-time NPC route overlay (green tiles = path, blue tile = current position)
- Day / global / time-filtered route modes
- Show all locations or current location only
- NPC search + mod-source filter in menu
- Configurable keybindings via Settings tab

### Фича 3 — Клик на тайл маршрута для выбора NPC
Нажмите **колёсико мыши** (Middle Mouse, настраивается) на любой тайл маршрута —
NPC добавляется в выборку. Повторный клик снимает выбор. Работает с мульти-выбором
через `SelectedNpcNames`. Клавиша перепривязывается во вкладке Настройки.

### Фича 4 — Следующая точка расписания в тултипе
При наведении курсора на тайл маршрута тултип показывает:
- Имя NPC и временную метку посещения
- Следующую точку расписания: `→ Saloon в 12:00`
- Подсказку о клавише выбора

## Keybindings (config.json)

| Клавиша | Действие | По умолчанию |
|---------|----------|--------------|
| MenuKey | Открыть меню трекера | G |
| DebugKey | Вывести варпы локации в лог | Z |
| SelectNpcKey | Выбрать/снять NPC на тайле | Middle Mouse |

## How to Build

This mod requires Stardew Valley + SMAPI installed locally:
1. Open `NpcTrackerMod.sln` in Visual Studio
2. Game DLLs are resolved automatically via `Pathoschild.Stardew.ModBuildConfig`
3. Build → copy output to `Stardew Valley/Mods/NpcTrackerMod/`

## User Preferences

_No preferences recorded yet._
