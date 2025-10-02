# ИНСТРУКЦИЯ ПО ЗАПУСКУ ПРОГРАММЫ С НУЛЯ

## 1. Перейти в папку проекта
cd /Users/islamtarcokov/Desktop/V

## 2. Запуск сервера (backend)
docker-compose up --build

## 3. Запуск клиента (Android)
### Вариант A: Через Android Studio
1. Открыть папку "Android" в Android Studio
2. Синхронизировать проект (Sync Project with Gradle Files)
3. Запустить программу на эмуляторе или устройстве

### Вариант B: Установка готового APK
1. Скопировать файл `EmployeeDirectory.apk` на Android устройство
2. Установить APK файл
3. Запустить приложение

## 4. Вход в приложение
### Администратор
- Логин: admin
- Пароль: admin123

### Обычный пользователь
- Регистрация через приложение

## 5. Проверка работы
- Сервер должен быть доступен на http://localhost:8080
- Android приложение подключается к серверу автоматически
- Данные сохраняются между перезапусками сервера

## 6. Остановка и очистка
# Остановить сервер
docker-compose down

# Полная очистка (удалить все данные)
docker-compose down -v
docker rmi v-api
docker volume rm v_empdir_db

## 7. Запуск с нуля (после полной очистки)
docker-compose up --build

## 8. Дополнительные команды
# Проверить статус сервера
curl -X POST http://localhost:8080/api/auth/login -H "Content-Type: application/json" -d '{"userName":"admin","password":"admin123"}'

# Просмотр логов
docker-compose logs

# Перезапуск только API
docker-compose restart api