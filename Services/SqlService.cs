using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

public class SqlService
{
    private readonly string _connectionString;
    private readonly int _tipoDePrecio;
    private readonly string _query;

    public SqlService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _tipoDePrecio = configuration.GetValue<int>("Parametros:TipoDePrecio");

        var queryPath = configuration.GetValue<string>("Parametros:RutaConsultaSQL");
        _query = File.ReadAllText(queryPath);
    }

    public IEnumerable<Producto> ObtenerProductos()
    {
        using var connection = new SqlConnection(_connectionString);
        return connection.Query<Producto>(_query, new { tdp_idesec = _tipoDePrecio });
    }
}
