# Employee Directory - Backend API

API сервер для управления справочником сотрудников с авторизацией и отчетами.

## 🚀 Быстрый старт

### Требования
- Docker
- Docker Compose

### Запуск

```bash
git clone <ссылка-на-репозиторий>
cd employee-directory-backend
docker-compose up --build
```

**Готово!** Сервер запустится на `http://localhost:8080`

## 📝 Учетные данные по умолчанию

**Администратор:**
- Логин: `admin`
- Пароль: `admin123`

## 🛠️ Технологии

- **.NET 9.0** - основной фреймворк
- **Entity Framework Core** - ORM для работы с БД
- **SQL Server** - база данных
- **JWT** - аутентификация
- **BCrypt** - хеширование паролей
- **Docker** - контейнеризация

## 📚 API Endpoints

### Авторизация
- `POST /api/auth/register` - регистрация
- `POST /api/auth/login` - вход

### Сотрудники
- `GET /api/employees` - список сотрудников
- `POST /api/employees` - добавить сотрудника (только админ)
- `PUT /api/employees/{id}` - обновить сотрудника (только админ)
- `DELETE /api/employees/{id}` - удалить сотрудника (только админ)

### Отчеты
- `GET /api/reports` - список отчетов
- `POST /api/reports` - создать отчет
- `GET /api/reports/{id}` - получить отчет
- `DELETE /api/reports/{id}` - удалить отчет
- `GET /api/reports/download/{id}` - скачать отчет
- `GET /api/reports/statistics` - статистика

## 🔧 Конфигурация

Настройки в `docker-compose.yml`:

```yaml
environment:
  ConnectionStrings__DefaultConnection: Server=db,1433;Database=empdir;...
  Jwt__Key: supersecret_dev_key_change_me_32chars
  Jwt__Issuer: EmployeeDirectory
  Jwt__Audience: EmployeeDirectory.Clients
```

## 📦 Структура проекта

```
Backend/
├── Program.cs                    # Основной файл приложения
├── EmployeeDirectory.Api.csproj  # Конфигурация проекта
├── appsettings.json              # Настройки приложения
├── Dockerfile                    # Docker образ
├── Migrations/                   # Миграции БД
│   ├── 20250922215353_InitialCreate.cs
│   └── AppDbContextModelSnapshot.cs
└── migration_add_position.sql    # SQL миграция
```

## 🐳 Docker команды

```bash
# Запуск
docker-compose up --build

# Запуск в фоне
docker-compose up -d

# Остановка
docker-compose down

# Полная очистка (удалит БД)
docker-compose down -v

# Логи
docker-compose logs -f

# Перезапуск только API
docker-compose restart api
```

## 🧪 Swagger

Документация API доступна по адресу:
`http://localhost:8080/swagger`

## 📊 База данных

- **СУБД:** Microsoft SQL Server 2022
- **Порт:** 1433
- **Пароль SA:** YourStrong@Passw0rd

### Подключение к БД

```bash
docker exec -it <container_name> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"
```

## 🔐 Безопасность

⚠️ **Важно для продакшена:**
- Измените пароль БД
- Измените JWT ключ (минимум 32 символа)
- Используйте HTTPS
- Настройте CORS
- Используйте переменные окружения для секретов

## 📄 Лицензия

MIT

