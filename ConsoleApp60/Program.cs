using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ConsoleApp60
{

    internal class Program
    {
        private static SqlConnection OpenConnection()
        {
            string connectionString = @"Data Source=DESKTOP-2J3MN6S;Initial Catalog=BookstoreDB;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в Книжарню!");

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Добавить книгу");
                Console.WriteLine("2. Редактировать книгу");
                Console.WriteLine("3. Продать книгу");
                Console.WriteLine("4. Удалить книгу");
                Console.WriteLine("5. Поиск книги");
                Console.WriteLine("6. Список новинок");
                Console.WriteLine("7. Список популярных книг");
                Console.WriteLine("8. Выход");

                Console.Write("Выберите опцию: ");
                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        AddBook();
                        break;
                    case 2:
                        EditBook();
                        break;
                    case 3:
                        SellBook();
                        break;
                    case 4:
                        DeleteBook();
                        break;
                    case 5:
                        SearchBook();
                        break;
                    case 6:
                        ListNewBooks();
                        break;
                    case 7:
                        ListPopularBooks();
                        break;
                    case 8:
                        Console.WriteLine("До свидания!");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте ещё раз.");
                        break;
                }
            }

        }
        private static void AddBook()
        {
            Console.WriteLine("Введите информацию о книге:");
            Console.Write("Название: ");
            string title = Console.ReadLine();
            Console.Write("Автор: ");
            string author = Console.ReadLine();
            Console.Write("Издательство: ");
            string publisher = Console.ReadLine();
            Console.Write("Количество страниц: ");
            int pageCount = int.Parse(Console.ReadLine());
            Console.Write("Жанр: ");
            string genre = Console.ReadLine();
            Console.Write("Год издания: ");
            int publicationYear = int.Parse(Console.ReadLine());
            Console.Write("Собственная стоимость: ");
            decimal cost = decimal.Parse(Console.ReadLine());
            Console.Write("Цена для продажи: ");
            decimal price = decimal.Parse(Console.ReadLine());
            Console.Write("Книга является продолжением другой? (true/false): ");
            bool isSequel = bool.Parse(Console.ReadLine());

            int sequelOf = 0;
            if (isSequel)
            {
                Console.Write("ID книги, продолжение которой это книга: ");
                sequelOf = int.Parse(Console.ReadLine());
            }

            // Сохраните информацию в базе данных
            using (SqlConnection connection = OpenConnection())
            {
                string query = "INSERT INTO Books (Title, Author, Publisher, PageCount, Genre, PublicationYear, Cost, Price, IsSequel, SequelOf) " +
                               "VALUES (@Title, @Author, @Publisher, @PageCount, @Genre, @PublicationYear, @Cost, @Price, @IsSequel, @SequelOf)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Author", author);
                command.Parameters.AddWithValue("@Publisher", publisher);
                command.Parameters.AddWithValue("@PageCount", pageCount);
                command.Parameters.AddWithValue("@Genre", genre);
                command.Parameters.AddWithValue("@PublicationYear", publicationYear);
                command.Parameters.AddWithValue("@Cost", cost);
                command.Parameters.AddWithValue("@Price", price);
                command.Parameters.AddWithValue("@IsSequel", isSequel);
                command.Parameters.AddWithValue("@SequelOf", sequelOf);

                command.ExecuteNonQuery();
                Console.WriteLine("Книга успешно добавлена.");
            }
        }

        private static void EditBook()
        {
            Console.Write("Введите ID книги для редактирования: ");
            int bookID = int.Parse(Console.ReadLine());

            // Получите информацию о книге из базы данных
            using (SqlConnection connection = OpenConnection())
            {
                string query = "SELECT * FROM Books WHERE BookID = @BookID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BookID", bookID);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    Console.WriteLine("Текущая информация о книге:");
                    Console.WriteLine($"Название: {reader["Title"]}");
                    Console.WriteLine($"Автор: {reader["Author"]}");
                    // Выведите и редактируйте остальные поля

                    // Получите новую информацию о книге от пользователя
                    Console.WriteLine("\nВведите новую информацию о книге:");

                    Console.Write("Новое название: ");
                    string newTitle = Console.ReadLine();
                    Console.Write("Новый автор: ");
                    string newAuthor = Console.ReadLine();
                    // Заполните остальные поля для редактирования

                    // Выполните обновление записи в базе данных
                    string updateQuery = "UPDATE Books SET Title = @NewTitle, Author = @NewAuthor, ... WHERE BookID = @BookID";
                    SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@NewTitle", newTitle);
                    updateCommand.Parameters.AddWithValue("@NewAuthor", newAuthor);
                    // Добавьте параметры для остальных полей
                    updateCommand.Parameters.AddWithValue("@BookID", bookID);

                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Информация о книге успешно обновлена.");
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при обновлении информации о книге.");
                    }
                }
                else
                {
                    Console.WriteLine("Книга с указанным ID не найдена.");
                }
            }
        }


        private static void SellBook()
        {
            Console.Write("Введите ID книги для продажи: ");
            int bookID = int.Parse(Console.ReadLine());
            Console.Write("Введите количество проданных книг: ");
            int quantity = int.Parse(Console.ReadLine());

            using (SqlConnection connection = OpenConnection())
            {
                // Начнем транзакцию для обеспечения целостности данных
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Проверяем, есть ли такая книга и получаем её цену
                    string getPriceQuery = "SELECT Price FROM Books WHERE BookID = @BookID";
                    SqlCommand getPriceCommand = new SqlCommand(getPriceQuery, connection, transaction);
                    getPriceCommand.Parameters.AddWithValue("@BookID", bookID);

                    decimal bookPrice = (decimal)getPriceCommand.ExecuteScalar();
                    decimal totalAmount = bookPrice * quantity;

                    // Создаем заказ в базе данных
                    string createOrderQuery = "INSERT INTO Orders (CustomerName, OrderDate, TotalAmount) VALUES (@CustomerName, @OrderDate, @TotalAmount); SELECT SCOPE_IDENTITY();";
                    SqlCommand createOrderCommand = new SqlCommand(createOrderQuery, connection, transaction);
                    createOrderCommand.Parameters.AddWithValue("@CustomerName", "Customer Name"); // Замените на имя покупателя
                    createOrderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    createOrderCommand.Parameters.AddWithValue("@TotalAmount", totalAmount);

                    int orderID = Convert.ToInt32(createOrderCommand.ExecuteScalar());

                    // Добавляем запись о продаже книги в таблицу OrderItems
                    string addOrderItemQuery = "INSERT INTO OrderItems (OrderID, BookID, Quantity) VALUES (@OrderID, @BookID, @Quantity)";
                    SqlCommand addOrderItemCommand = new SqlCommand(addOrderItemQuery, connection, transaction);
                    addOrderItemCommand.Parameters.AddWithValue("@OrderID", orderID);
                    addOrderItemCommand.Parameters.AddWithValue("@BookID", bookID);
                    addOrderItemCommand.Parameters.AddWithValue("@Quantity", quantity);

                    int rowsAffected = addOrderItemCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Если всё прошло успешно, фиксируем транзакцию
                        transaction.Commit();
                        Console.WriteLine($"Книга продана. Сумма заказа: {totalAmount}");
                    }
                    else
                    {
                        Console.WriteLine("Произошла ошибка при добавлении книги к заказу.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка при продаже книги: " + ex.Message);

                    // Если произошла ошибка, откатываем транзакцию
                    transaction.Rollback();
                }
            }
        }


        private static void DeleteBook()
        {
            Console.Write("Введите ID книги для удаления: ");
            int bookID = int.Parse(Console.ReadLine());

            // Проверяем, есть ли заказы с этой книгой
            using (SqlConnection connection = OpenConnection())
            {
                string checkOrdersQuery = "SELECT COUNT(*) FROM OrderItems WHERE BookID = @BookID";
                SqlCommand checkOrdersCommand = new SqlCommand(checkOrdersQuery, connection);
                checkOrdersCommand.Parameters.AddWithValue("@BookID", bookID);
                int orderCount = (int)checkOrdersCommand.ExecuteScalar();

                if (orderCount > 0)
                {
                    Console.WriteLine("Нельзя удалить книгу, так как она есть в заказах.");
                    return; // Прерываем операцию удаления
                }
            }

            // Удаляем запись о книге из базы данных
            using (SqlConnection connection = OpenConnection())
            {
                string deleteQuery = "DELETE FROM Books WHERE BookID = @BookID";
                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@BookID", bookID);
                int rowsAffected = deleteCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Книга удалена.");
                }
                else
                {
                    Console.WriteLine("Книга с указанным ID не найдена.");
                }
            }
        }


        private static void SearchBook()
        {
            Console.Write("Введите название, автора или жанр книги для поиска: ");
            string searchQuery = Console.ReadLine();

            // Выполните поиск книги в базе данных и выведите результат
            using (SqlConnection connection = OpenConnection())
            {
                string query = "SELECT * FROM Books WHERE Title LIKE @SearchQuery OR Author LIKE @SearchQuery OR Genre LIKE @SearchQuery";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SearchQuery", $"%{searchQuery}%");
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("Результаты поиска:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["BookID"]}");
                        Console.WriteLine($"Название: {reader["Title"]}");
                        Console.WriteLine($"Автор: {reader["Author"]}");
                        Console.WriteLine($"Издательство: {reader["Publisher"]}");
                        Console.WriteLine($"Количество страниц: {reader["PageCount"]}");
                        Console.WriteLine($"Жанр: {reader["Genre"]}");
                        Console.WriteLine($"Год издания: {reader["PublicationYear"]}");
                        Console.WriteLine($"Собственная стоимость: {reader["Cost"]}");
                        Console.WriteLine($"Цена для продажи: {reader["Price"]}");
                        Console.WriteLine($"Продолжение: {reader["IsSequel"]}");
                        if (reader["IsSequel"] != DBNull.Value && (bool)reader["IsSequel"])
                        {
                            Console.WriteLine($"Продолжение книги с ID: {reader["SequelOf"]}");
                        }
                        Console.WriteLine("-----------");
                    }
                }
                else
                {
                    Console.WriteLine("По вашему запросу ничего не найдено.");
                }
            }
        }


        private static void ListNewBooks()
        {
            // Устанавливаем пороговый год, например, последний год
            int yearThreshold = DateTime.Now.Year - 1;

            using (SqlConnection connection = OpenConnection())
            {
                // SQL-запрос для выбора новых книг (год выпуска больше или равен yearThreshold)
                string query = "SELECT * FROM Books WHERE PublicationYear >= @YearThreshold";

                // Создаем объект команды SQL и передаем параметры
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@YearThreshold", yearThreshold);

                // Выполняем SQL-запрос и получаем результаты
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("Список новых книг:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Название: {reader["Title"]}");
                        Console.WriteLine($"Автор: {reader["Author"]}");
                        Console.WriteLine($"Издательство: {reader["Publisher"]}");
                        Console.WriteLine($"Количество страниц: {reader["PageCount"]}");
                        Console.WriteLine($"Жанр: {reader["Genre"]}");
                        Console.WriteLine($"Год выпуска: {reader["PublicationYear"]}");
                        Console.WriteLine($"Собственная стоимость: {reader["Cost"]}");
                        Console.WriteLine($"Цена для продажи: {reader["Price"]}");
                        Console.WriteLine($"Продолжение: {(bool)reader["IsSequel"]}");
                        if ((bool)reader["IsSequel"])
                        {
                            Console.WriteLine($"Книга продолжение к: {reader["SequelOf"]}");
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("Новые книги не найдены.");
                }
            }
        }


        private static void ListPopularBooks()
        {
            // Выведите список популярных книг из базы данных (например, книги с наибольшими продажами)
            using (SqlConnection connection = OpenConnection())
            {
                string query = "SELECT TOP 10 * FROM Books ORDER BY Price DESC";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("Список популярных книг:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Название: {reader["Title"]}");
                        Console.WriteLine($"Автор: {reader["Author"]}");
                        Console.WriteLine($"Издательство: {reader["Publisher"]}");
                        Console.WriteLine($"Количество страниц: {reader["PageCount"]}");
                        Console.WriteLine($"Жанр: {reader["Genre"]}");
                        Console.WriteLine($"Год издания: {reader["PublicationYear"]}");
                        Console.WriteLine($"Цена: {reader["Price"]}");
                        Console.WriteLine(); // Пустая строка для разделения книг
                    }
                }
                else
                {
                    Console.WriteLine("Популярные книги не найдены.");
                }
            }
        }


    }
}
