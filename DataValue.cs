using System;
using System.Globalization;

namespace LightData.NET
{
    public enum DataType
    {
        Null,
        Boolean,
        Int64,
        Double,
        DateTime,
        String
    }

    /// <summary>
    /// Lightweight, high-performance value wrapper for heterogeneous data frame cells.
    /// Avoids excessive boxing and supports safe conversions.
    /// </summary>
    public readonly struct DataValue : IComparable<DataValue>, IEquatable<DataValue>
    {
        public DataType Type { get; }
        private readonly long _integerValue;
        private readonly double _doubleValue;
        private readonly bool _boolValue;
        private readonly DateTime _dateTimeValue;
        private readonly string _stringValue;

        public static DataValue Null { get; } = new DataValue();

        public bool IsNull => Type == DataType.Null;

        public DataValue()
        {
            Type = DataType.Null;
            _integerValue = 0;
            _doubleValue = 0;
            _boolValue = false;
            _dateTimeValue = default;
            _stringValue = null;
        }

        public DataValue(long value) : this()
        {
            Type = DataType.Int64;
            _integerValue = value;
        }

        public DataValue(double value) : this()
        {
            Type = DataType.Double;
            _doubleValue = value;
        }

        public DataValue(bool value) : this()
        {
            Type = DataType.Boolean;
            _boolValue = value;
        }

        public DataValue(DateTime value) : this()
        {
            Type = DataType.DateTime;
            _dateTimeValue = value;
        }

        public DataValue(string value) : this()
        {
            if (value == null)
            {
                Type = DataType.Null;
            }
            else
            {
                Type = DataType.String;
                _stringValue = value;
            }
        }

        public static DataValue Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Null;

            input = input.Trim();

            if (bool.TryParse(input, out bool boolRes))
                return new DataValue(boolRes);

            if (long.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out long intRes))
                return new DataValue(intRes);

            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleRes))
                return new DataValue(doubleRes);

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtRes))
                return new DataValue(dtRes);

            return new DataValue(input);
        }

        public double AsDouble()
        {
            return Type switch
            {
                DataType.Double => _doubleValue,
                DataType.Int64 => _integerValue,
                DataType.Boolean => _boolValue ? 1.0 : 0.0,
                DataType.String => double.TryParse(_stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double res) ? res : 0.0,
                _ => 0.0
            };
        }

        public long AsInt64()
        {
            return Type switch
            {
                DataType.Int64 => _integerValue,
                DataType.Double => (long)_doubleValue,
                DataType.Boolean => _boolValue ? 1L : 0L,
                DataType.String => long.TryParse(_stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long res) ? res : 0L,
                _ => 0L
            };
        }

        public bool AsBoolean()
        {
            return Type switch
            {
                DataType.Boolean => _boolValue,
                DataType.Int64 => _integerValue != 0,
                DataType.Double => Math.Abs(_doubleValue) > double.Epsilon,
                DataType.String => bool.TryParse(_stringValue, out bool res) && res,
                _ => false
            };
        }

        public DateTime AsDateTime() => Type == DataType.DateTime ? _dateTimeValue : default;

        public override string ToString()
        {
            return Type switch
            {
                DataType.Null => "",
                DataType.Boolean => _boolValue.ToString(),
                DataType.Int64 => _integerValue.ToString(CultureInfo.InvariantCulture),
                DataType.Double => _doubleValue.ToString("G15", CultureInfo.InvariantCulture),
                DataType.DateTime => _dateTimeValue.ToString("o", CultureInfo.InvariantCulture),
                DataType.String => _stringValue ?? "",
                _ => ""
            };
        }

        public int CompareTo(DataValue other)
        {
            if (IsNull && other.IsNull) return 0;
            if (IsNull) return -1;
            if (other.IsNull) return 1;

            if (Type == DataType.Int64 || Type == DataType.Double || other.Type == DataType.Int64 || other.Type == DataType.Double)
                return AsDouble().CompareTo(other.AsDouble());

            if (Type == DataType.DateTime && other.Type == DataType.DateTime)
                return _dateTimeValue.CompareTo(other._dateTimeValue);

            return string.Compare(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(DataValue other) => CompareTo(other) == 0;

        public override bool Equals(object obj) => obj is DataValue other && Equals(other);

        public override int GetHashCode()
        {
            return Type switch
            {
                DataType.Null => 0,
                DataType.Int64 => _integerValue.GetHashCode(),
                DataType.Double => _doubleValue.GetHashCode(),
                DataType.Boolean => _boolValue.GetHashCode(),
                DataType.DateTime => _dateTimeValue.GetHashCode(),
                DataType.String => _stringValue?.GetHashCode() ?? 0,
                _ => 0
            };
        }
    }
}