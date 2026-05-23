import type { ApiResponse } from '../types/auth';
import type {
  AssignTicketRequest,
  CreateTicketCommentRequest,
  CreateTicketForm,
  Ticket,
  TicketActivity,
  TicketComment,
  TicketDetail,
  UpdateTicketStatusRequest,
} from '../types/ticket';

export type {
  AssignTicketRequest,
  CreateTicketCommentRequest,
  UpdateTicketStatusRequest,
};
import { apiClient } from './client';
import { getApiErrorMessage } from './httpErrors';

export async function getTickets(): Promise<Ticket[]> {
  try {
    const { data } = await apiClient.get<ApiResponse<Ticket[]>>('/api/tickets');
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load tickets');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load tickets'));
  }
}

export async function getTicketDetail(id: string): Promise<TicketDetail> {
  try {
    const { data } = await apiClient.get<ApiResponse<TicketDetail>>(`/api/tickets/${id}`);
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load ticket');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load ticket'));
  }
}

export async function getTicketHistory(id: string): Promise<TicketActivity[]> {
  try {
    const { data } = await apiClient.get<ApiResponse<TicketActivity[]>>(`/api/tickets/${id}/history`);
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load history');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load history'));
  }
}

export async function createTicket(form: CreateTicketForm): Promise<Ticket> {
  const body = new FormData();
  body.append('title', form.title);
  body.append('categoryId', form.categoryId);
  body.append('description', form.description);
  body.append('priority', form.priority);
  for (const file of form.attachments) {
    body.append('attachments', file);
  }
  try {
    const { data } = await apiClient.post<ApiResponse<Ticket>>('/api/tickets', body);
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to submit ticket');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to submit ticket'));
  }
}

export async function updateTicketStatus(id: string, request: UpdateTicketStatusRequest): Promise<TicketDetail> {
  try {
    const { data } = await apiClient.patch<ApiResponse<TicketDetail>>(`/api/tickets/${id}/status`, request);
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to update status');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to update status'));
  }
}

export async function assignTicket(id: string, request: AssignTicketRequest): Promise<TicketDetail> {
  try {
    const { data } = await apiClient.patch<ApiResponse<TicketDetail>>(`/api/tickets/${id}/assign`, request);
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to assign ticket');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to assign ticket'));
  }
}

export async function addTicketComment(id: string, request: CreateTicketCommentRequest): Promise<TicketComment> {
  try {
    const { data } = await apiClient.post<ApiResponse<TicketComment>>(`/api/tickets/${id}/comments`, request);
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to add comment');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to add comment'));
  }
}

export const getTicket = getTicketDetail;
