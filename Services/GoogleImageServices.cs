using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GoogleImageService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _searchEngineId;

    public GoogleImageService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["GoogleSearch:ApiKey"] ?? throw new ArgumentNullException("GoogleSearch:ApiKey");
        _searchEngineId = config["GoogleSearch:SearchEngineId"] ?? throw new ArgumentNullException("GoogleSearch:SearchEngineId");
    }

    public async Task<string?> BuscarImagenPorSkuAsync(string sku)
    {
        try
        {
            var url = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(sku)}&cx={_searchEngineId}&key={_apiKey}&searchType=image&num=1";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var imageUrl = doc.RootElement
                .GetProperty("items")[0]
                .GetProperty("link")
                .GetString();

            return imageUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar imagen para SKU {sku}: {ex.Message}");
            return null;
        }
    }
}
