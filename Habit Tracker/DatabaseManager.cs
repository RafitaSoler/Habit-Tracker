using Microsoft.Data.Sqlite;
using System.Data;

namespace Habit_Tracker
{
    internal static class DatabaseManager
    {
        private static readonly string _connectionString = "Data Source=habit-tracker.db;Foreign Keys=True";

        public static SqliteConnection OpenConnection()
        {
            SqliteConnection connection = new(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                connection.Dispose();
                Logger.Log($"Error: {e.Message}");
                throw;
            }
        }

        public static string FormatParameters(SqliteParameterCollection parameters)
        {
            if (parameters.Count == 0)
                return "(none)";

            List<string> parts = new();
            foreach(SqliteParameter parameter in parameters)
            {
                parts.Add($"{parameter.ParameterName}={parameter.Value}");
            }
            return string.Join(", ", parts);
        }

        public static void ExecuteNonQuery(SqliteConnection connection, string sql, (string, string)[]? parameters = null)
        {
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                    }
                }
                command.ExecuteNonQuery();
                Logger.Log($"Executed command: {command.CommandText} | Parameters: {FormatParameters(command.Parameters)}");
            }
            catch (Exception e)
            {
                Logger.Log($"Error: {e.Message}");
                throw;
            }
        }

        public static SqliteDataReader GetReader(SqliteConnection connection, string sql, (string, string)[]? parameters = null)
        {
            SqliteCommand command = connection.CreateCommand();
            try
            {
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                    }
                }
                SqliteDataReader reader = command.ExecuteReader();
                Logger.Log($"Executed command: {command.CommandText}");
                return reader;
            }
            catch (Exception e)
            {
                Logger.Log($"Error: {e.Message}");
                command.Dispose();
                throw;
            }
        }

        public static DataTable GetTable(string tableName)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteDataReader reader = GetReader(connection, $"SELECT * FROM {tableName}");
            DataTable table = new();
            table.Load(reader);
            return table;
        }

        public static void Start()
        {
            using SqliteConnection connection = OpenConnection();
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS Habits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE NOT NULL
                    )"
            );
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS HabitEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HabitId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    Quantity INTEGER DEFAULT 0 CHECK(Quantity >= 0),
                    FOREIGN KEY (HabitId) REFERENCES Habits(Id)
                    )"
            );
        }

        public static void CreateHabit(string habitName)
        {
            using SqliteConnection connection = OpenConnection();
            ExecuteNonQuery(connection, 
                @"INSERT INTO Habits (Name) VALUES (@name)", 
                [
                    ("@name", habitName)
                ]
            );
        }

        public static void LogHabit(string habitName, string date, int quantity)
        {
            using SqliteConnection connection = OpenConnection();
            ExecuteNonQuery(connection, 
                @"INSERT INTO HabitEntries (HabitId, Date, Quantity) VALUES ((SELECT Id FROM Habits WHERE Name = @name), @date, @quantity)", 
                [
                    ("@name", habitName), 
                    ("@date", date), 
                    ("@quantity", Math.Max(0, quantity).ToString())
                ]
            );
        }

        public static List<(int Id, string Name)> GetAllHabitsDataTable()
        {
            DataTable table = GetTable("Habits");
            List<(int Id, string Name)> habits = new();
            const int Id = 0;
            const int Name = 1;
            foreach(DataRow row in table.Rows)
            {
                habits.Add((Convert.ToInt32(row.ItemArray[Id]), (string)row.ItemArray[Name]));
            }
            return habits;
        }

        public static List<(string Name, string Date, int Quantity)> GetAllHabits()
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteDataReader reader = GetReader(connection, @"SELECT H.Name, HE.Date, HE.Quantity 
                                                                    FROM Habits as H 
                                                                    INNER JOIN HabitEntries as HE 
                                                                    ON H.Id = HE.HabitId");
            List<(string Name, string Date, int Quantity)> habits = new();
            const int Name = 0;
            const int Date = 1;
            const int Quantity = 2;
            while (reader.Read())
            {
                habits.Add((reader.GetString(Name), reader.GetString(Date), reader.GetInt32(Quantity)));
            }
            return habits;
        }
    }
}