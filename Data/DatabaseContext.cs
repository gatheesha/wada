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
            try
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

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Client (
                            ClientID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            ClientName TEXT NOT NULL,
                            ClientContact TEXT,
                            ClientEmail TEXT
                        );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Project (
                            ProjectID INTEGER PRIMARY KEY AUTOINCREMENT,
                            StartDate TEXT,
                            EndDate TEXT,
                            Name TEXT NOT NULL,
                            ProjectStatus TEXT
                        );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Milestone (
                            MilestoneID INTEGER PRIMARY KEY AUTOINCREMENT,
                            MilestoneDescription TEXT,
                            Price REAL,
                            MilestoneDeadline TEXT,
                            MilestoneTimeRemaining TEXT,
                            ProjectID INTEGER,
                            FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
                        );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Task (
                            TaskID INTEGER PRIMARY KEY AUTOINCREMENT,
                            TaskDescription TEXT,
                            MilestoneID INTEGER,
                            TaskDeadline TEXT,
                            TaskTimeRemaining TEXT,
                            FOREIGN KEY (MilestoneID) REFERENCES Milestone(MilestoneID) ON DELETE CASCADE
                        );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Finance (
                            FinanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Amount REAL,
                            ProjectID INTEGER,
                            Date TEXT,
                            FinanceType TEXT,
                            FinanceDescription TEXT,
                            FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
                        );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ClientProject (
                            ClientID INTEGER,
                            ProjectID INTEGER,
                            PRIMARY KEY (ClientID, ProjectID),
                            FOREIGN KEY (ClientID) REFERENCES Client(ClientID) ON DELETE CASCADE,
                            FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
                        );";
                command.ExecuteNonQuery();
        }   
            
        catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Database Init Failed: {ex.Message}");
            }
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

        public void AddClient(string name, string contact, string email)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Client (ClientName, ClientContact, ClientEmail) 
                    VALUES ($name, $contact, $email);";

                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$contact", contact);
                command.Parameters.AddWithValue("$email", email);

                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"Error adding client: {ex.Message}"); }
        }

        public void AddProject(string name, string startDate, string endDate, string status)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Project (Name, StartDate, EndDate, ProjectStatus) 
                    VALUES ($name, $startDate, $endDate, $status);";

                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$startDate", startDate);
                command.Parameters.AddWithValue("$endDate", endDate);
                command.Parameters.AddWithValue("$status", status);

                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"Error adding project: {ex.Message}"); }
        }
        public void LinkClientToProject(int clientId, int projectId)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ClientProject (ClientID, ProjectID) VALUES ($clientId, $projectId);";
                command.Parameters.AddWithValue("$clientId", clientId);
                command.Parameters.AddWithValue("$projectId", projectId);

                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"Error linking client to project: {ex.Message}"); }
        }

        public List<ClientModel> FilterClient(string searchText)
        {
            var filtered = new List<ClientModel>();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ClientID, ClientName, ClientContact, ClientEmail FROM Client 
                    WHERE ClientName LIKE $search OR ClientContact LIKE $search OR ClientEmail LIKE $search;";
                command.Parameters.AddWithValue("$search", $"%{searchText}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    filtered.Add(new ClientModel
                    {
                        ClientID = reader.GetInt32(0),
                        ClientName = reader.GetString(1),
                        ClientContact = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        ClientEmail = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"FilterClient Error: {ex.Message}"); }
            return filtered;
        }

        public List<ProjectModel> FilterProject(string searchText)
        {
            var filtered = new List<ProjectModel>();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ProjectID, Name, StartDate, EndDate, ProjectStatus FROM Project WHERE Name LIKE $search;";
                command.Parameters.AddWithValue("$search", $"%{searchText}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    filtered.Add(new ProjectModel
                    {
                        ProjectID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        StartDate = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        EndDate = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        ProjectStatus = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"FilterProject Error: {ex.Message}"); }
            return filtered;
        }

        public void UpdateProject(ProjectModel project)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Project SET 
                        Name = $name, 
                        StartDate = $startDate, 
                        EndDate = $endDate, 
                        ProjectStatus = $status 
                    WHERE ProjectID = $id;";

                command.Parameters.AddWithValue("$name", project.Name);
                command.Parameters.AddWithValue("$startDate", project.StartDate);
                command.Parameters.AddWithValue("$endDate", project.EndDate);
                command.Parameters.AddWithValue("$status", project.ProjectStatus);
                command.Parameters.AddWithValue("$id", project.ProjectID);

                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"UpdateProject Error: {ex.Message}"); }
        }
    }
}
