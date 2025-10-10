namespace InventarioDDD.Infrastructure.Cache
{
    /// <summary>
    /// Interfaz para el servicio de caché en memoria
    /// </summary>
    public interface IMemoryCacheService
    {
        /// <summary>
        /// Obtiene un valor del caché por su clave
        /// </summary>
        T? Get<T>(string key);

        /// <summary>
        /// Establece un valor en el caché con una clave específica
        /// </summary>
        void Set<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// Obtiene un valor del caché o lo establece si no existe
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan expiration);

        /// <summary>
        /// Remueve un valor del caché por su clave
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Genera una clave de caché consistente
        /// </summary>
        string GenerateKey(params object[] keyParts);
    }
}