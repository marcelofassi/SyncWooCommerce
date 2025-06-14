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

            // Configurar headers más completos para evitar bloqueos
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "es-ES,es;q=0.9,en;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            _httpClient.DefaultRequestHeaders.Add("DNT", "1");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            var html = await _httpClient.GetStringAsync(url);

            // Múltiples patrones para encontrar imágenes
            var patterns = new[]
            {
            // Imágenes de Google encriptadas
            @"<img[^>]+src=""(https://encrypted-tbn[^""]+)""[^>]*class=""DS1iW""",
            
            // Imágenes con data-src
            @"<img[^>]+data-src=""(https://[^""]+\.(jpg|jpeg|png|webp))""",
            
            // Imágenes directas en src
            @"<img[^>]+src=""(https://[^""]+\.(jpg|jpeg|png|webp))""",
            
            // Patrón más general para imágenes de Google
            @"<img[^>]+src=""(https://encrypted-tbn\d+\.gstatic\.com/images[^""]+)""",
            
            // URLs de imágenes en atributos de datos
            @"""(https://[^""]*\.(jpg|jpeg|png|webp))[^""]*""",
        };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    if (match.Success && match.Groups.Count > 1)
                    {
                        var imageUrl = match.Groups[1].Value;

                        // Validar que la URL no sea un ícono o imagen muy pequeña
                        if (IsValidImageUrl(imageUrl))
                        {
                            return imageUrl;
                        }
                    }
                }
            }

            // Si no se encuentra nada con los patrones anteriores, buscar en el JavaScript
            return ExtractImageFromJavaScript(html);
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Error de HTTP al buscar imagen para SKU {sku}: {httpEx.Message}");
            return null;
        }
        catch (TaskCanceledException tcEx)
        {
            Console.WriteLine($"Timeout al buscar imagen para SKU {sku}: {tcEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general al buscar imagen para SKU {sku}: {ex.Message}");
            return null;
        }
    }

    private bool IsValidImageUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Excluir iconos y imágenes muy pequeñas comunes
        var excludePatterns = new[]
        {
        "logo", "icon", "favicon", "button", "arrow", "star",
        "1x1", "pixel", "transparent", "spacer"
    };

        var lowerUrl = url.ToLower();
        return !excludePatterns.Any(pattern => lowerUrl.Contains(pattern));
    }

    private string? ExtractImageFromJavaScript(string html)
    {
        try
        {
            // Buscar URLs de imágenes en el JavaScript de la página
            var jsPattern = @"""https://[^""]*\.(jpg|jpeg|png|webp)[^""]*""";
            var matches = Regex.Matches(html, jsPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var url = match.Value.Trim('"');
                if (IsValidImageUrl(url) && !url.Contains("logo") && !url.Contains("icon"))
                {
                    return url;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    // Método alternativo usando HtmlAgilityPack (recomendado)
    public async Task<string?> BuscarImagenPorSkuConHtmlAgilityAsync(string sku)
    {
        try
        {
            var query = System.Web.HttpUtility.UrlEncode(sku);
            var url = $"https://www.google.com/search?q={query}&tbm=isch";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            var html = await _httpClient.GetStringAsync(url);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // Buscar imágenes con clase DS1iW (imágenes principales de resultados)
            var imageNodes = doc.DocumentNode
                .SelectNodes("//img[@class='DS1iW']") ??
                doc.DocumentNode.SelectNodes("//img[contains(@src, 'encrypted-tbn')]") ??
                doc.DocumentNode.SelectNodes("//img[contains(@src, 'http')]");

            if (imageNodes != null)
            {
                foreach (var node in imageNodes)
                {
                    var src = node.GetAttributeValue("src", "") ??
                             node.GetAttributeValue("data-src", "");

                    if (!string.IsNullOrEmpty(src) && IsValidImageUrl(src))
                    {
                        return src;
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar imagen para SKU {sku}: {ex.Message}");
            return null;
        }
    }

}
