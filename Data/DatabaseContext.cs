using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
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

                // Enable foreign key enforcement (SQLite requires this per connection)
                var pragma = connection.CreateCommand();
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();

                var command = connection.CreateCommand();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name    TEXT NOT NULL,
                        Email   TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Client (
                        ClientID      INTEGER PRIMARY KEY AUTOINCREMENT,
                        ClientName    TEXT NOT NULL,
                        ClientContact TEXT,
                        ClientEmail   TEXT
                    );";
                command.ExecuteNonQuery();

                // Description column added vs original schema
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Project (
                        ProjectID     INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name          TEXT NOT NULL,
                        Description   TEXT,
                        StartDate     TEXT,
                        EndDate       TEXT,
                        ProjectStatus TEXT
                    );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Milestone (
                        MilestoneID          INTEGER PRIMARY KEY AUTOINCREMENT,
                        MilestoneDescription TEXT,
                        Price                REAL,
                        MilestoneDeadline    TEXT,
                        ProjectID            INTEGER,
                        FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // TaskName added — TaskDescription kept for optional longer notes
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Task (
                        TaskID          INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskName        TEXT NOT NULL,
                        TaskDescription TEXT,
                        TaskDeadline    TEXT,
                        IsCompleted     INTEGER NOT NULL DEFAULT 0,
                        MilestoneID     INTEGER,
                        FOREIGN KEY (MilestoneID) REFERENCES Milestone(MilestoneID) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Finance (
                        FinanceID          INTEGER PRIMARY KEY AUTOINCREMENT,
                        Amount             REAL,
                        ProjectID          INTEGER,
                        Date               TEXT,
                        FinanceType        TEXT,
                        FinanceDescription TEXT,
                        FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ClientProject (
                        ClientID  INTEGER,
                        ProjectID INTEGER,
                        PRIMARY KEY (ClientID, ProjectID),
                        FOREIGN KEY (ClientID)  REFERENCES Client(ClientID)   ON DELETE CASCADE,
                        FOREIGN KEY (ProjectID) REFERENCES Project(ProjectID) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Database Init Failed: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────

        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Foreign keys must be enabled per connection in SQLite
            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            return connection;
        }

        private long GetLastInsertedId(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT last_insert_rowid();";
            return (long)command.ExecuteScalar();
        }

        // ─────────────────────────────────────────────
        //  CLIENTS
        // ─────────────────────────────────────────────

        public List<ClientModel> GetAllClients()
        {
            var clients = new List<ClientModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ClientID, ClientName, ClientContact, ClientEmail FROM Client ORDER BY ClientName;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new ClientModel
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        MobileNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"GetAllClients Error: {ex.Message}"); }
            return clients;
        }

        public List<ClientModel> FilterClient(string searchText)
        {
            var filtered = new List<ClientModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ClientID, ClientName, ClientContact, ClientEmail FROM Client
                    WHERE ClientName    LIKE $search
                       OR ClientContact LIKE $search
                       OR ClientEmail   LIKE $search
                    ORDER BY ClientName;";
                command.Parameters.AddWithValue("$search", $"%{searchText}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    filtered.Add(new ClientModel
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        MobileNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"FilterClient Error: {ex.Message}"); }
            return filtered;
        }

        /// <summary>Adds a client and returns its new ClientID.</summary>
        public int AddClient(string name, string contact, string email)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Client (ClientName, ClientContact, ClientEmail)
                    VALUES ($name, $contact, $email);";
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$contact", contact ?? "");
                command.Parameters.AddWithValue("$email", email ?? "");
                command.ExecuteNonQuery();

                return (int)GetLastInsertedId(connection);
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddClient Error: {ex.Message}");
                return -1;
            }
        }

        public void UpdateClient(ClientModel client)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Client SET
                        ClientName    = $name,
                        ClientContact = $contact,
                        ClientEmail   = $email
                    WHERE ClientID = $id;";
                command.Parameters.AddWithValue("$name", client.Name);
                command.Parameters.AddWithValue("$contact", client.MobileNumber ?? "");
                command.Parameters.AddWithValue("$email", client.Email ?? "");
                command.Parameters.AddWithValue("$id", client.Id);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"UpdateClient Error: {ex.Message}"); }
        }

        public void DeleteClient(int clientId)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Client WHERE ClientID = $id;";
                command.Parameters.AddWithValue("$id", clientId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"DeleteClient Error: {ex.Message}"); }
        }

        // ─────────────────────────────────────────────
        //  PROJECTS
        // ─────────────────────────────────────────────

        public List<ProjectModel> GetAllProjects()
        {
            var projects = new List<ProjectModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ProjectID, Name, Description, StartDate, EndDate, ProjectStatus FROM Project ORDER BY Name;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    projects.Add(ReadProjectFromReader(reader));
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"GetAllProjects Error: {ex.Message}"); }
            return projects;
        }
        public List<ProjectModel> FilterProjects(string searchText)
        {
            var filtered = new List<ProjectModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ProjectID, Name, Description, StartDate, EndDate, ProjectStatus 
                    FROM Project
                    WHERE Name LIKE $search OR Description LIKE $search
                    ORDER BY Name;";
                command.Parameters.AddWithValue("$search", $"%{searchText}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    filtered.Add(ReadProjectFromReader(reader));
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"FilterProjects Error: {ex.Message}"); }
            return filtered;
        }

        /// <summary>Adds a project and returns its new ProjectID.</summary>
        public int AddProject(string name, string description, string startDate, string endDate, string status)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Project (Name, Description, StartDate, EndDate, ProjectStatus)
                    VALUES ($name, $description, $startDate, $endDate, $status);";
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$description", description ?? "");
                command.Parameters.AddWithValue("$startDate", startDate ?? "");
                command.Parameters.AddWithValue("$endDate", endDate ?? "");
                command.Parameters.AddWithValue("$status", status ?? "");
                command.ExecuteNonQuery();

                return (int)GetLastInsertedId(connection);
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddProject Error: {ex.Message}");
                return -1;
            }
        }

        public void UpdateProject(ProjectModel project)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Project SET
                        Name          = $name,
                        Description   = $description,
                        StartDate     = $startDate,
                        EndDate       = $endDate,
                        ProjectStatus = $status
                    WHERE ProjectID = $id;";
                command.Parameters.AddWithValue("$name", project.Name);
                command.Parameters.AddWithValue("$description", project.Description ?? "");
                command.Parameters.AddWithValue("$startDate", project.StartDate == DateTime.MinValue ? "" : project.StartDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("$endDate", project.EndDate == DateTime.MinValue ? "" : project.EndDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("$status", project.Status ?? "");
                command.Parameters.AddWithValue("$id", project.Id);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"UpdateProject Error: {ex.Message}"); }
        }

        public void DeleteProject(int projectId)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Project WHERE ProjectID = $id;";
                command.Parameters.AddWithValue("$id", projectId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"DeleteProject Error: {ex.Message}"); }
        }

        private ProjectModel ReadProjectFromReader(SqliteDataReader reader)
        {
            return new ProjectModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                StartDate = reader.IsDBNull(3) || string.IsNullOrEmpty(reader.GetString(3))
                                ? DateTime.MinValue
                                : DateTime.Parse(reader.GetString(3)),
                EndDate = reader.IsDBNull(4) || string.IsNullOrEmpty(reader.GetString(4))
                                ? DateTime.MinValue
                                : DateTime.Parse(reader.GetString(4)),
                Status = reader.IsDBNull(5) ? "" : reader.GetString(5)
            };
        }

        // ─────────────────────────────────────────────
        //  CLIENT ↔ PROJECT LINKING
        // ─────────────────────────────────────────────

        public void LinkClientToProject(int clientId, int projectId)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                // INSERT OR IGNORE avoids duplicate-key errors if already linked
                command.CommandText = "INSERT OR IGNORE INTO ClientProject (ClientID, ProjectID) VALUES ($clientId, $projectId);";
                command.Parameters.AddWithValue("$clientId", clientId);
                command.Parameters.AddWithValue("$projectId", projectId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"LinkClientToProject Error: {ex.Message}"); }
        }

        public void UnlinkClientFromProject(int clientId, int projectId)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ClientProject WHERE ClientID = $clientId AND ProjectID = $projectId;";
                command.Parameters.AddWithValue("$clientId", clientId);
                command.Parameters.AddWithValue("$projectId", projectId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"UnlinkClientFromProject Error: {ex.Message}"); }
        }

        /// <summary>Returns all clients linked to a given project.</summary>
        public List<ClientModel> GetClientsByProject(int projectId)
        {
            var clients = new List<ClientModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT c.ClientID, c.ClientName, c.ClientContact, c.ClientEmail
                    FROM Client c
                    INNER JOIN ClientProject cp ON c.ClientID = cp.ClientID
                    WHERE cp.ProjectID = $projectId
                    ORDER BY c.ClientName;";
                command.Parameters.AddWithValue("$projectId", projectId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    clients.Add(new ClientModel
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        MobileNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"GetClientsByProject Error: {ex.Message}"); }
            return clients;
        }

        /// <summary>Returns all projects linked to a given client.</summary>
        public List<ProjectModel> GetProjectsByClient(int clientId)
        {
            var projects = new List<ProjectModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT p.ProjectID, p.Name, p.Description, p.StartDate, p.EndDate, p.ProjectStatus
                    FROM Project p
                    INNER JOIN ClientProject cp ON p.ProjectID = cp.ProjectID
                    WHERE cp.ClientID = $clientId
                    ORDER BY p.Name;";
                command.Parameters.AddWithValue("$clientId", clientId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    projects.Add(ReadProjectFromReader(reader));
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"GetProjectsByClient Error: {ex.Message}"); }
            return projects;
        }

        // ─────────────────────────────────────────────
        //  MILESTONES
        // ─────────────────────────────────────────────

        public List<MilestoneModel> GetMilestonesByProject(int projectId)
        {
            var milestones = new List<MilestoneModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT MilestoneID, MilestoneDescription, Price, MilestoneDeadline, ProjectID
                    FROM Milestone
                    WHERE ProjectID = $projectId
                    ORDER BY MilestoneID;";
                command.Parameters.AddWithValue("$projectId", projectId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    milestones.Add(new MilestoneModel
                    {
                        Id = reader.GetInt32(0),
                        Description = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Price = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                        Deadline = reader.IsDBNull(3) || string.IsNullOrEmpty(reader.GetString(3))
                                        ? DateTime.MinValue
                                        : DateTime.Parse(reader.GetString(3)),
                        ProjectId = reader.GetInt32(4)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"GetMilestonesByProject Error: {ex.Message}"); }
            return milestones;
        }

        /// <summary>Adds a milestone and returns its new MilestoneID.</summary>
        public int AddMilestone(int projectId, string description, double price, string deadline)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Milestone (ProjectID, MilestoneDescription, Price, MilestoneDeadline)
                    VALUES ($projectId, $description, $price, $deadline);";
                command.Parameters.AddWithValue("$projectId", projectId);
                command.Parameters.AddWithValue("$description", description ?? "");
                command.Parameters.AddWithValue("$price", price);
                command.Parameters.AddWithValue("$deadline", deadline ?? "");
                command.ExecuteNonQuery();

                return (int)GetLastInsertedId(connection);
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddMilestone Error: {ex.Message}");
                return -1;
            }
        }

        public void UpdateMilestone(MilestoneModel milestone)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Milestone SET
                        MilestoneDescription = $description,
                        Price                = $price,
                        MilestoneDeadline    = $deadline
                    WHERE MilestoneID = $id;";
                command.Parameters.AddWithValue("$description", milestone.Description ?? "");
                command.Parameters.AddWithValue("$price", milestone.Price);
                command.Parameters.AddWithValue("$deadline", milestone.Deadline == DateTime.MinValue ? "" : milestone.Deadline.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("$id", milestone.Id);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"UpdateMilestone Error: {ex.Message}"); }
        }

        public void DeleteMilestone(int milestoneId)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Milestone WHERE MilestoneID = $id;";
                command.Parameters.AddWithValue("$id", milestoneId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"DeleteMilestone Error: {ex.Message}"); }
        }

        // ─────────────────────────────────────────────
        //  TASKS
        // ─────────────────────────────────────────────

        public List<TaskModel> GetTasksByMilestone(int milestoneId)
        {
            var tasks = new List<TaskModel>();
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TaskID, TaskName, TaskDescription, TaskDeadline, IsCompleted, MilestoneID
                    FROM Task
                    WHERE MilestoneID = $milestoneId
                    ORDER BY TaskID;";
                command.Parameters.AddWithValue("$milestoneId", milestoneId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(new TaskModel
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Deadline = reader.IsDBNull(3) || string.IsNullOrEmpty(reader.GetString(3))
                                        ? DateTime.MinValue
                                        : DateTime.Parse(reader.GetString(3)),
                        IsCompleted = reader.GetInt32(4) == 1,
                        MilestoneId = reader.GetInt32(5)
                    });
                }
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"GetTasksByMilestone Error: {ex.Message}"); }
            return tasks;
        }

        /// <summary>Adds a task and returns its new TaskID.</summary>
        public int AddTask(int milestoneId, string name, string description, string deadline)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Task (MilestoneID, TaskName, TaskDescription, TaskDeadline, IsCompleted)
                    VALUES ($milestoneId, $name, $description, $deadline, 0);";
                command.Parameters.AddWithValue("$milestoneId", milestoneId);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$description", description ?? "");
                command.Parameters.AddWithValue("$deadline", deadline ?? "");
                command.ExecuteNonQuery();

                return (int)GetLastInsertedId(connection);
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTask Error: {ex.Message}");
                return -1;
            }
        }

        public void UpdateTask(TaskModel task)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Task SET
                        TaskName        = $name,
                        TaskDescription = $description,
                        TaskDeadline    = $deadline,
                        IsCompleted     = $isCompleted
                    WHERE TaskID = $id;";
                command.Parameters.AddWithValue("$name", task.Name);
                command.Parameters.AddWithValue("$description", task.Description ?? "");
                command.Parameters.AddWithValue("$deadline", task.Deadline == DateTime.MinValue ? "" : task.Deadline.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("$isCompleted", task.IsCompleted ? 1 : 0);
                command.Parameters.AddWithValue("$id", task.Id);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"UpdateTask Error: {ex.Message}"); }
        }

        public void SetTaskCompleted(int taskId, bool isCompleted)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Task SET IsCompleted = $val WHERE TaskID = $id;";
                command.Parameters.AddWithValue("$val", isCompleted ? 1 : 0);
                command.Parameters.AddWithValue("$id", taskId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"SetTaskCompleted Error: {ex.Message}"); }
        }

        public void DeleteTask(int taskId)
        {
            try
            {
                using var connection = OpenConnection();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Task WHERE TaskID = $id;";
                command.Parameters.AddWithValue("$id", taskId);
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) { System.Diagnostics.Debug.WriteLine($"DeleteTask Error: {ex.Message}"); }
        }
    }
}