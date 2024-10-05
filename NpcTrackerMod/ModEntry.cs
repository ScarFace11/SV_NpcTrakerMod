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
            modInstance.tileStates.Clear();
            modInstance.npcPreviousPositions.Clear();
            modInstance.npcTemporaryColors.Clear();
            modInstance.SwitchGetNpcPath = false;

            modInstance.NpcList.NpcBlackList.Clear();
            modInstance.NpcList.NpcTotalList.Clear();
            modInstance.NpcList.NpcCurrentList.Clear();
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
                //modInstance.Monitor.Log("Апдейт", LogLevel.Debug);
                if (modInstance.DisplayGrid && (!modInstance.SwitchGlobalNpcPath && !modInstance.SwitchTargetLocations) && (Game1.player.currentLocation.characters.Count() != modInstance.NpcCount))
                {                  
                    modInstance.tileStates.Clear();
                    modInstance.SwitchGetNpcPath = true;
                    modInstance.NpcCount = Game1.player.currentLocation.characters.Count();
                }
            }
        }
    }
}
