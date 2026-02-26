using System;

namespace WebReckrytingSystem.Helpers
{
    /// <summary>
    /// Вспомогательные методы для представлений
    /// </summary>
    public static class ViewHelper
    {
        /// <summary>
        /// Обрезает строку до указанной длины с добавлением троеточия
        /// </summary>
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Преобразует тип занятости в русский текст
        /// </summary>
        public static string GetEmploymentTypeDisplay(string employmentType) => employmentType switch
        {
            "full" => "Полная занятость",
            "part" => "Частичная",
            "project" => "Проектная",
            "internship" => "Стажировка",
            "volunteer" => "Волонтёрство",
            _ => employmentType
        };

        /// <summary>
        /// Преобразует график работы в русский текст
        /// </summary>
        public static string GetWorkScheduleDisplay(string workSchedule) => workSchedule switch
        {
            "full_day" => "Полный день",
            "shifts" => "Сменный",
            "flexible" => "Гибкий",
            "remote" => "Удалёнка",
            "shift_work" => "Вахта",
            _ => workSchedule
        };
    }
}