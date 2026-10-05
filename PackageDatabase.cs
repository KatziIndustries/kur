using Npgsql;

public class PackageDatabase
{
    private readonly string _connectionString;

    public PackageDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Get all package identifiers
    public async Task<List<string>> GetPackages()
    {
        var packages = new List<string>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT identifier FROM packages ORDER BY identifier",
            connection
        );

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            packages.Add(reader.GetString(0));
        }

        return packages;
    }

    // Get the katzibuild of a specific package
    public async Task<string?> GetKatzibuild(string identifier)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT katzibuild FROM packages WHERE identifier = @identifier",
            connection
        );

        command.Parameters.AddWithValue("identifier", identifier);

        var result = await command.ExecuteScalarAsync();

        return result as string;
    }

   public async Task SetKatzibuild(string identifier, string katzibuild)
   {
       await using var connection = new NpgsqlConnection(_connectionString);
       await connection.OpenAsync();

       const string sql = """
           INSERT INTO packages (identifier, katzibuild)
           VALUES (@identifier, @katzibuild)
           ON CONFLICT (identifier)
           DO UPDATE SET katzibuild = EXCLUDED.katzibuild;
           """;

       await using var command = new NpgsqlCommand(sql, connection);

       command.Parameters.AddWithValue("identifier", identifier);
       command.Parameters.AddWithValue("katzibuild", katzibuild);

       await command.ExecuteNonQueryAsync();
   }

}
