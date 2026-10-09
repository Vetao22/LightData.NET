using System;
using System.Collections.Generic;
using System.Linq;

namespace LightData.NET
{
    public enum JoinType
    {
        Inner,
        Left
    }

    /// <summary>
    /// Primary in-memory tabular data structure with query, filter, aggregation, sorting, and join capabilities.
    /// </summary>
    public class DataFrame
    {
        private readonly List<DataColumn> _columns;
        private readonly Dictionary<string, int> _columnIndexMap;

        public int ColumnCount => _columns.Count;
        public int RowCount => _columns.Count == 0 ? 0 : _columns[0].Count;
        public IReadOnlyList<string> ColumnNames => _columns.Select(c => c.Name).ToList();

        public DataColumn this[string columnName]
        {
            get
            {
                if (!_columnIndexMap.TryGetValue(columnName, out int index))
                    throw new KeyNotFoundException($"Column '{columnName}' does not exist in DataFrame.");
                return _columns[index];
            }
        }

        public DataColumn this[int columnIndex] => _columns[columnIndex];

        public DataRow this[int rowIndex] => new DataRow(this, rowIndex);

        public DataFrame()
        {
            _columns = new List<DataColumn>();
            _columnIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddColumn(DataColumn column)
        {
            if (_columnIndexMap.ContainsKey(column.Name))
                throw new ArgumentException($"Column '{column.Name}' already exists.");

            _columns.Add(column);
            _columnIndexMap[column.Name] = _columns.Count - 1;
        }

        #region Operations: Filtering, Selecting, Sorting

        public DataFrame Select(params string[] columnNames)
        {
            var result = new DataFrame();
            foreach (var name in columnNames)
            {
                result.AddColumn(this[name].Clone());
            }
            return result;
        }

        public DataFrame Filter(Func<DataRow, bool> predicate)
        {
            var result = new DataFrame();
            foreach (var col in _columns)
            {
                result.AddColumn(new DataColumn(col.Name));
            }

            for (int r = 0; r < RowCount; r++)
            {
                var row = new DataRow(this, r);
                if (predicate(row))
                {
                    for (int c = 0; c < ColumnCount; c++)
                    {
                        result[c].Add(this[c][r]);
                    }
                }
            }

            return result;
        }

        public DataFrame SortBy(string columnName, bool ascending = true)
        {
            int colIdx = _columnIndexMap[columnName];
            var rowIndices = Enumerable.Range(0, RowCount).ToList();

            rowIndices.Sort((a, b) =>
            {
                int cmp = _columns[colIdx][a].CompareTo(_columns[colIdx][b]);
                return ascending ? cmp : -cmp;
            });

            var result = new DataFrame();
            foreach (var col in _columns)
            {
                var newCol = new DataColumn(col.Name);
                foreach (int idx in rowIndices)
                {
                    newCol.Add(col[idx]);
                }
                result.AddColumn(newCol);
            }

            return result;
        }

        #endregion

        #region Aggregations & GroupBy

        public double Sum(string columnName) => this[columnName].Select(v => v.AsDouble()).Sum();
        public double Mean(string columnName) => this[columnName].Select(v => v.AsDouble()).Average();
        public double Min(string columnName) => this[columnName].Select(v => v.AsDouble()).Min();
        public double Max(string columnName) => this[columnName].Select(v => v.AsDouble()).Max();

        public Dictionary<DataValue, DataFrame> GroupBy(string columnName)
        {
            var groups = new Dictionary<DataValue, List<int>>();
            var col = this[columnName];

            for (int r = 0; r < RowCount; r++)
            {
                var val = col[r];
                if (!groups.TryGetValue(val, out var list))
                {
                    list = new List<int>();
                    groups[val] = list;
                }
                list.Add(r);
            }

            var result = new Dictionary<DataValue, DataFrame>();
            foreach (var kvp in groups)
            {
                var dfGroup = new DataFrame();
                foreach (var c in _columns)
                {
                    dfGroup.AddColumn(new DataColumn(c.Name));
                }

                foreach (int r in kvp.Value)
                {
                    for (int c = 0; c < ColumnCount; c++)
                    {
                        dfGroup[c].Add(this[c][r]);
                    }
                }

                result[kvp.Key] = dfGroup;
            }

            return result;
        }

        #endregion

        #region Relational Joins

        public DataFrame Join(DataFrame right, string leftKey, string rightKey, JoinType joinType = JoinType.Inner)
        {
            var result = new DataFrame();

            // Prepare columns for new DataFrame
            foreach (var col in _columns)
            {
                result.AddColumn(new DataColumn(col.Name));
            }

            foreach (var col in right._columns)
            {
                string colName = col.Name;
                if (result._columnIndexMap.ContainsKey(colName))
                    colName = $"{colName}_right";

                result.AddColumn(new DataColumn(colName));
            }

            // Build index lookup table for right table
            var rightIndex = new Dictionary<DataValue, List<int>>();
            var rightKeyCol = right[rightKey];

            for (int r = 0; r < right.RowCount; r++)
            {
                var val = rightKeyCol[r];
                if (!rightIndex.TryGetValue(val, out var list))
                {
                    list = new List<int>();
                    rightIndex[val] = list;
                }
                list.Add(r);
            }

            // Perform Hash Join
            var leftKeyCol = this[leftKey];
            for (int rLeft = 0; rLeft < RowCount; rLeft++)
            {
                var keyVal = leftKeyCol[rLeft];

                if (rightIndex.TryGetValue(keyVal, out var rightRows))
                {
                    foreach (int rRight in rightRows)
                    {
                        AddJoinedRow(result, this, rLeft, right, rRight);
                    }
                }
                else if (joinType == JoinType.Left)
                {
                    AddJoinedRow(result, this, rLeft, right, -1);
                }
            }

            return result;
        }

        private static void AddJoinedRow(DataFrame target, DataFrame left, int rLeft, DataFrame right, int rRight)
        {
            int cIndex = 0;

            for (int c = 0; c < left.ColumnCount; c++)
            {
                target[cIndex++].Add(left[c][rLeft]);
            }

            for (int c = 0; c < right.ColumnCount; c++)
            {
                target[cIndex++].Add(rRight >= 0 ? right[c][rRight] : DataValue.Null);
            }
        }

        #endregion
    }
}