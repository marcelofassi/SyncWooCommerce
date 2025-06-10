# 🛒 SyncWooCommerce

Aplicación desarrollada en **C# .NET Core** que sincroniza productos desde una base de datos SQL Server con una tienda **WooCommerce**.  
Lee los productos usando una consulta SQL personalizada, y los compara con los existentes en WooCommerce vía API. Si el producto existe, lo actualiza; si no, lo crea.

---

## 🚀 Características

- Conexión a SQL Server mediante configuración externa
- Consulta SQL editable desde archivo externo
- Comparación por SKU (código de barras)
- Creación o actualización de productos vía API REST de WooCommerce
- Manejo robusto de errores y logs de progreso en consola
- Modularizado en servicios (`SqlService`, `WooCommerceService`, `GoogleImageService`, etc.)

---

## ⚙️ Configuración

Editar el archivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=MI_SERVIDOR;Database=MI_BASE;User Id=USUARIO;Password=CLAVE;"
  },
  "WooCommerce": {
    "BaseUrl": "https://mitienda.com/wp-json/wc/v3/",
    "ConsumerKey": "ck_XXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "ConsumerSecret": "cs_XXXXXXXXXXXXXXXXXXXXXXXXXXXX"
  },
  "ConsultaSqlPath": "consulta.sql",
  "TdpIdesec": 1
}
```

- Asegurarse de que `consulta.sql` esté presente en la raíz del proyecto o especificar la ruta completa.

---

## ▶️ Ejecución

```bash
dotnet build
dotnet run
```

Requiere [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) o superior.

---

## 📦 Dependencias

- `Microsoft.Data.SqlClient`
- `Newtonsoft.Json`
- `Microsoft.Extensions.Configuration`

---

## 📄 Estructura del Proyecto

```
/SyncWooCommerce
├── Program.cs
├── appsettings.json
├── SqlService.cs
├── WooCommerceService.cs
├── GoogleImageService.cs
├── consulta.sql
└── README.md
```

---

## 🧠 ¿Problemas o mejoras?

Podés continuar la conversación técnica directamente en este chat de ChatGPT:  
👉 [Volver al hilo del proyecto](https://chatgpt.com/c/683b6726-1d64-8003-890e-bad335bd406c)

---

## 📌 Nota

Si la API de WooCommerce devuelve solo un producto cuando hay más disponibles, 
Revisá que estés usando paginación correcta (`?per_page=100&page=1,2,3...`) y que no haya restricciones en la tienda (por ejemplo, productos ocultos, borradores, o sin visibilidad en la API)
Asegurate que no existan productos en borrado lógico (papelera de reciclaje de WooCommerce) .

---

## 📬 Contacto

Creado por Marcelo Fassi • Proyecto privado para sincronización de ecommerce con WooCommerce.
