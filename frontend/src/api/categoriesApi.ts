import type { ApiResponse } from '../types/auth';
import type { Category } from '../types/ticket';
import { apiClient } from './client';

export async function getCategories(): Promise<Category[]> {
  const { data } = await apiClient.get<ApiResponse<Category[]>>('/api/categories');
  if (!data.success || !data.data) {
    throw new Error(data.error?.message ?? 'Failed to load categories');
  }
  return data.data;
}
