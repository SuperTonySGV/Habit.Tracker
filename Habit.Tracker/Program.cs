using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HabitTracker
{
    class Program
    {
        static string connectionString = @"Data Source=habit-Tracker-2.db";

        static void Main(string[] args)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                //tableCmd.CommandText =
                //    @"CREATE TABLE IF NOT EXISTS habit (
                //        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                //        Name TEXT,
                //        Date TEXT,
                //        UOM TEXT,
                //        Quantity INTEGER
                //        )";

                tableCmd.CommandText +=
                    @"INSERT INTO 'habit' ('name', 'date', 'uom', 'quantity') VALUES
                      ('Water', '01-01-20','Ounces', '60'),
                      ('Meditating', '01-01-21','Minutes', '30'),
                      ('Healthy meals', '01-01-22','Count', '3'),
                      ('Reading', '01-01-23','Pages', '22')";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            GetUserInput();
        }

        static void GetUserInput()
        {
            Console.Clear();
            bool closeApp = false;
            while (closeApp == false)
            {
                Console.WriteLine("\n\nMAIN MENU");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("\nPress 0 to close the application");
                Console.WriteLine("\nPress 1 to view all records");
                Console.WriteLine("\nPress 2 to add a new record");
                Console.WriteLine("\nPress 3 to delete a record");
                Console.WriteLine("\nPress 4 to update a record");
                Console.WriteLine("-----------------------------------------------\n");

                string command = Console.ReadLine();

                switch(command)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye!\n");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("\nInvalid command. Please press a number from 0 to 4.");
                        break;
                }
            }
        }

        private static void GetAllRecords()
        {
            //var choice = GetNumberInput("\n\nType 1 for all records or 2 for a more advanced search. Type 0 to return to the main menu. \n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText =
                    $"SELECT * FROM habit";

                //if (choice == 1)
                //{
                //    tableCmd.CommandText =
                //        $"SELECT * FROM habit";
                //} else if (choice == 2)
                //{
                //    Console.WriteLine("\n\nWhat is the name of the column? Choose from Name, Date, UOM, or Quantity");
                //    var columnName = Console.ReadLine();
                //    Console.WriteLine("\n\nWhat is the value of the column?");
                //    var columnValue = Console.ReadLine();
                //    tableCmd.CommandText =
                //        $"SELECT * FROM habit WHERE Name = '{columnName}'";
                //}


                List<Habit> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(new Habit()
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Date = DateTime.ParseExact(reader.GetString(2), "dd-MM-yy", new CultureInfo("en-US")),
                            UnitOfMeasurement = reader.GetString(3),
                            Quantity = reader.GetInt32(4)
                        });
                    }
                } else
                {
                    Console.WriteLine("No rows found.");
                }

                connection.Close();

                Console.WriteLine("--------------------------\n");
                foreach (var dw in tableData)
                {
                    Console.WriteLine($"({dw.Id} - {dw.Date.ToString("dd-MMM-yyyy")}) - {dw.Name} - Unit of measurement: {dw.UnitOfMeasurement} - Quantity: {dw.Quantity}");
                }
                Console.WriteLine("--------------------------\n");
            }
        }
        private static void Insert()
        {
            string name = GetHabitName();

            string date = GetDateInput();

            string unitOfMeasurement = GetUnitOfMeasurementInput();

            int quantity = GetNumberInput("\n\nHow many times did you complete this habit today? (no decimals allowed)\n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"INSERT INTO habit(name, date, uom, quantity) VALUES('{name}', '{date}', '{unitOfMeasurement}',{quantity})";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
        internal static void Update()
        {
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type Id of the record you would like to update. Type 0 to return to the main menu. \n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM habit WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0) {
                    Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
                    connection.Close();
                    Update();
                }

                string name = GetHabitName();

                string date = GetDateInput();

                string unitOfMeasurement = GetUnitOfMeasurementInput();

                int quantity = GetNumberInput("\n\nHow many times did you complete this habit today? (no decimals allowed)\n\n");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE habit SET name = '{name}', date = '{date}', uom = '{unitOfMeasurement}', quantity = {quantity} WHERE Id = {recordId}";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
        private static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type the Id of the record you want to delete or type 0 to go back to the Main Menu \n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"DELETE from habit WHERE Id = '{recordId}'";

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with an Id {recordId} doesn't exist. \n\n");
                    Delete();
                }
            }

            Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n\n");

            GetUserInput();
        }

        internal static string GetHabitName()
        {
            Console.WriteLine("What is the name of your habit?");

            string name = Console.ReadLine();
            if (name == "0") GetUserInput();

            while (name == null)
            {
                Console.WriteLine("Please give your habit a name.");
                name = Console.ReadLine();
            }

            return name;
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to the main menu.");

            string dateInput = Console.ReadLine();

            if (dateInput == "0") GetUserInput();

            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\n\nInvalid date. Format is dd-mm-yy. Type 0 to return to the main menu or try again. \n\n");
                dateInput = Console.ReadLine();
            }

            return dateInput;
        }

        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            if (numberInput == "0") GetUserInput();

            while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
            {
                Console.WriteLine("\n\nInvalid number. Try again. \n\n");
                numberInput = Console.ReadLine();
            }

            int finalInput = Convert.ToInt32(numberInput);

            return finalInput;
        }

        internal static string GetUnitOfMeasurementInput()
        {
            Console.WriteLine("What is the unit of measurement for this habit? Ex. glasses of water, number of miles, minutes of meditation.");

            string unitOfMeasurementInput = Console.ReadLine();
            if (unitOfMeasurementInput == "0") GetUserInput();

            while (unitOfMeasurementInput == null)
            {
                Console.WriteLine("Please give your habit a name.");
                unitOfMeasurementInput = Console.ReadLine();
            }

            return unitOfMeasurementInput;
        }
    }

    public class DrinkingWater
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
    public class Habit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public string UnitOfMeasurement {  get; set; }
    }
}