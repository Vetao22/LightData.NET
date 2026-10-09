using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LightData.NET
{
    /// <summary>
    /// Zero-dependency, memory-efficient RFC 4180 compliant CSV parser.
    /// Supports quoted values, embedded commas, and line breaks.
    /// </summary>
    public static class CsvParser
    {
        public static DataFrame ReadCsv(string filePath, char delimiter = ',', bool hasHeader = true)
        {
            using var stream = File.OpenRead(filePath);
            return ReadCsv(stream, delimiter, hasHeader);
        }

        public static DataFrame ReadCsv(Stream stream, char delimiter = ',', bool hasHeader = true)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var records = ParseStream(reader, delimiter);

            if (records.Count == 0)
                return new DataFrame();

            var df = new DataFrame();
            int startRow = 0;

            // Header evaluation
            if (hasHeader)
            {
                var headers = records[0];
                for (int i = 0; i < headers.Count; i++)
                {
                    string colName = string.IsNullOrWhiteSpace(headers[i]) ? $"Column_{i}" : headers[i].Trim();
                    df.AddColumn(new DataColumn(colName));
                }
                startRow = 1;
            }
            else
            {
                for (int i = 0; i < records[0].Count; i++)
                {
                    df.AddColumn(new DataColumn($"Column_{i}"));
                }
            }

            // Populate rows and infer data types automatically
            for (int r = startRow; r < records.Count; r++)
            {
                var rowData = records[r];
                for (int c = 0; c < df.ColumnCount; c++)
                {
                    string valStr = c < rowData.Count ? rowData[c] : null;
                    df[c].Add(DataValue.Parse(valStr));
                }
            }

            return df;
        }

        public static void WriteCsv(DataFrame df, string filePath, char delimiter = ',')
        {
            using var stream = File.Create(filePath);
            WriteCsv(df, stream, delimiter);
        }

        public static void WriteCsv(DataFrame df, Stream stream, char delimiter = ',')
        {
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            
            // Write Headers
            for (int i = 0; i < df.ColumnCount; i++)
            {
                writer.Write(EscapeCsvField(df.ColumnNames[i], delimiter));
                if (i < df.ColumnCount - 1) writer.Write(delimiter);
            }
            writer.WriteLine();

            // Write Data
            for (int r = 0; r < df.RowCount; r++)
            {
                for (int c = 0; c < df.ColumnCount; c++)
                {
                    writer.Write(EscapeCsvField(df[c][r].ToString(), delimiter));
                    if (c < df.ColumnCount - 1) writer.Write(delimiter);
                }
                writer.WriteLine();
            }
        }

        private static List<List<string>> ParseStream(TextReader reader, char delimiter)
        {
            var records = new List<List<string>>();
            var currentRecord = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            int ch;
            while ((ch = reader.Read()) != -1)
            {
                char c = (char)ch;

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (reader.Peek() == '"') // Escaped Quote
                        {
                            currentField.Append('"');
                            reader.Read();
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == delimiter)
                    {
                        currentRecord.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else if (c == '\r')
                    {
                        if (reader.Peek() == '\n') reader.Read();
                        currentRecord.Add(currentField.ToString());
                        currentField.Clear();
                        records.Add(currentRecord);
                        currentRecord = new List<string>();
                    }
                    else if (c == '\n')
                    {
                        currentRecord.Add(currentField.ToString());
                        currentField.Clear();
                        records.Add(currentRecord);
                        currentRecord = new List<string>();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            if (currentField.Length > 0 || currentRecord.Count > 0)
            {
                currentRecord.Add(currentField.ToString());
                records.Add(currentRecord);
            }

            return records;
        }

        private static string EscapeCsvField(string field, char delimiter)
        {
            if (field == null) return "";
            if (field.Contains(delimiter) || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}