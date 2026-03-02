namespace Shared.Contracts.Reporting;

public class ImportColumnMapping
{
    public int ColumnIndex { get; set; } // 0-based Excel column index
    public string PropertyName { get; set; } = string.Empty;
    public bool Required { get; set; } = true;
    public Type? PropertyType { get; set; }
    public string? Format { get; set; } // For date parsing
    public string? DisplayName { get; set; } // For error messages
}

public class ImportResult<T>
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int TotalRows { get; set; }
    public List<T> ImportedData { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? RawValue { get; set; }
}
