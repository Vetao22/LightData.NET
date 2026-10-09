using System;
using System.Collections.Generic;

namespace LightData.NET
{
    /// <summary>
    /// Provides indexed and named access to a row within a DataFrame.
    /// </summary>
    public readonly struct DataRow
    {
        private readonly DataFrame _dataFrame;
        public int RowIndex { get; }

        public DataRow(DataFrame dataFrame, int rowIndex)
        {
            _dataFrame = dataFrame ?? throw new ArgumentNullException(nameof(dataFrame));
            RowIndex = rowIndex;
        }

        public DataValue this[string columnName] => _dataFrame[columnName][RowIndex];

        public DataValue this[int columnIndex] => _dataFrame[columnIndex][RowIndex];

        public Dictionary<string, DataValue> ToDictionary()
        {
            var dict = new Dictionary<string, DataValue>(_dataFrame.ColumnCount);
            foreach (var colName in _dataFrame.ColumnNames)
            {
                dict[colName] = this[colName];
            }
            return dict;
        }
    }
}