// lib/api.ts
import axios, {
    AxiosInstance,
    AxiosRequestConfig,
  } from 'axios';
  
  const baseURL = process.env.NEXT_PUBLIC_API_URL;
  if (!baseURL) {
    throw new Error('Environment variable NEXT_PUBLIC_API_URL is not defined');
  }
  
  const api: AxiosInstance = axios.create({
    baseURL,
    //timeout: 10_000,
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    }
  });
  
  export async function get<T>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const { data } = await api.get<T>(url, config);
    return data;
  }
  
  export async function post<T, D = any>(
    url: string,
    payload?: D,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const { data } = await api.post<T>(url, payload, config);
    return data;
  }
  
  export async function put<T, D = any>(
    url: string,
    payload?: D,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const { data } = await api.put<T>(url, payload, config);
    return data;
  }
  
  export async function remove<T>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const { data } = await api.delete<T>(url, config);
    return data;
  }
  
  export default api;
  