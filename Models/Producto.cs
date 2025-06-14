public class Producto
{
    public string? Categoria { get; set; }
    public string? SubCategoria { get; set; }
    public string? CodigoBarra { get; set; }
    public string? Descripcion { get; set; }
    public decimal PrecioFinal { get; set; }
    public string? Observaciones { get; set; }

    public int? CategoriaId { get; set; }
    public int? SubCategoriaId { get; set; }

}