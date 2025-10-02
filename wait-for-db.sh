#!/bin/bash

echo "🔄 Ожидание готовности SQL Server..."

# Функция для проверки подключения к базе данных
check_db() {
    /opt/mssql-tools/bin/sqlcmd -S db -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" -C -l 5 > /dev/null 2>&1
    return $?
}

# Счетчик попыток
attempts=0
max_attempts=60

echo "⏳ Проверяем подключение к SQL Server..."

while [ $attempts -lt $max_attempts ]; do
    if check_db; then
        echo "✅ SQL Server готов!"
        echo "🚀 Можно запускать приложение!"
        exit 0
    fi
    
    attempts=$((attempts + 1))
    echo "⏳ Попытка $attempts/$max_attempts - SQL Server еще не готов, ждем 5 секунд..."
    sleep 5
done

echo "❌ Ошибка: SQL Server не готов после $max_attempts попыток"
exit 1
