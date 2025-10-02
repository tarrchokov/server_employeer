using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory
{
    /// <summary>
    /// Модель сотрудника для системы управления персоналом
    /// </summary>
    public sealed class Employee
    {
        /// <summary>
        /// Уникальный идентификатор сотрудника
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя сотрудника
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия сотрудника
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Должность сотрудника
        /// </summary>
        [Required(ErrorMessage = "Должность обязательна для заполнения")]
        [StringLength(100, ErrorMessage = "Должность не должна превышать 100 символов")]
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Отдел сотрудника
        /// </summary>
        [Required(ErrorMessage = "Отдел обязателен для заполнения")]
        [StringLength(100, ErrorMessage = "Отдел не должен превышать 100 символов")]
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта сотрудника
        /// </summary>
        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Полное имя сотрудника (имя + фамилия)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего обновления записи
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Employee()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="firstName">Имя</param>
        /// <param name="lastName">Фамилия</param>
        /// <param name="position">Должность</param>
        /// <param name="department">Отдел</param>
        /// <param name="email">Email</param>
        public Employee(string firstName, string lastName, string position, string department, string email)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Department = department;
            Email = email;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Обновление информации о сотруднике
        /// </summary>
        /// <param name="firstName">Новое имя</param>
        /// <param name="lastName">Новая фамилия</param>
        /// <param name="position">Новая должность</param>
        /// <param name="department">Новый отдел</param>
        /// <param name="email">Новый email</param>
        public void Update(string firstName, string lastName, string position, string department, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Department = department;
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Проверка валидности данных сотрудника
        /// </summary>
        /// <returns>True если данные валидны, иначе false</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Position) &&
                   !string.IsNullOrWhiteSpace(Department) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Email.Contains("@");
        }

        /// <summary>
        /// Получение строкового представления сотрудника
        /// </summary>
        /// <returns>Строка с информацией о сотруднике</returns>
        public override string ToString()
        {
            return $"{FullName} - {Position} ({Department}) - {Email}";
        }

        /// <summary>
        /// Сравнение сотрудников по идентификатору
        /// </summary>
        /// <param name="obj">Объект для сравнения</param>
        /// <returns>True если сотрудники одинаковые</returns>
        public override bool Equals(object? obj)
        {
            if (obj is Employee other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Получение хеш-кода сотрудника
        /// </summary>
        /// <returns>Хеш-код</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
