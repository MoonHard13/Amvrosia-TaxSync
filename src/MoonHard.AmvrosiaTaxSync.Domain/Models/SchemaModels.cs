namespace MoonHard.AmvrosiaTaxSync.Domain.Models;

public sealed record DatabaseInfo(string Name, string ServerVersion);

public sealed record SchemaColumn(
    string Name,
    string DataType,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsForeignKey);

public sealed record SchemaObject(
    string Schema,
    string Name,
    string Kind,
    IReadOnlyList<SchemaColumn> Columns)
{
    public string QualifiedName => $"{Schema}.{Name}";
    public int ColumnCount => Columns.Count;
}
