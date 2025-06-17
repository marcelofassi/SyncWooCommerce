
# SyncWooCommerce

Aplicación en .NET Core que sincroniza productos desde una base de datos SQL Server hacia una tienda WooCommerce. Crea o actualiza productos según su código de barras (SKU).

## Funcionalidades

- Lectura de productos desde SQL Server con script configurable.
- Conexión a la API de WooCommerce para creación y actualización de productos.
- Búsqueda automática de imágenes usando Google Custom Search.
- Logueo detallado por consola (productos leídos, creados, errores, tiempos).

## Requisitos

- .NET Core 8.0
- Acceso a una base SQL Server
- WooCommerce con API habilitada (Consumer Key / Secret)
- Cuenta en Google Cloud para búsquedas de imágenes

## Configuración

Editar `appsettings.json` con los siguientes parámetros:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=TU_SERVIDOR;Database=TU_BASE;User Id=USUARIO;Password=CLAVE;TrustServerCertificate=True;"
  },
  "WooCommerce": {
    "BaseUrl": "https://tusitio.com/wp-json/wc/v3/",
    "ConsumerKey": "tu_consumer_key",
    "ConsumerSecret": "tu_consumer_secret"
  },
  "GoogleSearch": {
    "ApiKey": "TU_API_KEY",
    "SearchEngineId": "TU_SEARCH_ENGINE_ID"
  },
  "Parametros": {
    "TdpIdesec": 1
  }
}
```

## Cómo obtener la API Key y SearchEngineId de Google

1. Ingresar a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear un nuevo proyecto (o usar uno existente)
3. Habilitar la **Custom Search API**
4. Ir a `APIs y Servicios > Credenciales` y crear una nueva **clave de API**
5. Ir a [Google Programmable Search Engine](https://programmablesearchengine.google.com/)
6. Crear un motor de búsqueda con el sitio `www.google.com`
7. Activar la búsqueda de imágenes
8. Copiar el **Search Engine ID (cx)** generado

## Enlace para continuar esta conversación en ChatGPT

[Acceder al historial de este proyecto en ChatGPT](https://chat.openai.com/share)

---

Para cualquier mejora, abrir un issue o contactar al desarrollador.
