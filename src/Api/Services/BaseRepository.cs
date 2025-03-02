using Dapper;
using Npgsql;
using static Api.Services.RepositoryHelper;

namespace Api.Repositories;

public class BaseRepository(NpgsqlConnection _sqlConnection)
{
    /// <returns>Id созданной в базе данных записи.</returns>
    public async Task<int> Create<T>(T entity)
    {
        var insertSql = GenerateInsertQuery<T>(true);
        var entityId = await _sqlConnection.QuerySingleAsync<int>(
            insertSql, entity);

        return entityId;
    }

    /// <returns>Все записи типа <typeparamref name="T"/> из базы данных.</returns>
    public async Task<IEnumerable<T>> Get<T>(int? idFilter = null)
    {
        var selectSql = GenerateSelectQuery<T>(idFilter);
        return await _sqlConnection.QueryAsync<T>(selectSql);
    }
}
