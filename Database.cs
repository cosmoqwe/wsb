using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using ConsoleApp2;

namespace WorkDB
{
        public delegate void DatabaseAction(string message);
        public delegate void DatabaseError(string error);
        public interface IDatabaseOperations
        {
            event DatabaseAction OnSuccess;
            event DatabaseError OnError;
            bool SomeoneAuthorized(ref int access_level);
            void Reg(string username, string password);
            void Auth(string username, string password);
            void RenameColumn(string tableName, string oldColumnName, string newColumnName);
            List<string> ShowTables();
            void DeleteRecord(string tableName, int id);
            void ShowTableData(string tableName);
            void CreateNewTable(string tableName);
            void AddColumnToTable(string tableName, string columnName, string dataType, int? charLength = null);
            void UpdateTableData(string tableName, string columnName, string newValue, int id);
            void InsertIntoTable(string tableName, Dictionary<string, string> columnValues);
            void ShowTableDataWithCondition(string tableName, string columnName, string condition);
            List<string> GetTableColumns(string tableName);
            void UpdateSpecificColumnData(string tableName, string columnName, string newValue, string conditionColumn, string conditionValue);
            void DeleteColumnFromTable(string tableName, string columnName);

        }
    

    public class Database : IDatabaseOperations
    {
        public event DatabaseAction OnSuccess;
        public event DatabaseError OnError;
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDB"].ConnectionString;
        public bool SomeoneAuthorized(ref int access_level)
        {
            if (ConfigurationManager.AppSettings["Username"] != null && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Username"].ToString()))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM Auth";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (Equals(ConfigurationManager.AppSettings["Username"], reader["Username"].ToString()))
                                    {
                                        access_level = reader.GetInt32(reader.GetOrdinal("access_level"));
                                        return true;
                                    }
                                    else access_level = 0;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex.Message);
                    }
                }
            }
            access_level = 0;
            return false;
        }
        public void Reg(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    bool IsCreated(string localUsername)
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM Auth";
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (string.Equals(localUsername.ToLower().Trim(), reader["Username"].ToString().Trim()))
                                    {
                                        OnError?.Invoke("User is already registered!");
                                        return true;
                                    }
                                }
                            }
                        }
                        return false;
                    }
                    if (!IsCreated(username))
                    {
                        var salt = Hash.Hash.GenerateSalt();
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "INSERT INTO Auth (username, password, salt) VALUES (@value1, @value2, @value3)";
                            command.Parameters.AddWithValue("@value1", username.ToLower().Trim());
                            command.Parameters.AddWithValue("@value2", Hash.Hash.HashPassword(password.Trim(), salt));
                            command.Parameters.AddWithValue("@value3", salt.Trim());
                            command.ExecuteNonQuery();
                            OnSuccess?.Invoke("User was added to database!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void Auth(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM Auth";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if(Equals(username.ToLower().Trim(), reader["Username"]))
                                {
                                    string salt = reader["salt"].ToString();
                                    //Console.WriteLine($"{reader["Password"].ToString()}" + Hash.Hash.HashPassword(password, salt));
                                    if(Equals(Hash.Hash.HashPassword(password,salt), reader["Password"]))
                                    {
                                        SaveToConfig.Save(username, Hash.Hash.HashPassword(password, salt));
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"EXEC sp_rename '{tableName}.{oldColumnName}', '{newColumnName}', 'COLUMN'";
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Rename successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }


        #region Commented Code
        /*public void Unlogin()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE Auth SET IsLogged = 0 WHERE IsLogged = 1 ";
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Log out successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void Auth(string username, string password, ref int access_level)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    bool shouldUpdate = false;

                    using (SqlCommand command = new SqlCommand("SELECT * FROM Auth", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (string.Equals(username.ToLower().Trim(), reader["Username"].ToString().Trim()) &&
                                    string.Equals(password.Trim(), reader["Password"].ToString().Trim()))
                                {
                                    shouldUpdate = true;
                                    access_level = reader.GetInt32(reader.GetOrdinal("access_level"));
                                    break;
                                }
                            }
                        }
                    }

                    if (shouldUpdate)
                    {
                        using (SqlCommand updateCommand = new SqlCommand("UPDATE Auth SET IsLogged = 1 WHERE Username = @username", connection))
                        {
                            updateCommand.Parameters.AddWithValue("@username", username);
                            updateCommand.ExecuteNonQuery();
                            OnSuccess?.Invoke($"User {username} successfully authenticated.");
                        }
                    }
                    else OnError?.Invoke("Invalid username or password!");
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Authentication error: {ex.Message}");
                }
            }
        }
        public void ShowTableData(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tableName} ORDER BY id";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.Write($"{reader.GetName(i),-16}");
                                }
                                Console.WriteLine();

                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        Console.Write($"{reader[i].ToString().Trim(),-16}");
                                    }
                                    Console.WriteLine();
                                }
                            }
                            else
                            {
                                Console.WriteLine($"No data found in table {tableName}.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        */
        #endregion
        public List<string> ShowTables()
        {
            List<string> tables = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!string.Equals(reader["TABLE_NAME"].ToString(), "Auth"))
                                {
                                    tables.Add(reader["TABLE_NAME"].ToString());

                                }
                            }
                            foreach (string table in tables)
                            {
                                Console.WriteLine(table);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
            return tables;
        }
       /* */
        public void ShowTableData(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tableName} ORDER BY id";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine($"No data found in table {tableName}.");
                                return;
                            }

                            int[] columnWidths = new int[reader.FieldCount];
                            List<string[]> rows = new List<string[]>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                columnWidths[i] = reader.GetName(i).Length;
                            }

                            while (reader.Read())
                            {
                                string[] currentRow = new string[reader.FieldCount];
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    currentRow[i] = reader[i].ToString().Trim();
                                    columnWidths[i] = Math.Max(columnWidths[i], currentRow[i].Length);
                                }
                                rows.Add(currentRow);
                            }

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i).PadRight(columnWidths[i] + 2)}");
                            }
                            Console.WriteLine();

                            foreach (var row in rows)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.Write($"{row[i].PadRight(columnWidths[i] + 2)}");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }


        public void CreateNewTable(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"CREATE TABLE {tableName} (id INT PRIMARY KEY IDENTITY(1,1))";
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Creating successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void AddColumnToTable(string tableName, string columnName, string dataType, int? charLength = null)
        {
            string columnDefinition = dataType;
            if (dataType.ToLower() == "varchar" || dataType.ToLower() == "nvarchar" && charLength.HasValue)
            {
                columnDefinition += $"({charLength.Value})";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"ALTER TABLE {tableName} ADD {columnName} {columnDefinition}";
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Adding successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void UpdateTableData(string tableName, string columnName, string newValue, int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE {tableName} SET {columnName} = @newValue WHERE id = @id";
                        command.Parameters.AddWithValue("@newValue", newValue);
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Updating successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void InsertIntoTable(string tableName, Dictionary<string, string> columnValues)
        {
            string columns = string.Join(", ", columnValues.Keys);
            string values = string.Join(", ", columnValues.Values.Select(v => $"'{v}'"));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Inserting successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void ShowTableDataWithCondition(string tableName, string columnName, string condition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = @condition ORDER BY id";
                        command.Parameters.AddWithValue("@condition", condition);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine($"No data found in table {tableName}.");
                                return;
                            }

                            int[] columnWidths = new int[reader.FieldCount];
                            List<string[]> rows = new List<string[]>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                columnWidths[i] = reader.GetName(i).Length;
                            }

                            while (reader.Read())
                            {
                                string[] currentRow = new string[reader.FieldCount];
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    currentRow[i] = reader[i].ToString().Trim();
                                    columnWidths[i] = Math.Max(columnWidths[i], currentRow[i].Length);
                                }
                                rows.Add(currentRow);
                            }

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i).PadRight(columnWidths[i] + 2)}");
                            }
                            Console.WriteLine();

                            foreach (var row in rows)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.Write($"{row[i].PadRight(columnWidths[i] + 2)}");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }

        public List<string> GetTableColumns(string tableName)
        {
            List<string> columns = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!String.Equals(reader["COLUMN_NAME"].ToString().ToLower(), "id"))
                                    columns.Add(reader["COLUMN_NAME"].ToString());
                            }
                        }
                    }
                    foreach(string column in columns)
                    {
                        Console.Write(column + " ");
                    }
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
            return columns;
        }
        public void DeleteRecord(string tablename, int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"DELETE FROM {tablename} WHERE id = @id";
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteReader();
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void UpdateSpecificColumnData(string tableName, string columnName, string newValue, string conditionColumn, string conditionValue)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE {tableName} SET {columnName} = @newValue WHERE {conditionColumn} = @conditionValue";
                        command.Parameters.AddWithValue("@newValue", newValue);
                        command.Parameters.AddWithValue("@conditionValue", conditionValue);
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Updating successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
        public void DeleteColumnFromTable(string tableName, string columnName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"ALTER TABLE {tableName} DROP COLUMN {columnName}";
                        command.ExecuteNonQuery();
                        OnSuccess?.Invoke("Deleting successful!");
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex.Message);
                }
            }
        }
    }
}
