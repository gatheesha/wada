using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using wada.Models;

namespace wada.Data
{
    internal class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "AppDatabase.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        public List<UserModel> GetUsers()
        {
            var users = new List<UserModel>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Email FROM Users";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new UserModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                });
            }
            return users;
        }

        public void AddUser(string name, string email)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Name, Email) 
                VALUES ($name, $email);";
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$email", email);

            command.ExecuteNonQuery();
        }
    }
}
