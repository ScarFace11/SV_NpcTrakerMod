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

        /// <summary>
        /// Конструктор класса ModEntry.
        /// </summary>
        /// /// <param name="instance">Экземпляр NpcTrackerMod.</param>
        public ModEntry(NpcTrackerMod instance)
        {
            modInstance = instance;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопок.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие нажатия кнопки.</param>
        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu != null) return; // Проверка, что нет активного меню

            switch (e.Button)
            {
                case SButton.G:
                    OpenTrackingMenu();
                    break;
                case SButton.Z:
                    LogCurrentLocationWarps();
                    break;
            }
        }

        /// <summary>
        /// Открывает меню отслеживания NPC.
        /// </summary>
        private void OpenTrackingMenu()
        {
            Game1.activeClickableMenu = new TrackingMenu();
        }

        /// <summary>
        /// Обрабатывает событие начала дня.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие начала дня.</param>
        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            DayStarted = true;

            if (!modInstance.LocationSet)
                modInstance.LocationsList.SetLocations();

            UpdateNpcCount();
            ClearDataForNewDay();
        }

        /// <summary>
        /// Обновляет количество NPC и пересоздает списки, если их количество изменилось.
        /// </summary>
        private void UpdateNpcCount()
        {
            int npcCount = Game1.characterData.Count();
            if (npcCount == modInstance.NpcCount) return;

            modInstance.NpcList.CreateTotalAndBlackList();
            modInstance.NpcCount = npcCount;
        }

        /// <summary>
        /// Очищает данные для нового дня.
        /// </summary>
        private void ClearDataForNewDay()
        {
            modInstance.tileStates.Clear();
            modInstance.npcPreviousPositions.Clear();
            modInstance.npcTemporaryColors.Clear();
        }

        /// <summary>
        /// Обрабатывает событие конца дня.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие конца дня.</param>
        public void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            DayStarted = false;
        }

        /// <summary>
        /// Проверяет, установлен ли определенный мод по его ID.
        /// </summary>
        /// <param name="modId">ID мода.</param>
        /// <returns>Возвращает true, если мод установлен, иначе false.</returns>
        public bool IsModInstalled(string modId)
        {
            return modInstance.Helper.ModRegistry.IsLoaded(modId);
        }

        /// <summary>
        /// Отрисовывает сетку и маршруты NPC в мире игры.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие отрисовки мира.</param>
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

        /// <summary>
        /// Логирует текущие локации и их координаты телепортов.
        /// </summary>
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

        /// <summary>
        /// Логирует все локации в консоли.
        /// </summary>
        private void AllGameLocation()
        {
            foreach (var location in Game1.locations)
            {
                modInstance.Monitor.Log(location.NameOrUniqueName, LogLevel.Debug);
            }
        }

        /// <summary>
        /// Обрабатывает обновление по тикам.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие обновления тиков.</param>
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
