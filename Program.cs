using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Console.WriteLine("Inicializando configuración...");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(builder)
                .AddSingleton<SqlService>()
                .AddSingleton<WooCommerceService>()
                .BuildServiceProvider();

            Console.WriteLine("Inicializando servicios...");
            var db = services.GetRequiredService<SqlService>();
            var woo = services.GetRequiredService<WooCommerceService>();

            Console.WriteLine("Obteniendo productos desde SQL Server...");
            var productos = db.ObtenerProductos().ToList();
            Console.WriteLine($"Productos obtenidos desde SQL Server: {productos.Count}");

            Console.WriteLine("Obteniendo productos desde WooCommerce...");
            var productosWoo = await woo.ObtenerTodosLosProductos();
            Console.WriteLine($"Productos existentes en WooCommerce: {productosWoo.Count}");

            int actualizados = 0, creados = 0;
            foreach (var producto in productos)
            {
                var existente = productosWoo.FirstOrDefault(p => (string?)p?.sku == producto.CodigoBarra);


                if (existente != null)
                {
                    await woo.ActualizarProducto((int)existente.id, producto);
                    actualizados++;
                }
                else
                {
                    await woo.CrearProducto(producto);
                    creados++;
                }
            }

            stopwatch.Stop();
            Console.WriteLine("Sincronización completada.");
            Console.WriteLine($"Productos actualizados: {actualizados}, productos creados: {creados}");
            Console.WriteLine($"Tiempo total de ejecución: {stopwatch.Elapsed.TotalSeconds:F2} segundos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error: {ex.Message}");
        }
    }
}