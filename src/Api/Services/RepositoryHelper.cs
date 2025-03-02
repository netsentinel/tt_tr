namespace Api.Services;

public static class RepositoryHelper
{
    public static string GenerateInsertQuery<T>(bool withReturningId = false, string returningColumn = "Id")
    {
        var type = typeof(T);

        var tableName = type.Name + "s";

        var properties = type.GetProperties()
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var paramNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var returningStatement = withReturningId ? $"RETURNING {returningColumn}" : "";

        return $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames}) {returningStatement}";
    }

    public static string GenerateSelectQuery<T>(int? idFilter = null)
    {
        var type = typeof(T);

        var tableName = type.Name + "s";

        var whereStatement = idFilter.HasValue ? $"WHERE Id={idFilter}" : "";

        return $"SELECT * FROM {tableName} {whereStatement}";
    }
}
