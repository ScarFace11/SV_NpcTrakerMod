using System.Text.RegularExpressions;

namespace NpcTrackerMod.Scheduling
{
    /// <summary>
    /// Утилиты для обработки JSON-расписаний.
    /// Чистый статический класс — без зависимостей на XNA/SMAPI.
    /// </summary>
    public static class JsonUtils
    {
        /// <summary>
        /// Удаляет однострочные (// ...) и многострочные (/* ... */) комментарии из JSON.
        /// </summary>
        public static string RemoveComments(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            json = Regex.Replace(json, @"//.*?(?=\r?$)", "",  RegexOptions.Multiline);
            json = Regex.Replace(json, @"/\*.*?\*/",      "", RegexOptions.Singleline);
            return json;
        }
    }
}
