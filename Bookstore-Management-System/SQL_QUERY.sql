-- Создание базы данных
CREATE DATABASE [Bookstore-Management-System];
GO

-- Использование базы данных
USE [Bookstore-Management-System];
GO

-- Таблица для жанров
CREATE TABLE Genres (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL
);

-- Таблица для книг
CREATE TABLE Books (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    Publisher NVARCHAR(100),
	CoverImage varbinary(max), ---- Изображение титульной страницы
    PageCount INT,
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    YearPublished INT,
	PublishedDate datetime,
    CostPrice DECIMAL(18, 2),
    SellingPrice DECIMAL(18, 2),
    IsSequel BIT DEFAULT 0, -- Является ли продолжением (0 или 1)
    SequelToBookId INT NULL, -- Ссылка на другую книгу (если это продолжение)
    CONSTRAINT FK_SequelBook FOREIGN KEY (SequelToBookId) REFERENCES Books(Id)
);

-- Таблица для акций
CREATE TABLE Promotions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PromotionName NVARCHAR(100) NOT NULL,
    Discount DECIMAL(5, 2) NOT NULL, -- Скидка в процентах
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL
);

-- Таблица для связи книг и акций (многие ко многим)
CREATE TABLE BookPromotions (
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    PromotionId INT FOREIGN KEY REFERENCES Promotions(Id),
    PRIMARY KEY (BookId, PromotionId)
);

-- Таблица для менеджеров
CREATE TABLE Managers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ManagerName NVARCHAR(100) NOT NULL,
    ManagerSurname NVARCHAR(100) NOT NULL
);

-- Таблица для покупателей
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerSurname NVARCHAR(100) NOT NULL,
    City NVARCHAR(100),
    Address NVARCHAR(255),
    Tel NVARCHAR(20),
    Email NVARCHAR(50)
);

-- Таблица для продаж
CREATE TABLE Sales (
    SaleId INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    SaleDate DATE NOT NULL,
    Quantity INT NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    ManagerId INT FOREIGN KEY REFERENCES Managers(Id),
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id)
);

-- Таблица для отложенных книг
CREATE TABLE ReservedBooks (
    ReservationId INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
    ReservationDate DATE NOT NULL
);

-- Таблица для пользователей
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(100) NOT NULL, -- Хэш пароля
    Role NVARCHAR(50) NOT NULL -- Роль пользователя (например, "Admin", "User")
);

CREATE TABLE Order (
	Id INT PRIMARY KEY IDENTITY,
	OrderDate DATE NOT NULL,
	CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
	BookId INT FOREIGN KEY REFERENCES Books(Id),
	Quantity INT NOT NULL,
	TotalAmount DECIMAL(18, 2) NOT NULL
	);

-- Индексы для ускорения поиска
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Author ON Books(Author);
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Sales_SaleDate ON Sales(SaleDate);


--- Скрипт для заполнения таблиц тестовыми данными
-- Вставка данных в таблицу Genres
INSERT INTO Genres (Name)
VALUES
('Роман'),
('Фэнтези'),
('Антиутопия'),
('Детектив'),
('Научная фантастика'),
('Исторический'),
('Биография');

-- Вставка данных в таблицу Books
INSERT INTO Books (Title, Author, Publisher, PageCount, GenreId, YearPublished, CostPrice, SellingPrice, IsSequel, SequelToBookId)
VALUES
('Война и мир', 'Лев Толстой', 'Эксмо', 1225, 1, 1869, 500.00, 1200.00, 0, NULL),
('Преступление и наказание', 'Фёдор Достоевский', 'АСТ', 672, 1, 1866, 300.00, 950.00, 0, NULL),
('1984', 'Джордж Оруэлл', 'Азбука', 328, 3, 1949, 200.00, 800.00, 0, NULL),
('Мастер и Маргарита', 'Михаил Булгаков', 'Эксмо', 480, 1, 1967, 400.00, 1100.00, 0, NULL),
('Гарри Поттер и философский камень', 'Джоан Роулинг', 'Росмэн', 432, 2, 1997, 350.00, 900.00, 0, NULL),
('Гарри Поттер и Тайная комната', 'Джоан Роулинг', 'Росмэн', 480, 2, 1998, 350.00, 900.00, 1, 5),
('Маленький принц', 'Антуан де Сент-Экзюпери', 'Эксмо', 96, 1, 1943, 150.00, 500.00, 0, NULL);

