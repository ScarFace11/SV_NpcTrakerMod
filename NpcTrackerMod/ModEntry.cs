using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace NpcTrackerMod
{
    public class ModEntry
    {
        private NpcTrackerMod modInstance;

        private bool DayStarted = false;
        public ModEntry(NpcTrackerMod instance)
        {
            this.modInstance = instance;
        }
        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.G && Game1.activeClickableMenu == null) // Вызов меню
            {
                Game1.activeClickableMenu = new TrackingMenu();
            }
            if (e.Button == SButton.Z)
            {
                LogCurrentLocationWarps();
                //AllGameLoaction();
            }
            if (e.Button == SButton.X) // Допустим, X - это ваша кнопка для отображения всех маршрутов
            {
                modInstance.showAllRoutes = !modInstance.showAllRoutes;
                if (modInstance.showAllRoutes)
                {
                    modInstance.SwitchGetNpcPath = true;
                }
                //Console.Clear();
                modInstance.Monitor.Log($"Смена лоакций {modInstance.showAllRoutes}", LogLevel.Debug);
            }
        }
        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            DayStarted = true;

            var NpcCount = Game1.characterData.Count();
            if (NpcCount != modInstance.NpcCount)
            {
                modInstance.TotalNpcList.CreateTotalAndBlackList();
                modInstance.NpcCount = NpcCount;
            }

            modInstance.LocationsList.Locations();

            modInstance.tileStates.Clear();
            modInstance.npcPreviousPositions.Clear();
            modInstance.npcTemporaryColors.Clear();
        }
        public void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            DayStarted = false;
        }
        // Проверка наличия определенного мода по его ID
        public bool IsModInstalled(string modId)
        {
            return this.modInstance.Helper.ModRegistry.IsLoaded(modId);
        }
        public void OnRenderedWorld(object sender, RenderedWorldEventArgs e) // отрисовка в мире
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
        private void LogCurrentLocationWarps()
        {
            /*
            modInstance.Monitor.Log($"{Game1.currentLocation.Name}", LogLevel.Info);
            var warpCoordinates = Game1.currentLocation.warps
                .Select(warp => $"({warp.X}, {warp.Y})")
                .ToList();
            modInstance.Monitor.Log(string.Join(", ", warpCoordinates), LogLevel.Info);
            */



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

            foreach (var loc in Game1.locations)
            {
                //modInstance.Monitor.Log($"LocName: {loc.Name}", LogLevel.Debug);
            }


        }
        private void AllGameLoaction()
        {
            foreach (var location in Game1.locations.ToList())
            {
                modInstance.Monitor.Log($"{location.NameOrUniqueName}", LogLevel.Debug);
            }
        }
        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            int Tick = 60;
            int seconds = 5;
            //modInstance.Helper.Events.GameLoop.TimeChanged timeChanged
            
            if (DayStarted && e.IsMultipleOf((uint)(Tick * seconds)))
            {
                modInstance.Monitor.Log("Апдейт", LogLevel.Debug);
            }
            
        }

        /*
        // Для меню
        public static ModEntry Instance;
        public bool DisplayGrid { get; set; }
        public bool SwitchTargetLocations { get; set; } // true - Все локации / false - Локация с игроком     
        public bool SwitchTargetNPC { get; set; } // true - выбор отедльного нпс / false - всех нпс
        public bool SwitchGetNpcPath { get; set; } = true; //
        public bool SwitchListFull { get; set; } = false;


        public Dictionary<Point, (Color originalColor, Color currentColor, int priority)> tileStates = new Dictionary<Point, (Color, Color, int)>();
        public Dictionary<NPC, Point> npcPreviousPositions = new Dictionary<NPC, Point>();
        public Dictionary<Point, Color> npcTemporaryColors = new Dictionary<Point, Color>();

        public bool showAllRoutes { get; set; } = false;
        // Список путей и локаций



        public bool Switchnpcpath = false;

        public int NpcSelected { get; set; }

        public NpcList TotalNpcList;
        // было приватным
        public string previousLocationName; // локация где находился игрок
        public Texture2D lineTexture;
        public int tileSize;
        public Draw_Tiles DrawTiles;
        public NpcManager Npc_Manager;
        public NpcTrackerMod NpcTracker;

        public List<List<(string, List<Point>)>> path = new List<List<(string, List<Point>)>>();
        public ModEntry()
        {
            TotalNpcList = new NpcList();
        }
        public override void Entry(IModHelper helper)
        {
            tileSize = Game1.tileSize;
            lineTexture = CreateLineTexture(Game1.graphics.GraphicsDevice);

            Instance = this; // Инициализация экземпляра
            DrawTiles = new Draw_Tiles(Instance, tileSize, lineTexture);
            Npc_Manager = new NpcManager(Instance);

            // Подписка на события
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            // Пример использования функции для проверки мода
            if (IsModInstalled("FlashShifter.StardewValleyExpandedCP"))
            {
                Monitor.Log("Мод установлен!", LogLevel.Info);
                // Действия, если мод установлен
            }
            else
            {
                Monitor.Log("Нет мода.", LogLevel.Info);
                // Действия, если мод не установлен
            }


        }
        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.G && Game1.activeClickableMenu == null) // Вызов меню
            {
                Game1.activeClickableMenu = new TrackingMenu(ModEntry.Instance);
            }
            if (e.Button == SButton.Z)
            {
                //LogCurrentLocationWarps();
                //AllGameLoaction();
            }
            if (e.Button == SButton.X) // Допустим, X - это ваша кнопка для отображения всех маршрутов
            {
                showAllRoutes = !showAllRoutes;
                if (showAllRoutes)
                {
                    SwitchGetNpcPath = true;
                }
                //Console.Clear();
                Monitor.Log($"Смена лоакций {showAllRoutes}", LogLevel.Debug);
            }
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            TotalNpcList.CreateTotalAndBlackList();
            tileStates.Clear();
            npcPreviousPositions.Clear();
            npcTemporaryColors.Clear();
        }
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e) // отрисовка в мире
        {

            if (!DisplayGrid) return;

            try
            {
                var spriteBatch = e.SpriteBatch;
                Vector2 cameraOffset = new Vector2(Game1.viewport.X, Game1.viewport.Y);

                DrawTiles.DrawGrid(spriteBatch, cameraOffset);
                NpcTracker.DrawNpcPaths(spriteBatch, cameraOffset);

            }
            catch (Exception ex)
            {
                Monitor.Log($"Error in {nameof(OnRenderedWorld)}: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);
            }
        }
        private bool IsModInstalled(string modId)
        {
            return this.Helper.ModRegistry.IsLoaded(modId);
        }
        private static Texture2D CreateLineTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }
        */

    }
}
