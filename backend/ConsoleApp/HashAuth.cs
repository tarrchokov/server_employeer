using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDirectory
{
    /// <summary>
    /// Класс для хеширования и аутентификации паролей
    /// </summary>
    public static class HashAuth
    {
        private const int SaltSize = 32; // Размер соли в байтах
        private const int HashSize = 32; // Размер хеша в байтах
        private const int Iterations = 10000; // Количество итераций для PBKDF2

        /// <summary>
        /// Создание хеша пароля с солью
        /// </summary>
        /// <param name="password">Пароль для хеширования</param>
        /// <returns>Хеш пароля в формате base64</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));
            }

            // Генерация случайной соли
            byte[] salt = GenerateSalt();

            // Создание хеша с использованием PBKDF2
            byte[] hash = PBKDF2(password, salt, Iterations, HashSize);

            // Объединение соли и хеша
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Проверка пароля против хеша
        /// </summary>
        /// <param name="password">Пароль для проверки</param>
        /// <param name="hashedPassword">Хешированный пароль</param>
        /// <returns>True если пароль верный</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            try
            {
                // Декодирование хеша из base64
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                if (hashBytes.Length != SaltSize + HashSize)
                {
                    return false;
                }

                // Извлечение соли
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Извлечение хеша
                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

                // Вычисление хеша для проверяемого пароля
                byte[] computedHash = PBKDF2(password, salt, Iterations, HashSize);

                // Сравнение хешей
                return SlowEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Генерация случайной соли
        /// </summary>
        /// <returns>Массив байтов соли</returns>
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Реализация PBKDF2 для создания хеша
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <param name="salt">Соль</param>
        /// <param name="iterations">Количество итераций</param>
        /// <param name="outputBytes">Размер выходного хеша</param>
        /// <returns>Хеш пароля</returns>
        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(outputBytes);
            }
        }

        /// <summary>
        /// Безопасное сравнение массивов байтов для предотвращения атак по времени
        /// </summary>
        /// <param name="a">Первый массив</param>
        /// <param name="b">Второй массив</param>
        /// <returns>True если массивы одинаковые</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            uint diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }

            return diff == 0;
        }

        /// <summary>
        /// Генерация случайного пароля
        /// </summary>
        /// <param name="length">Длина пароля</param>
        /// <param name="includeSpecialChars">Включать ли специальные символы</param>
        /// <returns>Сгенерированный пароль</returns>
        public static string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true)
        {
            if (length < 4)
            {
                throw new ArgumentException("Длина пароля должна быть не менее 4 символов", nameof(length));
            }

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            string chars = lowercase + uppercase + digits;
            if (includeSpecialChars)
            {
                chars += special;
            }

            var password = new StringBuilder();
            var random = new Random();

            // Гарантируем наличие хотя бы одного символа каждого типа
            password.Append(lowercase[random.Next(lowercase.Length)]);
            password.Append(uppercase[random.Next(uppercase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            if (includeSpecialChars)
            {
                password.Append(special[random.Next(special.Length)]);
            }

            // Заполняем оставшуюся длину случайными символами
            for (int i = password.Length; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            // Перемешиваем символы
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }

        /// <summary>
        /// Проверка силы пароля
        /// </summary>
        /// <param name="password">Пароль для проверки</param>
        /// <returns>Результат проверки силы пароля</returns>
        public static PasswordStrength CheckPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new PasswordStrength { Score = 0, Level = "Очень слабый", Recommendations = new[] { "Пароль не может быть пустым" } };
            }

            int score = 0;
            var recommendations = new List<string>();

            // Проверка длины
            if (password.Length >= 8) score += 1;
            else recommendations.Add("Используйте минимум 8 символов");

            if (password.Length >= 12) score += 1;
            else if (password.Length < 8) recommendations.Add("Рекомендуется использовать 12+ символов");

            // Проверка наличия различных типов символов
            if (password.Any(char.IsLower)) score += 1;
            else recommendations.Add("Добавьте строчные буквы (a-z)");

            if (password.Any(char.IsUpper)) score += 1;
            else recommendations.Add("Добавьте заглавные буквы (A-Z)");

            if (password.Any(char.IsDigit)) score += 1;
            else recommendations.Add("Добавьте цифры (0-9)");

            if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) score += 1;
            else recommendations.Add("Добавьте специальные символы (!@#$%^&*...)");

            // Проверка на повторяющиеся символы
            if (password.Distinct().Count() == password.Length) score += 1;
            else recommendations.Add("Избегайте повторяющихся символов");

            // Определение уровня
            string level = score switch
            {
                <= 2 => "Очень слабый",
                <= 3 => "Слабый",
                <= 4 => "Средний",
                <= 5 => "Хороший",
                _ => "Отличный"
            };

            return new PasswordStrength
            {
                Score = score,
                Level = level,
                Recommendations = recommendations.ToArray()
            };
        }

        /// <summary>
        /// Создание токена для сброса пароля
        /// </summary>
        /// <returns>Токен сброса пароля</returns>
        public static string GeneratePasswordResetToken()
        {
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        /// <summary>
        /// Проверка токена сброса пароля
        /// </summary>
        /// <param name="token">Токен для проверки</param>
        /// <param name="storedToken">Сохраненный токен</param>
        /// <param name="expirationTime">Время истечения токена</param>
        /// <returns>True если токен валидный</returns>
        public static bool ValidatePasswordResetToken(string token, string storedToken, DateTime expirationTime)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(storedToken))
            {
                return false;
            }

            if (DateTime.UtcNow > expirationTime)
            {
                return false;
            }

            return token == storedToken;
        }
    }

    /// <summary>
    /// Результат проверки силы пароля
    /// </summary>
    public class PasswordStrength
    {
        /// <summary>
        /// Оценка силы пароля (0-6)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Уровень силы пароля
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Рекомендации по улучшению пароля
        /// </summary>
        public string[] Recommendations { get; set; } = Array.Empty<string>();
    }
}
