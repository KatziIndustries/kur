using Npgsql;
using System;
using System.Threading.Tasks;

public static class DB {
    public static async Task<string[]> GetRows() {
        var constring = "Host=localhost;Port=5432;Database=kur;Username=admin;Password=password";

        await using var conn = new NpgsqlConnection(constring);

        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name FROM users",
            conn
        );

        return [];
    }
}