/*
- Вставка данных в таблицу Books с изображениями
INSERT INTO Books (Title, Author, Publisher, PageCount, GenreId, YearPublished, CostPrice, SellingPrice, IsSequel, SequelToBookId, CoverImage)
VALUES
('Война и мир', 'Лев Толстой', 'Эксмо', 1225, 1, 1869, 500.00, 1200.00, 0, NULL, (SELECT BulkColumn FROM OPENROWSET(BULK 'C:\path_to_image1.jpg', SINGLE_BLOB) AS img)),
('Преступление и наказание', 'Фёдор Достоевский', 'АСТ', 672, 1, 1866, 300.00, 950.00, 0, NULL, (SELECT BulkColumn FROM OPENROWSET(BULK 'C:\path_to_image2.jpg', SINGLE_BLOB) AS img));
*/

-- Вставка данных в таблицу Promotions
INSERT INTO Promotions (PromotionName, Discount, StartDate, EndDate)
VALUES
('Новогодняя распродажа', 10.00, '2023-12-20', '2024-01-10'),
('Черная пятница', 20.00, '2023-11-24', '2023-11-26');

-- Вставка данных в таблицу BookPromotions
INSERT INTO BookPromotions (BookId, PromotionId)
VALUES
(1, 1), -- Война и мир в новогодней распродаже
(5, 1), -- Гарри Поттер и философский камень в новогодней распродаже
(3, 2); -- 1984 в черной пятнице

-- Вставка данных в таблицу Managers
INSERT INTO Managers (ManagerName, ManagerSurname)
VALUES
('Иван', 'Иванов'),
('Мария', 'Петрова');

-- Вставка данных в таблицу Customers
INSERT INTO Customers (CustomerName, CustomerSurname, City, Address, Tel, Email)
VALUES
('Алексей', 'Сидоров', 'Москва', 'ул. Ленина, 10', '+79161234567', 'alex@example.com'),
('Елена', 'Кузнецова', 'Санкт-Петербург', 'ул. Пушкина, 5', '+79167654321', 'elena@example.com');

-- Вставка данных в таблицу Sales
INSERT INTO Sales (BookId, SaleDate, Quantity, TotalAmount, ManagerId, CustomerId)
VALUES
(1, '2023-10-01', 2, 2400.00, 1, 1),
(5, '2023-10-02', 1, 900.00, 2, 2),
(3, '2023-10-03', 3, 2400.00, 1, 1);

-- Вставка данных в таблицу ReservedBooks
INSERT INTO ReservedBooks (BookId, CustomerId, ReservationDate)
VALUES
(2, 1, '2023-10-05'), -- Преступление и наказание отложено для Алексея
(4, 2, '2023-10-06'); -- Мастер и Маргарита отложено для Елены

-- Вставка данных в таблицу Users
INSERT INTO Users (Username, PasswordHash, Role)
VALUES
('admin', 'hashed_password_1', 'Admin'), -- Пароль должен быть хэширован
('user', 'hashed_password_2', 'Customer'), -- Пароль должен быть хэширован
('user2', 'hashed_password_3', 'Customer'), -- Пароль должен быть хэширован
('IvanovI', 'hashed_password_4', 'Managers'), -- Пароль должен быть хэширован
('PetrovaM', 'hashed_password_4', 'Managers'); -- Пароль должен быть хэширован


