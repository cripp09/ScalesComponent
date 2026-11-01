using ScalesComponent.Models;

namespace ScalesComponent.Parsers
{
    /// <summary>
    /// Интерфейс парсера данных весов.
    /// </summary>
    /// <remarks>
    /// <para><b>Важно:</b> Не все реализации этого интерфейса были физически протестированы на реальном оборудовании.</para>
    /// <para>
    /// Парсеры, протестированные автором на реальном железе через COM-порт (RS-232):
    /// <list type="bullet">
    ///   <item><description>CasParser (CAS CI-200A)</description></item>
    ///   <item><description>MassaKParser (Massa-K ТВ-М)</description></item>
    ///   <item><description>MetrisoftParser</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Остальные парсеры реализованы на основе официальной документации производителей 
    /// и требуют верификации на конкретном оборудовании перед использованием в 
    /// промышленной эксплуатации. См. файл TESTING_STATUS.md в корне репозитория 
    /// для актуального статуса тестирования.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Возвращает название бренда
        /// </summary>
        string GetBrandName();
    }
}