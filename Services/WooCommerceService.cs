using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class WooCommerceService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly GoogleImageService _googleImageService;

    public WooCommerceService(IConfiguration config, GoogleImageService googleImageService)
    {
        _baseUrl = config["WooCommerce:BaseUrl"];
        var key = config["WooCommerce:ConsumerKey"];
        var secret = config["WooCommerce:ConsumerSecret"];

        _httpClient = new HttpClient();
        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{key}:{secret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _googleImageService = googleImageService;
    }

    public async Task<List<dynamic>> ObtenerTodosLosProductos()
    {
        var todos = new List<dynamic>();
        int page = 1;
        const int pageSize = 100;

        while (true)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}products?per_page={pageSize}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var pagina = JsonConvert.DeserializeObject<List<dynamic>>(json);

            if (pagina == null || pagina.Count == 0)
                break;

            todos.AddRange(pagina);
            Console.WriteLine($"Productos traídos página {page}: {pagina.Count}");

            if (pagina.Count < pageSize)
                break;

            page++;
        }

        Console.WriteLine($"Total productos existentes en WooCommerce: {todos.Count}");
        return todos;
    }

    public async Task CrearProducto(Producto p)
    {
        try
        {
            var nuevo = new Dictionary<string, object?>
            {
                ["name"] = p.Descripcion,
                ["type"] = "simple",
                ["regular_price"] = p.PrecioFinal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                ["sku"] = p.CodigoBarra,
                ["categories"] = new[] {
                    new { name = p.Categoria },
                    new { name = p.SubCategoria }
                }.Where(c => !string.IsNullOrWhiteSpace(c.name)).ToArray(),
                ["description"] = p.Observaciones
            };

            var imageUrl = await _googleImageService.BuscarImagenPorSkuAsync(p.CodigoBarra ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                nuevo["images"] = new[] { new { src = imageUrl } };
            }

            var jsonString = JsonConvert.SerializeObject(nuevo);
            Console.WriteLine($"🆕 Enviando producto: {p.Descripcion} | SKU: {p.CodigoBarra}");
            Console.WriteLine(jsonString);

            var json = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var res = await _httpClient.PostAsync($"{_baseUrl}products", json);

            if (!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error al crear producto [{p.CodigoBarra}]: {res.StatusCode}");
                Console.WriteLine($"🪵 Respuesta del servidor: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 Excepción al crear producto [{p.CodigoBarra}]: {ex.Message}");
        }
    }

    public async Task ActualizarProducto(int id, Producto p)
    {
        try
        {
            var update = new Dictionary<string, object?>
            {
                ["name"] = p.Descripcion,
                ["regular_price"] = p.PrecioFinal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                ["description"] = p.Observaciones
            };

            // Verifica si tiene imagen, si no, agrega una desde Google
            var tieneImagen = await TieneImagen(id);
            if (!tieneImagen)
            {
                var imageUrl = await _googleImageService.BuscarImagenPorSkuAsync(p.CodigoBarra ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    update["images"] = new[] { new { src = imageUrl } };
                    Console.WriteLine($"🖼 Imagen añadida automáticamente desde Google para SKU {p.CodigoBarra}");
                }
            }

            var json = new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json");
            var res = await _httpClient.PutAsync($"{_baseUrl}products/{id}", json);

            if (!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error al actualizar producto [{p.CodigoBarra}] (ID {id}): {res.StatusCode}");
                Console.WriteLine($"🪵 Respuesta del servidor: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 Excepción al actualizar producto [{p.CodigoBarra}] (ID {id}): {ex.Message}");
        }
    }

    private async Task<bool> TieneImagen(int productId)
    {
        try
        {
            var res = await _httpClient.GetAsync($"{_baseUrl}products/{productId}");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            var images = data?.images;
            return images != null && images.Count > 0;
        }
        catch
        {
            return false;
        }
    }
}

