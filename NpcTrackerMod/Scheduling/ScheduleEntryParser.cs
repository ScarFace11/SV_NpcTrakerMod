using System.Text.RegularExpressions;

namespace NpcTrackerMod.Scheduling
{
    /// <summary>
    /// Парсер одной строки расписания NPC.
    /// Чистый статический класс — без зависимостей на игровые объекты.
    /// Вынесен отдельно для юнит-тестирования.
    /// </summary>
    public static class ScheduleEntryParser
    {
        private static readonly Regex _digitRx = new Regex(@"\d+", RegexOptions.Compiled);

        /// <summary>
        /// Разбирает строку-часть расписания (один слот, без /).
        /// </summary>
        /// <param name="parts">Строка, разбитая по пробелам.</param>
        /// <param name="lastLocationName">Локация конца предыдущего слота (может быть null).</param>
        /// <param name="time">Игровое время (строка, например "900").</param>
        /// <param name="locationName">Имя целевой локации.</param>
        /// <param name="x">X-координата цели.</param>
        /// <param name="y">Y-координата цели.</param>
        /// <param name="facingDirection">Направление взгляда (0-3).</param>
        /// <param name="endBehavior">Анимация в точке назначения (или null).</param>
        /// <param name="endMessage">Строка диалога (или null).</param>
        public static void Parse(
            string[] parts,
            string lastLocationName,
            out string time,
            out string locationName,
            out int x,
            out int y,
            out int facingDirection,
            out string endBehavior,
            out string endMessage)
        {
            time = "0";
            locationName = string.Empty;
            x = y = facingDirection = 0;
            endBehavior = null;
            endMessage = null;

            int i = 0;

            // Слот 1: число → время, иначе → локация
            if (i < parts.Length)
            {
                var m = _digitRx.Match(parts[i]);
                if (m.Success && m.Index == 0) // строго начало строки — это время
                {
                    time = m.Value;
                    i++;
                }
                else
                {
                    locationName = int.TryParse(parts[i], out _)
                        ? lastLocationName
                        : parts[i];
                    i++;
                }
            }

            // Слот 2: локация (если ещё не задана)
            if (i < parts.Length && string.IsNullOrEmpty(locationName))
            {
                if (int.TryParse(parts[i], out _))
                    locationName = lastLocationName;
                else
                {
                    locationName = parts[i];
                    i++;
                }
            }

            // Слот 3,4: X Y
            if (parts.Length > i + 1 &&
                int.TryParse(parts[i], out var px) &&
                int.TryParse(parts[i + 1], out var py))
            {
                x = px;
                y = py;
                i += 2;
            }

            // Слот 5: направление взгляда
            if (i < parts.Length && int.TryParse(parts[i], out var dir))
            {
                facingDirection = dir;
                i++;
            }
            else
            {
                facingDirection = 2; // south — дефолт
            }

            // Слот 6: анимация или сообщение
            if (i < parts.Length)
            {
                var val = parts[i];
                if (val.StartsWith("\"Strings\\"))
                    endMessage = val;
                else
                    endBehavior = val;
                i++;
            }

            // Слот 7: сообщение после анимации
            if (i < parts.Length && parts[i].StartsWith("\"Strings\\"))
                endMessage = parts[i];
        }

        /// <summary>
        /// Возвращает true, если запись расписания достаточно полна для обработки.
        /// </summary>
        public static bool IsValid(string key, string rawData)
            => !string.IsNullOrWhiteSpace(key) && rawData != null && rawData.Contains(" ");

        /// <summary>
        /// Возвращает true, если строка-слот расписания должна быть пропущена.
        /// </summary>
        public static bool ShouldSkip(string entry)
            => entry.Contains("MAIL") || entry.Contains("friendship") ||
               entry.Contains("GOTO") || entry.Contains("NO_SCHEDULE");
    }
}
