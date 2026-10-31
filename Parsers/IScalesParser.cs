using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Интерфейс парсера данных весов
    /// </summary>
    public interface IScalesParser
    {
        /// <summary>
        /// Парсит строку данных от весов
        /// </summary>
        /// <param name="line">Строка данных</param>
        /// <returns>Объект чтения или null, если строка не распознана</returns>
        ScalesReading Parse(string line);

        /// <summary>
        /// Возвращает паттерн для автоопределения типа весов
        /// </summary>
        string GetDetectionPattern();
        string GetBrandName();
    }
}