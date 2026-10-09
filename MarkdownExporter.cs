using System.Text;

namespace LightData.NET
{
    /// <summary>
    /// Utility to format and export DataFrames into clean Markdown tables.
    /// </summary>
    public static class MarkdownExporter
    {
        public static string ToMarkdownTable(this DataFrame df, int maxRows = 100)
        {
            if (df == null || df.ColumnCount == 0)
                return "*Empty DataFrame*";

            var sb = new StringBuilder();
            int[] columnWidths = new int[df.ColumnCount];

            // Calculate width for header and contents
            for (int i = 0; i < df.ColumnCount; i++)
            {
                columnWidths[i] = df.ColumnNames[i].Length;
                int rowsToInspect = System.Math.Min(df.RowCount, maxRows);
                for (int r = 0; r < rowsToInspect; r++)
                {
                    int valLen = df[i][r].ToString().Length;
                    if (valLen > columnWidths[i])
                        columnWidths[i] = valLen;
                }
            }

            // Header Line
            sb.Append("| ");
            for (int i = 0; i < df.ColumnCount; i++)
            {
                sb.Append(df.ColumnNames[i].PadRight(columnWidths[i]));
                sb.Append(" | ");
            }
            sb.AppendLine();

            // Separator Line
            sb.Append("| ");
            for (int i = 0; i < df.ColumnCount; i++)
            {
                sb.Append(new string('-', columnWidths[i]));
                sb.Append(" | ");
            }
            sb.AppendLine();

            // Data Lines
            int printRows = System.Math.Min(df.RowCount, maxRows);
            for (int r = 0; r < printRows; r++)
            {
                sb.Append("| ");
                for (int c = 0; c < df.ColumnCount; c++)
                {
                    sb.Append(df[c][r].ToString().PadRight(columnWidths[c]));
                    sb.Append(" | ");
                }
                sb.AppendLine();
            }

            if (df.RowCount > maxRows)
            {
                sb.AppendLine($"\n*...Showing {maxRows} of {df.RowCount} rows.*");
            }

            return sb.ToString();
        }
    }
}