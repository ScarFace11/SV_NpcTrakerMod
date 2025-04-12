using System;
using System.Linq;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
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

        /// <summary>
        /// Конструктор класса ModEntry.
        /// </summary>
        /// /// <param name="instance">Экземпляр NpcTrackerMod.</param>
        public ModEntry(_modInstance instance)
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
            if (!Context.IsWorldReady) return;
            if (Game1.activeClickableMenu != null || (!Context.IsPlayerFree)) return; // Проверка, что нет активного меню
            
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

        private void LogCurrentLocationWarps()
        {
            //AllGameLocation();

            modInstance.Monitor.Log($"{Game1.currentLocation.Name}", LogLevel.Info);
            var warpCoordinates = Game1.currentLocation.warps
                .Select(warp => $"({warp.X}, {warp.Y})")
                .ToList();
            //modInstance.Monitor.Log(string.Join(", ", warpCoordinates), LogLevel.Info);

            foreach (var warp in Game1.currentLocation.warps)
            {
                modInstance.Monitor.Log($" warp: | X: {warp.X}\t Y: {warp.Y}\t | {warp.TargetName}", LogLevel.Debug);
            }
            foreach (var dor3 in Game1.currentLocation.doors.Pairs)
            {
                modInstance.Monitor.Log($" doors: {dor3}", LogLevel.Debug);
            }
            //foreach (var ch in Game1.characterData.Keys)
            //{
            //    modInstance.Monitor.Log($"character Keys: {ch}", LogLevel.Debug);
            //}
        }
        private void AllGameLocation()
        {
            foreach (var location in Game1.locations)
            {
                modInstance.Monitor.Log(location.NameOrUniqueName, LogLevel.Debug);
            }
        }
        /// <summary>
        /// Открывает меню отслеживания NPC.
        /// </summary>
        private void OpenTrackingMenu()
        {
            Game1.activeClickableMenu = new TrackingMenu(modInstance);
        }

        /// <summary>
        /// Обрабатывает событие начала дня.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие начала дня.</param>
        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;


            ClearDataForNewDay();

            DayStarted = true;
            
            if (!modInstance.LocationSet)
                modInstance.LocationsList.SetLocations();

            

            UpdateNpcCount();
            
        }

        /// <summary>
        /// Обновляет количество NPC и пересоздает списки, если их количество изменилось.
        /// </summary>
        private void UpdateNpcCount()
        {
            modInstance.NpcList.CreateTotalAndBlackList();
        }

        /// <summary>
        /// Очищает данные для нового дня.
        /// </summary>
        private void ClearDataForNewDay()
        {
            modInstance.DrawTiles.tileStates.Clear();
            modInstance.npcPreviousPositions.Clear();
            modInstance.DrawTiles.npcTemporaryColors.Clear();
            modInstance.SwitchGetNpcPath = false;

            modInstance.NpcList.BlacklistedNpcs .Clear();
            modInstance.NpcList.TotalNpcList.Clear();
            modInstance.NpcList.CurrentNpcList.Clear();
            modInstance.NpcList.NpcTotalToDayPath.Clear();

            modInstance.NpcCount = 0;

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
        /// Отрисовывает сетку и маршруты NPC в мире игры.
        /// </summary>
        /// <param name="sender">Объект, отправивший событие.</param>
        /// <param name="e">Событие отрисовки мира.</param>
        public void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {           
            if (!modInstance.EnableDisplay) return;
            
            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);

                modInstance.DrawNpcPaths(spriteBatch, cameraOffset);

                if (modInstance.DisplayGrid) modInstance.DrawTiles.DrawGrid(spriteBatch, cameraOffset);               
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
                modInstance.DrawTiles.tileStates.Clear();
                previousLocationName = Game1.player.currentLocation.Name;
                modInstance.SwitchGetNpcPath = true;
                modInstance.NpcCount = Game1.player.currentLocation.characters.Count();

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
            const int intervalInSeconds = 1;

            if (DayStarted && e.IsMultipleOf((uint)(tickRate * intervalInSeconds)))
            {
                //foreach(var i in modInstance.NpcList.NpcCurrentList)
                //{
                //    modInstance.Monitor.Log($"{i}", LogLevel.Debug);
                //}

                /*
                1) Мод включён
                2) отображения всех возможных путей и всех нпс отключено
                3) кол-во нпс в текущей локации не совпадает с кол-вом нпс с прошлого расчета
                4) ? возможно заменить 3-ье условия на варпнулся ли игрок
                */
                if (modInstance.EnableDisplay && (!modInstance.SwitchGlobalNpcPath && !modInstance.SwitchTargetLocations) && (Game1.player.currentLocation.characters.Count() != modInstance.NpcCount))
                {
                    modInstance.DrawTiles.tileStates.Clear();
                    modInstance.SwitchGetNpcPath = true;
                    modInstance.NpcCount = Game1.player.currentLocation.characters.Count();
                    if(!modInstance.SwitchGlobalNpcPath) modInstance.NpcList.RefreshCurrentNpcList();
                }
            }
        }
    }
}
