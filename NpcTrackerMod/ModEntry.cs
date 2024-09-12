using System;
using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;


namespace NpcTrackerMod
{
    public class ModEntry
    {
        private readonly NpcTrackerMod modInstance;
        private bool DayStarted = false;
        public ModEntry(NpcTrackerMod instance)
        {
            modInstance = instance;
        }

        // Обработка нажатий кнопок
        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu != null) return; // Проверка, что нет активного меню

            switch (e.Button)
            {
                case SButton.G:
                    OpenTrackingMenu();
                    break;
                case SButton.X:
                    ToggleShowAllRoutes();
                    break;
                case SButton.Z:
                    LogCurrentLocationWarps(); // Закомментировано
                    break;
            }
        }

        // Открытие меню отслеживания
        private void OpenTrackingMenu()
        {
            Game1.activeClickableMenu = new TrackingMenu();
        }

        // Переключение отображения всех маршрутов
        private void ToggleShowAllRoutes()
        {
            modInstance.showAllRoutes = !modInstance.showAllRoutes;
            modInstance.SwitchGetNpcPath = modInstance.showAllRoutes;
            modInstance.Monitor.Log($"Смена локаций: {modInstance.showAllRoutes}", LogLevel.Debug);
        }

        // Обработка события начала дня
        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            DayStarted = true;

            if (!modInstance.LocationSet)
                modInstance.LocationsList.SetLocations();

            UpdateNpcCount();
            ClearDataForNewDay();
        }

        // Проверка изменения количества NPC и обновление списков
        private void UpdateNpcCount()
        {
            int npcCount = Game1.characterData.Count();
            if (npcCount == modInstance.NpcCount) return;

            modInstance.NpcList.CreateTotalAndBlackList();
            modInstance.NpcCount = npcCount;
        }

        // Очистка данных перед началом нового дня
        private void ClearDataForNewDay()
        {
            modInstance.tileStates.Clear();
            modInstance.npcPreviousPositions.Clear();
            modInstance.npcTemporaryColors.Clear();
        }

        // Обработка события конца дня
        public void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            DayStarted = false;
        }

        // Проверка наличия определенного мода
        public bool IsModInstalled(string modId)
        {
            return modInstance.Helper.ModRegistry.IsLoaded(modId);
        }

        // Отрисовка объектов в мире
        public void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {           
            if (!modInstance.DisplayGrid) return;

            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);
                
                modInstance.DrawTiles.DrawGrid(spriteBatch, cameraOffset);
                modInstance.DrawNpcPaths(spriteBatch, cameraOffset);
            }
            catch (Exception ex)
            {
                modInstance.Monitor.Log($"Error in {nameof(OnRenderedWorld)}: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);
            }
        }

        // Все локации и их координаты телепортов
        private void LogCurrentLocationWarps()
        {
            AllGameLocation();

            modInstance.Monitor.Log($"{Game1.currentLocation.Name}", LogLevel.Info);
            var warpCoordinates = Game1.currentLocation.warps
                .Select(warp => $"({warp.X}, {warp.Y})")
                .ToList();
            modInstance.Monitor.Log(string.Join(", ", warpCoordinates), LogLevel.Info);

            foreach (var warp in Game1.currentLocation.warps)
            {
                modInstance.Monitor.Log($" warp: | X: {warp.X} | Y: {warp.Y} {warp.TargetName}", LogLevel.Debug);
            }
            foreach (var dor3 in Game1.currentLocation.doors.Pairs)
            {
                foreach (var dor0 in dor3.Value)
                {
                    modInstance.Monitor.Log($" doors: {dor0}", LogLevel.Debug);
                }
                modInstance.Monitor.Log($" doors: {dor3}", LogLevel.Debug);
            }
            foreach (var ch in Game1.characterData.Keys)
            {
                modInstance.Monitor.Log($"Keys: {ch}", LogLevel.Debug);
            }
        }

        // Лог для отображения всех локаций в консоли
        private void AllGameLocation()
        {
            foreach (var location in Game1.locations)
            {
                modInstance.Monitor.Log(location.NameOrUniqueName, LogLevel.Debug);
            }
        }

        // Обновление по тикам
        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            const int tickRate = 60;
            const int intervalInSeconds = 5;

            if (DayStarted && e.IsMultipleOf((uint)(tickRate * intervalInSeconds)))
            {
                modInstance.Monitor.Log("Апдейт", LogLevel.Debug);
            }
        }
    }
}
