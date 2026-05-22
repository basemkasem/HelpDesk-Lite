import type { ApiResponse } from '../types/auth';
import type { Ticket } from '../types/ticket';
import { apiClient } from './client';

export async function getTickets(): Promise<Ticket[]> {
  const { data } = await apiClient.get<ApiResponse<Ticket[]>>('/api/tickets');
  if (!data.success || !data.data) {
    throw new Error(data.error?.message ?? 'Failed to load tickets');
  }
  return data.data;
}
