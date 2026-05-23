import type { ApiResponse } from '../types/auth';
import type { AgentUser } from '../types/ticket';
import { apiClient } from './client';
import { getApiErrorMessage } from './httpErrors';

export async function getAgents(): Promise<AgentUser[]> {
  try {
    const { data } = await apiClient.get<ApiResponse<AgentUser[]>>('/api/users/agents');
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load agents');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load agents'));
  }
}
