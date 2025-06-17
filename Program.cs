using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

class Program
{
    private readonly GoogleImageService _googleImageServiceTest;

    public Program(GoogleImageService googleImageServiceTest)
    {
        _googleImageServiceTest = googleImageServiceTest;
    }

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
                .AddSingleton<GoogleImageService>()
                .BuildServiceProvider();

            //------------------------------------------------------------------------------------------------------------
            //Test GoogleImageService para verificar funcionamiento sin ingresar al bucle de sincronización
            var googleImageService = services.GetRequiredService<GoogleImageService>();
            var programInstance = new Program(googleImageService);
            var r = programInstance._googleImageServiceTest.BuscarImagenPorSkuAsync("7790368029647").GetAwaiter().GetResult();
            if (r != null)
            {
                Console.WriteLine($"Imagen de test encontrada: {r}");
            }
            else
            {
                Console.WriteLine("No se encontró imagen de test para el SKU proporcionado.");
            }
            //Fin Test GoogleImageService-----------------------------------------------------------------------------------

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
                try
                {
                    var existente = productosWoo.FirstOrDefault(p =>
                            ((string?)p?.sku)?.Trim() == producto.CodigoBarra?.Trim());

                    if (existente != null)
                    {
                        Console.WriteLine($"🔄 Actualizando producto SKU: {producto.CodigoBarra}");
                        await woo.ActualizarProducto((int)existente.id, producto);
                        actualizados++;
                    }
                    else
                    {
                        Console.WriteLine($"🆕 Creando producto SKU: {producto.CodigoBarra}");
                        await woo.CrearProducto(producto);
                        creados++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al procesar producto SKU: {producto.CodigoBarra} → {ex.Message}");
                }
            }

            Console.WriteLine($"✅ Sincronización completada. Productos actualizados: {actualizados}, creados: {creados}");

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