using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class GoogleImageService
{
    private readonly HttpClient _httpClient;

    public GoogleImageService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string?> BuscarImagenPorSkuAsync(string sku)
    {
        try
        {
            var query = System.Web.HttpUtility.UrlEncode(sku);
            var url = $"https://www.google.com/search?q={query}&tbm=isch";

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            var html = await _httpClient.GetStringAsync(url);

            var match = Regex.Match(html, @"https://[^""]+\.(jpg|png|jpeg)", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar imagen para SKU {sku}: {ex.Message}");
            return null;
        }
    }
}
