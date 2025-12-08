using System;
using System.Linq;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Helpers
{
    /// <summary>
    /// Вспомогательные методы для кабинета работодателя
    /// </summary>
    public static class EmployerDashboardHelper
    {
        /// <summary>
        /// Получает статус отклика на русском языке
        /// </summary>
        public static string GetStatusDisplay(string status) => status switch
        {
            "pending" => "Ожидает",
            "viewed" => "Просмотрено",
            "interview" => "Собеседование",
            "hired" => "Принят",
            "rejected" => "Отказано",
            _ => status
        };

        /// <summary>
        /// Форматирует дату в короткую строку
        /// </summary>
        public static string FormatDate(DateTime date) => date.ToString("dd.MM.yyyy");
    }
}