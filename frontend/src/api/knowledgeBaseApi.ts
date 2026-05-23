import type { ApiResponse } from '../types/auth';
import type { KnowledgeBaseSuggestion } from '../types/ticket';
import { apiClient } from './client';

export async function getKnowledgeBaseSuggestions(description: string): Promise<KnowledgeBaseSuggestion[]> {
  if (!description.trim() || description.trim().length < 3) {
    return [];
  }

  const { data } = await apiClient.get<ApiResponse<KnowledgeBaseSuggestion[]>>(
    '/api/knowledgebase/suggestions',
    { params: { description } },
  );
  if (!data.success || !data.data) {
    return [];
  }
  return data.data;
}
