using System;
using System.IO;
using LightData.NET;

namespace LightDataDemo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== LightData.NET Demonstration ===\n");

            // 1. Programmatically Create a DataFrame
            var dfEmployees = new DataFrame();
            
            var colId = new DataColumn("Id");
            colId.AddRange(new DataValue[] { new(101), new(102), new(103), new(104), new(105) });
            
            var colName = new DataColumn("Name");
            colName.AddRange(new DataValue[] { new("Alice"), new("Bob"), new("Charlie"), new("Diana"), new("Eve") });
            
            var colDept = new DataColumn("Department");
            colDept.AddRange(new DataValue[] { new("IT"), new("HR"), new("IT"), new("Finance"), new("HR") });
            
            var colSalary = new DataColumn("Salary");
            colSalary.AddRange(new DataValue[] { new(8500.50), new(6200.00), new(9400.00), new(7800.00), new(6500.00) });

            dfEmployees.AddColumn(colId);
            dfEmployees.AddColumn(colName);
            dfEmployees.AddColumn(colDept);
            dfEmployees.AddColumn(colSalary);

            Console.WriteLine("--- Initial Employees DataFrame ---");
            Console.WriteLine(dfEmployees.ToMarkdownTable());

            // 2. Filtering & Sorting
            Console.WriteLine("--- Filtered (Salary > 7000) & Sorted by Salary Descending ---");
            var filteredDf = dfEmployees
                .Filter(row => row["Salary"].AsDouble() > 7000.0)
                .SortBy("Salary", ascending: false);

            Console.WriteLine(filteredDf.ToMarkdownTable());

            // 3. GroupBy Aggregations
            Console.WriteLine("--- GroupBy Department & Average Salary ---");
            var groups = dfEmployees.GroupBy("Department");
            foreach (var group in groups)
            {
                double meanSalary = group.Value.Mean("Salary");
                Console.WriteLine($"Department: {group.Key} | Employees: {group.Value.RowCount} | Avg Salary: ${meanSalary:F2}");
            }
            Console.WriteLine();

            // 4. Relational Join Example
            var dfDepartments = new DataFrame();
            var colDeptKey = new DataColumn("DeptName");
            colDeptKey.AddRange(new DataValue[] { new("IT"), new("HR"), new("Finance") });

            var colLocation = new DataColumn("Location");
            colLocation.AddRange(new DataValue[] { new("New York"), new("London"), new("Tokyo") });

            dfDepartments.AddColumn(colDeptKey);
            dfDepartments.AddColumn(colLocation);

            Console.WriteLine("--- Joined Employee and Department DataFrames ---");
            var joinedDf = dfEmployees.Join(dfDepartments, leftKey: "Department", rightKey: "DeptName", joinType: JoinType.Inner);
            Console.WriteLine(joinedDf.ToMarkdownTable());

            // 5. CSV Reader & Writer Demo
            string csvPath = "sample_data.csv";
            CsvParser.WriteCsv(dfEmployees, csvPath);
            Console.WriteLine($"[CSV] Successfully exported DataFrame to '{csvPath}'.");

            var loadedFromCsv = CsvParser.ReadCsv(csvPath);
            Console.WriteLine("\n--- DataFrame Re-Loaded from CSV File ---");
            Console.WriteLine(loadedFromCsv.ToMarkdownTable());

            // Cleanup CSV file
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
    }
}