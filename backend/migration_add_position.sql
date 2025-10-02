-- Миграция для добавления поля Position в таблицу Employees
-- Выполните этот скрипт в базе данных

ALTER TABLE Employees ADD Position NVARCHAR(255) NOT NULL DEFAULT 'Не указана';

-- Обновляем существующие записи с примерными должностями
UPDATE Employees SET Position = 'Разработчик' WHERE Department = 'IT';
UPDATE Employees SET Position = 'Менеджер' WHERE Department = 'HR';
UPDATE Employees SET Position = 'Бухгалтер' WHERE Department = 'Finance';
UPDATE Employees SET Position = 'Аналитик' WHERE Department = 'Analytics';
UPDATE Employees SET Position = 'Дизайнер' WHERE Department = 'Design';
UPDATE Employees SET Position = 'Тестировщик' WHERE Department = 'QA';
UPDATE Employees SET Position = 'Администратор' WHERE Department = 'Admin';
UPDATE Employees SET Position = 'Консультант' WHERE Department = 'Consulting';
UPDATE Employees SET Position = 'Маркетолог' WHERE Department = 'Marketing';
UPDATE Employees SET Position = 'Продавец' WHERE Department = 'Sales';
