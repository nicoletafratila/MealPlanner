namespace Common.Pagination
{
    public class FilterItem
    {
        public string PropertyName { get; init; } = string.Empty;
        public object? Value { get; init; }
        public FilterOperator Operator { get; init; }
        public StringComparison StringComparison { get; init; } = StringComparison.Ordinal;

        public FilterItem() { }

        public FilterItem(
            string propertyName,
            object? value,
            FilterOperator @operator,
            StringComparison stringComparison = StringComparison.Ordinal)
        {
            PropertyName = propertyName;
            Value = value;
            Operator = @operator;
            StringComparison = stringComparison;
        }
    }
}
