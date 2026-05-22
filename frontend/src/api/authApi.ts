import type { ApiResponse, LoginRequest, LoginResponse, User } from '../types/auth';
import { apiClient } from './client';

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await apiClient.post<ApiResponse<LoginResponse>>('/api/auth/login', request);
  if (!data.success || !data.data) {
    throw new Error(data.error?.message ?? 'Login failed');
  }
  return data.data;
}

export async function logout(): Promise<void> {
  await apiClient.post('/api/auth/logout');
}

export async function getCurrentUser(): Promise<User> {
  const { data } = await apiClient.get<ApiResponse<User>>('/api/auth/me');
  if (!data.success || !data.data) {
    throw new Error(data.error?.message ?? 'Failed to load user');
  }
  return data.data as User;
}
