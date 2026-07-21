using StardewModdingAPI;

namespace NpcTrackerMod
{
    /// <summary>
    /// Конфигурация мода, сохраняемая в config.json.
    /// </summary>
    public class ModConfig
    {
        /// <summary> Клавиша открытия меню трекера. </summary>
        public SButton MenuKey { get; set; } = SButton.G;

        /// <summary> Клавиша отладочного вывода варпов. </summary>
        public SButton DebugKey { get; set; } = SButton.Z;

        /// <summary> Клавиша выбора/снятия NPC при наведении на тайл маршрута. </summary>
        public SButton SelectNpcKey { get; set; } = SButton.MouseMiddle;
    }
}
