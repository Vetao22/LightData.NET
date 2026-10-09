using System;
using System.Collections;
using System.Collections.Generic;

namespace LightData.NET
{
    /// <summary>
    /// Strongly-typed column structure containing row values.
    /// </summary>
    public class DataColumn : IEnumerable<DataValue>
    {
        public string Name { get; set; }
        private readonly List<DataValue> _values;

        public int Count => _values.Count;

        public DataValue this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        public DataColumn(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _values = new List<DataValue>();
        }

        public DataColumn(string name, IEnumerable<DataValue> values)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _values = new List<DataValue>(values);
        }

        public void Add(DataValue value) => _values.Add(value);

        public void AddRange(IEnumerable<DataValue> values) => _values.AddRange(values);

        public DataColumn Clone() => new DataColumn(Name, _values);

        public IEnumerator<DataValue> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}