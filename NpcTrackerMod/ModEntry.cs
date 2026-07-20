using System;
using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace NpcTrackerMod
{
    public class ModEntry
    {
        private readonly _modInstance modInstance;
        private bool DayStarted = false;

        /// <summary> Предыдущая локация игрока. </summary>
        private string previousLocationName;

        public ModEntry(_modInstance instance)
        {
            modInstance = instance;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопок.
        /// Клавиши читаются из config.json и могут быть изменены пользователем.
        /// </summary>
        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (Game1.activeClickableMenu != null || !Context.IsPlayerFree) return;

            if (e.Button == modInstance.Config.MenuKey)
                OpenTrackingMenu();
            else if (e.Button == modInstance.Config.DebugKey)
                LogCurrentLocationWarps();
        }

        private void LogCurrentLocationWarps()
        {
            modInstance.Monitor.Log($"{Game1.currentLocation.Name}", LogLevel.Info);
            foreach (var warp in Game1.currentLocation.warps)
                modInstance.Monitor.Log($" warp: | X: {warp.X}\t Y: {warp.Y}\t | {warp.TargetName}", LogLevel.Debug);
            foreach (var door in Game1.currentLocation.doors.Pairs)
                modInstance.Monitor.Log($" doors: {door}", LogLevel.Debug);
        }

        private void OpenTrackingMenu()
        {
            Game1.activeClickableMenu = new TrackingMenu(modInstance);
        }

        /// <summary>
        /// Обрабатывает событие начала дня.
        /// </summary>
        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            ClearDataForNewDay();
            DayStarted = true;

            if (!modInstance.LocationSet)
                modInstance.LocationsList.SetLocations();

            UpdateNpcCount();
        }

        private void UpdateNpcCount()
        {
            modInstance.NpcList.CreateTotalAndBlackList();
        }

        /// <summary>
        /// Очищает данные для нового дня.
        /// </summary>
        private void ClearDataForNewDay()
        {
            modInstance.DrawTiles.ClearTiles();
            modInstance.DrawTiles.npcTemporaryColors.Clear();
            modInstance.npcPreviousPositions.Clear();
            modInstance.SwitchGetNpcPath = false;

            modInstance.NpcList.BlacklistedNpcs.Clear();
            modInstance.NpcList.TotalNpcList.Clear();
            modInstance.NpcList.CurrentNpcList.Clear();
            modInstance.NpcList.NpcTotalToDayPath.Clear();
            modInstance.NpcList.NpcTimedDayPath.Clear();

            modInstance.NpcCount = 0;
        }

        public void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            DayStarted = false;
        }

        /// <summary>
        /// Отрисовывает сетку и маршруты NPC. После отрисовки тайлов
        /// показывает всплывающую подсказку при наведении на помеченный тайл.
        /// </summary>
        public void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!modInstance.EnableDisplay) return;

            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);

                modInstance.DrawNpcPaths(spriteBatch, cameraOffset);

                if (modInstance.DisplayGrid)
                    modInstance.DrawTiles.DrawGrid(spriteBatch, cameraOffset);

                // Tooltip: при наведении на тайл маршрута показываем имя NPC и метку времени
                int tileX = (int)((Game1.viewport.X + Game1.getMouseX()) / Game1.tileSize);
                int tileY = (int)((Game1.viewport.Y + Game1.getMouseY()) / Game1.tileSize);
                var hoveredTile = new Point(tileX, tileY);

                if (modInstance.DrawTiles.tileOwners.TryGetValue(hoveredTile, out var ownerList) && ownerList.Count > 0)
                {
                    var lines = new System.Text.StringBuilder();
                    foreach (var entry in ownerList)
                    {
                        if (lines.Length > 0) lines.Append('\n');
                        lines.Append(string.IsNullOrEmpty(entry.timeInfo)
                            ? entry.npcName
                            : $"{entry.npcName} ({entry.timeInfo})");
                    }
                    IClickableMenu.drawHoverText(spriteBatch, lines.ToString(), Game1.smallFont);
                }
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Error in {nameof(OnRenderedWorld)}: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Проверка игрока на переход между локациями.
        /// </summary>
        public void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (Game1.player.currentLocation.Name != previousLocationName)
            {
                modInstance.DrawTiles.ClearTiles();
                previousLocationName = Game1.player.currentLocation.Name;
                modInstance.SwitchGetNpcPath = true;
                modInstance.NpcCount = Game1.player.currentLocation.characters.Count();
            }
        }

        /// <summary>
        /// Обрабатывает обновление по тикам.
        /// </summary>
        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            const int tickRate = 60;
            const int intervalInSeconds = 1;

            if (DayStarted && e.IsMultipleOf((uint)(tickRate * intervalInSeconds)))
            {
                if (modInstance.EnableDisplay &&
                    !modInstance.SwitchGlobalNpcPath &&
                    !modInstance.SwitchTargetLocations &&
                    Game1.player.currentLocation.characters.Count() != modInstance.NpcCount)
                {
                    modInstance.DrawTiles.ClearTiles();
                    modInstance.SwitchGetNpcPath = true;
                    modInstance.NpcCount = Game1.player.currentLocation.characters.Count();
                    if (!modInstance.SwitchGlobalNpcPath)
                        modInstance.NpcList.RefreshCurrentNpcList();
                }
            }
        }
    }
}