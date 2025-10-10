using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace InventarioDDD.Infrastructure.Cache
{
    /// <summary>
    /// Servicio de la capa de infraestructura con las operaciones implementadas
    /// para la instancia y el manejo de los datos (llevar, recuperar, eliminar) en la caché
    /// </summary>
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene un valor del caché por su clave
        /// </summary>
        public T? Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));

            try
            {
                return _memoryCache.Get<T>(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el valor del caché para la clave: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Establece un valor en el caché con una clave específica
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));

            try
            {
                var options = new MemoryCacheEntryOptions();
                options.SetAbsoluteExpiration(expiration);

                // Configurar prioridad para el caché
                options.SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(key, value, options);

                _logger.LogDebug("Valor almacenado en caché para la clave: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer el valor en caché para la clave: {Key}", key);
            }
        }

        /// <summary>
        /// Obtiene un valor del caché o lo crea si no existe
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));

            if (getItem == null)
                throw new ArgumentNullException(nameof(getItem));

            var cachedValue = Get<T>(key);
            if (cachedValue != null)
            {
                _logger.LogDebug("Valor encontrado en caché para la clave: {Key}", key);
                return cachedValue;
            }

            try
            {
                var item = await getItem();
                Set(key, item, expiration);
                _logger.LogDebug("Valor creado y almacenado en caché para la clave: {Key}", key);
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener o crear el valor para la clave: {Key}", key);
                throw;
            }
        }

        /// <summary>
        /// Elimina un valor del caché
        /// </summary>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));

            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Valor eliminado del caché para la clave: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el valor del caché para la clave: {Key}", key);
            }
        }

        /// <summary>
        /// Verifica si existe una clave en el caché
        /// </summary>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la existencia de la clave en caché: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Limpia todos los valores del caché (si es posible)
        /// </summary>
        public void Clear()
        {
            try
            {
                // En IMemoryCache no hay un método Clear directo
                // Podríamos implementar un sistema de tracking de claves si fuera necesario
                _logger.LogInformation("Solicitud de limpieza de caché recibida");

                // Una opción sería recrear la instancia del caché, pero eso requeriría
                // acceso al container de DI. Por ahora, dejamos esto como un método
                // que puede ser extendido según las necesidades específicas.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar limpiar el caché");
            }
        }

        /// <summary>
        /// Invalida entradas del caché que coincidan con un patrón
        /// </summary>
        public void InvalidateByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("El patrón no puede ser nulo o vacío", nameof(pattern));

            try
            {
                // Para implementar esto completamente, necesitaríamos mantener un registro
                // de todas las claves. Por ahora, dejamos la estructura para futuras mejoras.
                _logger.LogDebug("Invalidación por patrón solicitada: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar por patrón: {Pattern}", pattern);
            }
        }

        /// <summary>
        /// Genera claves de caché consistentes para entidades
        /// </summary>
        public string GenerateKey(params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                throw new ArgumentException("Los parámetros no pueden ser nulos o vacíos", nameof(parameters));

            return string.Join(":", parameters.Select(p => p?.ToString() ?? "null"));
        }

        /// <summary>
        /// Genera claves de caché consistentes para entidades
        /// </summary>
        public static string GenerateKey(string entityType, Guid id)
        {
            return $"{entityType}:{id}";
        }

        /// <summary>
        /// Genera claves de caché para consultas complejas
        /// </summary>
        public static string GenerateQueryKey(string queryType, params object[] parameters)
        {
            var paramString = string.Join(":", parameters.Select(p => p?.ToString() ?? "null"));
            return $"query:{queryType}:{paramString}";
        }
    }
}