using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmployeeDirectory
{
    /// <summary>
    /// Менеджер для работы с файлами системы управления персоналом
    /// </summary>
    public static class FileManager
    {
        private static readonly string DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private static readonly string ReportsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
        private static readonly string LogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        /// <summary>
        /// Инициализация директорий
        /// </summary>
        static FileManager()
        {
            EnsureDirectoryExists(DataDirectory);
            EnsureDirectoryExists(ReportsDirectory);
            EnsureDirectoryExists(LogsDirectory);
        }

        /// <summary>
        /// Создание директории если она не существует
        /// </summary>
        /// <param name="directoryPath">Путь к директории</param>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Сохранение списка сотрудников в JSON файл
        /// </summary>
        /// <param name="employees">Список сотрудников</param>
        /// <param name="fileName">Имя файла (опционально)</param>
        /// <returns>Путь к сохраненному файлу</returns>
        public static async Task<string> SaveEmployeesToJsonAsync(List<Employee> employees, string? fileName = null)
        {
            fileName ??= $"employees_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
            var filePath = Path.Combine(DataDirectory, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(employees, options);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

            await LogOperationAsync($"Сохранено {employees.Count} сотрудников в файл: {fileName}");
            return filePath;
        }

        /// <summary>
        /// Загрузка списка сотрудников из JSON файла
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns>Список сотрудников</returns>
        public static async Task<List<Employee>> LoadEmployeesFromJsonAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var employees = JsonSerializer.Deserialize<List<Employee>>(json);

            if (employees == null)
            {
                throw new InvalidOperationException("Не удалось десериализовать данные сотрудников");
            }

            await LogOperationAsync($"Загружено {employees.Count} сотрудников из файла: {Path.GetFileName(filePath)}");
            return employees;
        }

        /// <summary>
        /// Экспорт сотрудников в CSV формат
        /// </summary>
        /// <param name="employees">Список сотрудников</param>
        /// <param name="fileName">Имя файла (опционально)</param>
        /// <returns>Путь к сохраненному файлу</returns>
        public static async Task<string> ExportEmployeesToCsvAsync(List<Employee> employees, string? fileName = null)
        {
            fileName ??= $"employees_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
            var filePath = Path.Combine(DataDirectory, fileName);

            var csv = new StringBuilder();
            csv.AppendLine("ID,Имя,Фамилия,Должность,Отдел,Email,Дата создания,Дата обновления");

            foreach (var employee in employees)
            {
                csv.AppendLine($"{employee.Id},{employee.FirstName},{employee.LastName},{employee.Position},{employee.Department},{employee.Email},{employee.CreatedAt:yyyy-MM-dd HH:mm:ss},{employee.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);

            await LogOperationAsync($"Экспортировано {employees.Count} сотрудников в CSV: {fileName}");
            return filePath;
        }

        /// <summary>
        /// Создание отчета в текстовом формате
        /// </summary>
        /// <param name="reportContent">Содержимое отчета</param>
        /// <param name="reportTitle">Заголовок отчета</param>
        /// <param name="fileName">Имя файла (опционально)</param>
        /// <returns>Путь к сохраненному файлу</returns>
        public static async Task<string> CreateTextReportAsync(string reportContent, string reportTitle, string? fileName = null)
        {
            fileName ??= $"{reportTitle}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var filePath = Path.Combine(ReportsDirectory, fileName);

            var fullReport = $"{reportTitle}\n" +
                           $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                           $"==========================================\n\n" +
                           $"{reportContent}";

            await File.WriteAllTextAsync(filePath, fullReport, Encoding.UTF8);

            await LogOperationAsync($"Создан отчет: {fileName}");
            return filePath;
        }

        /// <summary>
        /// Создание отчета по отделам
        /// </summary>
        /// <param name="employees">Список сотрудников</param>
        /// <returns>Путь к сохраненному файлу</returns>
        public static async Task<string> CreateDepartmentReportAsync(List<Employee> employees)
        {
            var departments = employees.GroupBy(e => e.Department).OrderByDescending(g => g.Count());
            
            var reportContent = new StringBuilder();
            reportContent.AppendLine($"ОТЧЕТ ПО ОТДЕЛАМ");
            reportContent.AppendLine($"Всего сотрудников: {employees.Count}");
            reportContent.AppendLine($"Количество отделов: {departments.Count()}");
            reportContent.AppendLine();

            foreach (var dept in departments)
            {
                reportContent.AppendLine($"{dept.Key}: {dept.Count()} сотрудников");
                foreach (var emp in dept.OrderBy(e => e.LastName))
                {
                    reportContent.AppendLine($"  - {emp.FullName} ({emp.Position})");
                }
                reportContent.AppendLine();
            }

            return await CreateTextReportAsync(reportContent.ToString(), "Отчет по отделам");
        }

        /// <summary>
        /// Создание статистического отчета
        /// </summary>
        /// <param name="employees">Список сотрудников</param>
        /// <returns>Путь к сохраненному файлу</returns>
        public static async Task<string> CreateStatisticsReportAsync(List<Employee> employees)
        {
            var departments = employees.GroupBy(e => e.Department);
            var positions = employees.GroupBy(e => e.Position);
            
            var reportContent = new StringBuilder();
            reportContent.AppendLine($"СТАТИСТИЧЕСКИЙ ОТЧЕТ");
            reportContent.AppendLine($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
            reportContent.AppendLine();
            reportContent.AppendLine($"ОБЩАЯ СТАТИСТИКА:");
            reportContent.AppendLine($"• Всего сотрудников: {employees.Count}");
            reportContent.AppendLine($"• Количество отделов: {departments.Count()}");
            reportContent.AppendLine($"• Количество должностей: {positions.Count()}");
            reportContent.AppendLine($"• Среднее количество сотрудников на отдел: {(departments.Count() > 0 ? (double)employees.Count / departments.Count() : 0):F1}");
            reportContent.AppendLine();

            reportContent.AppendLine($"ТОП-5 ОТДЕЛОВ ПО КОЛИЧЕСТВУ СОТРУДНИКОВ:");
            foreach (var dept in departments.OrderByDescending(d => d.Count()).Take(5))
            {
                reportContent.AppendLine($"• {dept.Key}: {dept.Count()} сотрудников");
            }
            reportContent.AppendLine();

            reportContent.AppendLine($"ТОП-5 ДОЛЖНОСТЕЙ ПО КОЛИЧЕСТВУ СОТРУДНИКОВ:");
            foreach (var pos in positions.OrderByDescending(p => p.Count()).Take(5))
            {
                reportContent.AppendLine($"• {pos.Key}: {pos.Count()} сотрудников");
            }

            return await CreateTextReportAsync(reportContent.ToString(), "Статистический отчет");
        }

        /// <summary>
        /// Получение списка всех файлов отчетов
        /// </summary>
        /// <returns>Список файлов отчетов</returns>
        public static List<string> GetReportFiles()
        {
            if (!Directory.Exists(ReportsDirectory))
            {
                return new List<string>();
            }

            return Directory.GetFiles(ReportsDirectory, "*.txt")
                           .OrderByDescending(f => File.GetCreationTime(f))
                           .ToList();
        }

        /// <summary>
        /// Удаление файла отчета
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>True если файл удален успешно</returns>
        public static async Task<bool> DeleteReportFileAsync(string fileName)
        {
            var filePath = Path.Combine(ReportsDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                await LogOperationAsync($"Удален файл отчета: {fileName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Получение содержимого файла отчета
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Содержимое файла</returns>
        public static async Task<string> GetReportContentAsync(string fileName)
        {
            var filePath = Path.Combine(ReportsDirectory, fileName);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл отчета не найден: {fileName}");
            }

            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// Логирование операций
        /// </summary>
        /// <param name="operation">Описание операции</param>
        private static async Task LogOperationAsync(string operation)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {operation}";
            var logFile = Path.Combine(LogsDirectory, $"operations_{DateTime.Now:yyyy-MM-dd}.log");
            
            await File.AppendAllTextAsync(logFile, logEntry + Environment.NewLine, Encoding.UTF8);
        }

        /// <summary>
        /// Получение размера директории в байтах
        /// </summary>
        /// <param name="directoryPath">Путь к директории</param>
        /// <returns>Размер в байтах</returns>
        public static long GetDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return 0;
            }

            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                           .Sum(file => new FileInfo(file).Length);
        }

        /// <summary>
        /// Очистка старых файлов (старше указанного количества дней)
        /// </summary>
        /// <param name="days">Количество дней</param>
        /// <returns>Количество удаленных файлов</returns>
        public static async Task<int> CleanupOldFilesAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var deletedCount = 0;

            // Очистка отчетов
            if (Directory.Exists(ReportsDirectory))
            {
                var reportFiles = Directory.GetFiles(ReportsDirectory, "*", SearchOption.AllDirectories);
                foreach (var file in reportFiles)
                {
                    if (File.GetCreationTime(file) < cutoffDate)
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                }
            }

            // Очистка логов
            if (Directory.Exists(LogsDirectory))
            {
                var logFiles = Directory.GetFiles(LogsDirectory, "*.log");
                foreach (var file in logFiles)
                {
                    if (File.GetCreationTime(file) < cutoffDate)
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                }
            }

            await LogOperationAsync($"Очищено {deletedCount} старых файлов (старше {days} дней)");
            return deletedCount;
        }
    }
}
