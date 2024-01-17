using System.Collections.Generic;
using System;
using WorkDB;
using ConsoleApp2;

internal class Program
{
    /*private static void OnConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Database database = new Database();
        database.Unlogin();
        Environment.Exit(0);
        Console.WriteLine("Closing...");
    }*/
    private static void Main(string[] args)
    {
        //Console.CancelKeyPress += new ConsoleCancelEventHandler(OnConsoleCancelKeyPress); TODO
        IDatabaseOperations database = new Database();
        database.OnSuccess += (message) => Console.WriteLine("Success: " + message);
        database.OnError += (message) => Console.WriteLine("Error: " + message);
        string choose = string.Empty;
        string username, password;
        int access_level = 0;
        while (true)
        {
            database.SomeoneAuthorized(ref access_level);
            switch (access_level)
            {
                case 0:

                    Console.WriteLine();
                    Console.WriteLine("1.Registration");
                    Console.WriteLine("2.Auth");
                    Console.WriteLine("0.Exit");
                    choose = Console.ReadLine();
                    Console.WriteLine();
                    switch (choose)
                    {
                        case "1":
                            Console.Write("Input username:");
                            username = Console.ReadLine();
                            Console.Write("Input Password:");
                            password = Console.ReadLine();
                            database.Reg(username.ToLower(), password);
                            break;
                        case "2":
                            Console.Write("Input username:");
                            username = Console.ReadLine();
                            Console.Write("Input Password:");
                            password = Console.ReadLine();
                            database.Auth(username, password);
                            database.SomeoneAuthorized(ref access_level);
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                    }
                    break;
                case 1:

                    Console.WriteLine();
                    Console.WriteLine("1.Show Tables");
                    //Console.WriteLine("2.Show Tables with conditions");
                    Console.WriteLine("3.Log Out");
                    Console.WriteLine("0.Exit");
                    choose = Console.ReadLine();
                    Console.WriteLine();
                    switch (choose)
                    {
                        case "1":
                            database.ShowTables();
                            Console.Write("Select a table: ");
                            string selectedTable = Console.ReadLine();
                            database.ShowTableData(selectedTable);
                            break;
                        /*case "2":
                            Console.WriteLine("Choose condiotion:");
                            Console.WriteLine();
                            Console.WriteLine("1.Equals [string, int]");
                            Console.WriteLine("2.More than [int,float,double]");
                            Console.WriteLine("3.Less than [int,float,double");
                            break;*/
                        case "3":
                            SaveToConfig.Remove();
                            database.SomeoneAuthorized(ref access_level);
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                    }

                    break;

                case 2:
                    Console.WriteLine();
                    Console.WriteLine("1. Create New Table");
                    Console.WriteLine("2. Add Column to Table");
                    Console.WriteLine("3. Show Table Data");
                    Console.WriteLine("4. Update Table Data");
                    Console.WriteLine("5. Insert New Record");
                    Console.WriteLine("6. Delete Column");
                    Console.WriteLine("7. Update Column Data");
                    Console.WriteLine("8. Rename Column");
                    Console.WriteLine("9. Show Table Data with conditions");
                    Console.WriteLine("10. Delete record from table");
                    Console.WriteLine("11. Log Out");
                    Console.WriteLine("0. Exit");
                    choose = Console.ReadLine();
                    Console.WriteLine();

                    switch (choose)
                    {
                        case "1":
                            Console.Write("Enter table name to create: ");
                            string newTableName = Console.ReadLine();
                            database.CreateNewTable(newTableName);
                            break;
                        case "2":
                            database.ShowTables();
                            Console.Write("Enter table name to add column: ");
                            string tableName = Console.ReadLine();
                            Console.Write("Enter column name: ");
                            string columnName = Console.ReadLine();
                            Console.Write("Enter data type (e.g., varchar, int): ");
                            string dataType = Console.ReadLine();
                            if (dataType.ToLower() == "varchar")
                            {
                                Console.Write("Enter character length: ");
                                int charLength = int.Parse(Console.ReadLine());
                                database.AddColumnToTable(tableName, columnName, dataType, charLength);
                            }
                            else
                            {
                                database.AddColumnToTable(tableName, columnName, dataType);
                            }
                            break;
                        case "3":
                            database.ShowTables();
                            Console.Write("Enter table name to show data: ");
                            string tableNameForData = Console.ReadLine();
                            database.ShowTableData(tableNameForData);
                            break;
                        case "4":
                            database.ShowTables();
                            Console.Write("Enter table name for updating data: ");
                            string tableNameForUpdate = Console.ReadLine();
                            database.ShowTableData(tableNameForUpdate);
                            Console.Write("Enter column name for updating data: ");
                            database.GetTableColumns(tableNameForUpdate);
                            string columnNameForUpdate = Console.ReadLine();
                            Console.Write("Enter new value: ");
                            string newValue = Console.ReadLine();
                            Console.Write("Enter the ID of the record to update: ");
                            int recordId = int.Parse(Console.ReadLine());
                            database.UpdateTableData(tableNameForUpdate, columnNameForUpdate, newValue, recordId);
                            break;
                        case "5":
                            database.ShowTables();
                            Console.Write("Enter table name for inserting new record: ");
                            string tableNameForInsert = Console.ReadLine();

                            List<string> columns = database.GetTableColumns(tableNameForInsert);
                            Dictionary<string, string> columnValues = new Dictionary<string, string>();

                            foreach (string column in columns)
                            {
                                Console.Write($"Enter value for {column}: ");
                                string value = Console.ReadLine();
                                columnValues.Add(column, value);
                            }

                            database.InsertIntoTable(tableNameForInsert, columnValues);
                            break;

                        case "6":
                            database.ShowTables();
                            Console.Write("Enter table name to delete column: ");
                            string tableNameForDeleteColumn = Console.ReadLine();
                            database.ShowTableData(tableNameForDeleteColumn);
                            Console.Write("Enter column name to delete: ");
                            string columnNameToDelete = Console.ReadLine();

                            database.DeleteColumnFromTable(tableNameForDeleteColumn, columnNameToDelete);
                            break;
                        case "7":
                            database.ShowTables();
                            Console.Write("Enter table name for updating column data: ");
                            string tableNameForUpdateColumn = Console.ReadLine();
                            database.ShowTableData(tableNameForUpdateColumn);
                            Console.Write("Enter column name to update: ");
                            string columnNameToUpdate = Console.ReadLine();
                            Console.Write("Enter new value for the column: ");
                            string newColumnValue = Console.ReadLine();
                            Console.Write("Enter the condition column name: ");
                            string conditionColumn = Console.ReadLine();
                            Console.Write("Enter the condition value: ");
                            string conditionValue = Console.ReadLine();

                            database.UpdateSpecificColumnData(tableNameForUpdateColumn, columnNameToUpdate, newColumnValue, conditionColumn, conditionValue);
                            break;
                        case "8":
                            database.ShowTables();
                            Console.Write("Enter table name for renaming column: ");
                            string tableNameForRenameColumn = Console.ReadLine();
                            database.ShowTableData(tableNameForRenameColumn);
                            Console.Write("Enter old column name: ");
                            string oldColumnName = Console.ReadLine();
                            Console.Write("Enter new column name: ");
                            string newColumnName = Console.ReadLine();

                            database.RenameColumn(tableNameForRenameColumn, oldColumnName, newColumnName);
                            break;
                        case "9":
                            database.ShowTables();
                            Console.Write("Enter table name to show data with condition: ");
                            string tableNameForCondition = Console.ReadLine();
                            database.GetTableColumns(tableNameForCondition);
                            Console.Write("Enter column name for condition: ");
                            string columnNameForCondition = Console.ReadLine();
                            Console.Write("Enter the condition value: ");
                            conditionValue = Console.ReadLine();
                            database.ShowTableDataWithCondition(tableNameForCondition, columnNameForCondition, conditionValue);
                            break;
                        case "10":
                            database.ShowTables();
                            Console.Write("Enter table name for deleting record: ");
                            string tableNameForDeleting = Console.ReadLine();
                            database.ShowTableData(tableNameForDeleting);
                            Console.WriteLine("Enter ID for deleting: ");
                            int id = int.Parse(Console.ReadLine());
                            database.DeleteRecord(tableNameForDeleting, id);
                            break;
                        case "11":
                            SaveToConfig.Remove();
                            database.SomeoneAuthorized(ref access_level);
                            break;
                        case "0":
                            Environment.Exit(0);
                            break;
                    }
                    break;
            }

        }
    }
}