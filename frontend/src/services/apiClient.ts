import axios, { AxiosInstance, AxiosError } from 'axios';

const API_BASE_URL = 'http://localhost:5261/api';

// Crear instancia de axios
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
});

// Interceptor para request
apiClient.interceptors.request.use(
  (config) => {
    // Aquí se pueden agregar tokens de autenticación si es necesario
    // const token = localStorage.getItem('token');
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`;
    // }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para response
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response) {
      // Error de respuesta del servidor
      const message = (error.response.data as any)?.message || 'Error en la solicitud';
      throw new Error(message);
    } else if (error.request) {
      // No se recibió respuesta
      throw new Error('No se pudo conectar con el servidor. Verifica que el backend esté corriendo.');
    } else {
      // Error en la configuración de la request
      throw new Error('Error al realizar la solicitud');
    }
  }
);

export default apiClient;
