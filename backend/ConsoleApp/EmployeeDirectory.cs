using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDirectory
{
    /// <summary>
    /// Главный класс консольного приложения EmployeeDirectory
    /// </summary>
    class Program
    {
        private static List<Employee> employees = new List<Employee>();
        private static bool isRunning = true;

        /// <summary>
        /// Точка входа в приложение
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== СИСТЕМА УПРАВЛЕНИЯ ПЕРСОНАЛОМ ===");
            Console.WriteLine("Версия: 1.0.0");
            Console.WriteLine("Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            Console.WriteLine();

            // Загрузка тестовых данных
            await LoadSampleDataAsync();

            // Главный цикл приложения
            while (isRunning)
            {
                try
                {
                    await ShowMainMenuAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    try
                    {
                        Console.ReadKey();
                    }
                    catch (InvalidOperationException)
                    {
                        // Игнорируем ошибку при автоматическом тестировании
                    }
                }
            }

            Console.WriteLine("Спасибо за использование системы!");
        }

        /// <summary>
        /// Отображение главного меню
        /// </summary>
        static async Task ShowMainMenuAsync()
        {
            Console.Clear();
            Console.WriteLine("=== ГЛАВНОЕ МЕНЮ ===");
            Console.WriteLine($"Всего сотрудников в системе: {employees.Count}");
            Console.WriteLine();
            Console.WriteLine("1. Просмотр всех сотрудников");
            Console.WriteLine("2. Добавить сотрудника");
            Console.WriteLine("3. Редактировать сотрудника");
            Console.WriteLine("4. Удалить сотрудника");
            Console.WriteLine("5. Поиск сотрудников");
            Console.WriteLine("6. Статистика");
            Console.WriteLine("7. Отчеты");
            Console.WriteLine("8. Работа с файлами");
            Console.WriteLine("9. Тестирование аутентификации");
            Console.WriteLine("0. Выход");
            Console.WriteLine();
            Console.Write("Выберите действие (0-9): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await ShowAllEmployeesAsync();
                    break;
                case "2":
                    await AddEmployeeAsync();
                    break;
                case "3":
                    await EditEmployeeAsync();
                    break;
                case "4":
                    await DeleteEmployeeAsync();
                    break;
                case "5":
                    await SearchEmployeesAsync();
                    break;
                case "6":
                    await ShowStatisticsAsync();
                    break;
                case "7":
                    await GenerateReportsAsync();
                    break;
                case "8":
                    await FileOperationsAsync();
                    break;
                case "9":
                    await TestAuthenticationAsync();
                    break;
                case "0":
                    isRunning = false;
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            if (isRunning)
            {
                Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                try
                {
                    Console.ReadKey();
                }
                catch (InvalidOperationException)
                {
                    // Игнорируем ошибку при автоматическом тестировании
                }
            }
        }

        /// <summary>
        /// Отображение всех сотрудников
        /// </summary>
        static async Task ShowAllEmployeesAsync()
        {
            Console.WriteLine("=== СПИСОК ВСЕХ СОТРУДНИКОВ ===");
            
            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            Console.WriteLine($"Найдено сотрудников: {employees.Count}");
            Console.WriteLine();

            for (int i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];
                Console.WriteLine($"{i + 1}. {emp.FullName}");
                Console.WriteLine($"   Должность: {emp.Position}");
                Console.WriteLine($"   Отдел: {emp.Department}");
                Console.WriteLine($"   Email: {emp.Email}");
                Console.WriteLine($"   ID: {emp.Id}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        static async Task AddEmployeeAsync()
        {
            Console.WriteLine("=== ДОБАВЛЕНИЕ СОТРУДНИКА ===");
            
            Console.Write("Имя: ");
            var firstName = Console.ReadLine()?.Trim();
            
            Console.Write("Фамилия: ");
            var lastName = Console.ReadLine()?.Trim();
            
            Console.Write("Должность: ");
            var position = Console.ReadLine()?.Trim();
            
            Console.Write("Отдел: ");
            var department = Console.ReadLine()?.Trim();
            
            Console.Write("Email: ");
            var email = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
                string.IsNullOrEmpty(position) || string.IsNullOrEmpty(department) || 
                string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Ошибка: Все поля обязательны для заполнения.");
                return;
            }

            var employee = new Employee(firstName, lastName, position, department, email);
            
            if (!employee.IsValid())
            {
                Console.WriteLine("Ошибка: Некорректные данные сотрудника.");
                return;
            }

            employees.Add(employee);
            Console.WriteLine($"Сотрудник {employee.FullName} успешно добавлен!");
        }

        /// <summary>
        /// Редактирование сотрудника
        /// </summary>
        static async Task EditEmployeeAsync()
        {
            Console.WriteLine("=== РЕДАКТИРОВАНИЕ СОТРУДНИКА ===");
            
            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            Console.Write("Введите ID сотрудника для редактирования: ");
            var idInput = Console.ReadLine();
            
            if (!Guid.TryParse(idInput, out var employeeId))
            {
                Console.WriteLine("Неверный формат ID.");
                return;
            }

            var employee = employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                Console.WriteLine("Сотрудник не найден.");
                return;
            }

            Console.WriteLine($"Редактирование: {employee.FullName}");
            Console.WriteLine();

            Console.Write($"Имя ({employee.FirstName}): ");
            var firstName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(firstName)) firstName = employee.FirstName;

            Console.Write($"Фамилия ({employee.LastName}): ");
            var lastName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(lastName)) lastName = employee.LastName;

            Console.Write($"Должность ({employee.Position}): ");
            var position = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(position)) position = employee.Position;

            Console.Write($"Отдел ({employee.Department}): ");
            var department = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(department)) department = employee.Department;

            Console.Write($"Email ({employee.Email}): ");
            var email = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(email)) email = employee.Email;

            employee.Update(firstName, lastName, position, department, email);
            Console.WriteLine($"Сотрудник {employee.FullName} успешно обновлен!");
        }

        /// <summary>
        /// Удаление сотрудника
        /// </summary>
        static async Task DeleteEmployeeAsync()
        {
            Console.WriteLine("=== УДАЛЕНИЕ СОТРУДНИКА ===");
            
            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            Console.Write("Введите ID сотрудника для удаления: ");
            var idInput = Console.ReadLine();
            
            if (!Guid.TryParse(idInput, out var employeeId))
            {
                Console.WriteLine("Неверный формат ID.");
                return;
            }

            var employee = employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                Console.WriteLine("Сотрудник не найден.");
                return;
            }

            Console.WriteLine($"Вы уверены, что хотите удалить сотрудника {employee.FullName}? (y/n)");
            var confirm = Console.ReadLine()?.ToLower();
            
            if (confirm == "y" || confirm == "yes")
            {
                employees.Remove(employee);
                Console.WriteLine($"Сотрудник {employee.FullName} успешно удален!");
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
        }

        /// <summary>
        /// Поиск сотрудников
        /// </summary>
        static async Task SearchEmployeesAsync()
        {
            Console.WriteLine("=== ПОИСК СОТРУДНИКОВ ===");
            
            Console.Write("Введите поисковый запрос (имя, фамилия, должность, отдел): ");
            var query = Console.ReadLine()?.Trim().ToLower();
            
            if (string.IsNullOrEmpty(query))
            {
                Console.WriteLine("Поисковый запрос не может быть пустым.");
                return;
            }

            var results = employees.Where(e => 
                e.FirstName.ToLower().Contains(query) ||
                e.LastName.ToLower().Contains(query) ||
                e.Position.ToLower().Contains(query) ||
                e.Department.ToLower().Contains(query) ||
                e.Email.ToLower().Contains(query)
            ).ToList();

            Console.WriteLine($"Найдено сотрудников: {results.Count}");
            Console.WriteLine();

            foreach (var emp in results)
            {
                Console.WriteLine($"• {emp.FullName} - {emp.Position} ({emp.Department})");
            }
        }

        /// <summary>
        /// Отображение статистики
        /// </summary>
        static async Task ShowStatisticsAsync()
        {
            Console.WriteLine("=== СТАТИСТИКА ===");
            
            if (employees.Count == 0)
            {
                Console.WriteLine("Нет данных для отображения статистики.");
                return;
            }

            var departments = employees.GroupBy(e => e.Department);
            var positions = employees.GroupBy(e => e.Position);

            Console.WriteLine($"Общее количество сотрудников: {employees.Count}");
            Console.WriteLine($"Количество отделов: {departments.Count()}");
            Console.WriteLine($"Количество должностей: {positions.Count()}");
            Console.WriteLine($"Среднее количество сотрудников на отдел: {(double)employees.Count / departments.Count():F1}");
            Console.WriteLine();

            Console.WriteLine("ТОП-5 ОТДЕЛОВ:");
            foreach (var dept in departments.OrderByDescending(d => d.Count()).Take(5))
            {
                Console.WriteLine($"• {dept.Key}: {dept.Count()} сотрудников");
            }

            Console.WriteLine();
            Console.WriteLine("ТОП-5 ДОЛЖНОСТЕЙ:");
            foreach (var pos in positions.OrderByDescending(p => p.Count()).Take(5))
            {
                Console.WriteLine($"• {pos.Key}: {pos.Count()} сотрудников");
            }
        }

        /// <summary>
        /// Генерация отчетов
        /// </summary>
        static async Task GenerateReportsAsync()
        {
            Console.WriteLine("=== ГЕНЕРАЦИЯ ОТЧЕТОВ ===");
            
            if (employees.Count == 0)
            {
                Console.WriteLine("Нет данных для создания отчетов.");
                return;
            }

            Console.WriteLine("Выберите тип отчета:");
            Console.WriteLine("1. Общий отчет");
            Console.WriteLine("2. Отчет по отделам");
            Console.WriteLine("3. Статистический отчет");
            Console.WriteLine("4. Экспорт в CSV");
            Console.Write("Выбор (1-4): ");

            var choice = Console.ReadLine();
            string reportPath = "";

            try
            {
                switch (choice)
                {
                    case "1":
                        reportPath = await FileManager.CreateTextReportAsync(
                            GenerateGeneralReport(), "Общий отчет");
                        break;
                    case "2":
                        reportPath = await FileManager.CreateDepartmentReportAsync(employees);
                        break;
                    case "3":
                        reportPath = await FileManager.CreateStatisticsReportAsync(employees);
                        break;
                    case "4":
                        reportPath = await FileManager.ExportEmployeesToCsvAsync(employees);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        return;
                }

                Console.WriteLine($"Отчет успешно создан: {reportPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании отчета: {ex.Message}");
            }
        }

        /// <summary>
        /// Операции с файлами
        /// </summary>
        static async Task FileOperationsAsync()
        {
            Console.WriteLine("=== РАБОТА С ФАЙЛАМИ ===");
            Console.WriteLine("1. Сохранить данные в JSON");
            Console.WriteLine("2. Загрузить данные из JSON");
            Console.WriteLine("3. Показать список отчетов");
            Console.WriteLine("4. Очистить старые файлы");
            Console.Write("Выбор (1-4): ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        var jsonPath = await FileManager.SaveEmployeesToJsonAsync(employees);
                        Console.WriteLine($"Данные сохранены в: {jsonPath}");
                        break;
                    case "2":
                        Console.Write("Введите путь к JSON файлу: ");
                        var filePath = Console.ReadLine();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            employees = await FileManager.LoadEmployeesFromJsonAsync(filePath);
                            Console.WriteLine($"Загружено {employees.Count} сотрудников.");
                        }
                        break;
                    case "3":
                        var reports = FileManager.GetReportFiles();
                        Console.WriteLine($"Найдено отчетов: {reports.Count}");
                        foreach (var report in reports.Take(10))
                        {
                            Console.WriteLine($"• {Path.GetFileName(report)}");
                        }
                        break;
                    case "4":
                        Console.Write("Удалить файлы старше скольких дней? (по умолчанию 30): ");
                        var daysInput = Console.ReadLine();
                        var days = int.TryParse(daysInput, out var d) ? d : 30;
                        var deletedCount = await FileManager.CleanupOldFilesAsync(days);
                        Console.WriteLine($"Удалено файлов: {deletedCount}");
                        break;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Тестирование аутентификации
        /// </summary>
        static async Task TestAuthenticationAsync()
        {
            Console.WriteLine("=== ТЕСТИРОВАНИЕ АУТЕНТИФИКАЦИИ ===");
            
            Console.Write("Введите пароль для тестирования: ");
            var password = Console.ReadLine();
            
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Пароль не может быть пустым.");
                return;
            }

            // Хеширование пароля
            var hashedPassword = HashAuth.HashPassword(password);
            Console.WriteLine($"Хеш пароля: {hashedPassword}");
            
            // Проверка пароля
            var isValid = HashAuth.VerifyPassword(password, hashedPassword);
            Console.WriteLine($"Проверка пароля: {(isValid ? "УСПЕШНО" : "ОШИБКА")}");
            
            // Проверка силы пароля
            var strength = HashAuth.CheckPasswordStrength(password);
            Console.WriteLine($"Сила пароля: {strength.Level} (оценка: {strength.Score}/6)");
            
            if (strength.Recommendations.Length > 0)
            {
                Console.WriteLine("Рекомендации:");
                foreach (var rec in strength.Recommendations)
                {
                    Console.WriteLine($"• {rec}");
                }
            }
            
            // Генерация случайного пароля
            var randomPassword = HashAuth.GenerateRandomPassword(12, true);
            Console.WriteLine($"Пример безопасного пароля: {randomPassword}");
        }

        /// <summary>
        /// Загрузка тестовых данных
        /// </summary>
        static async Task LoadSampleDataAsync()
        {
            employees.AddRange(new[]
            {
                new Employee("Иван", "Иванов", "Разработчик", "IT", "ivan.ivanov@company.com"),
                new Employee("Петр", "Петров", "Менеджер", "Продажи", "petr.petrov@company.com"),
                new Employee("Мария", "Сидорова", "Дизайнер", "Маркетинг", "maria.sidorova@company.com"),
                new Employee("Алексей", "Козлов", "Аналитик", "Финансы", "alexey.kozlov@company.com"),
                new Employee("Елена", "Морозова", "HR-менеджер", "HR", "elena.morozova@company.com")
            });
        }

        /// <summary>
        /// Генерация общего отчета
        /// </summary>
        static string GenerateGeneralReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine($"Всего сотрудников: {employees.Count}");
            report.AppendLine($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
            report.AppendLine();
            report.AppendLine("СПИСОК СОТРУДНИКОВ:");
            
            foreach (var emp in employees.OrderBy(e => e.LastName))
            {
                report.AppendLine($"• {emp.FullName} - {emp.Position} ({emp.Department}) - {emp.Email}");
            }
            
            return report.ToString();
        }
    }
}
