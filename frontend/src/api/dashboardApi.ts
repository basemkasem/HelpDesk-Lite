import type { ApiResponse } from '../types/auth';
import type {
  BulkAssignRequest,
  EmployeeDashboard,
  ManagerDashboard,
  PagedTickets,
  SupportQueueDashboard,
  TicketSearchParams,
} from '../types/dashboard';
import { apiClient } from './client';
import { getApiErrorMessage } from './httpErrors';

function toQueryParams(params: TicketSearchParams): Record<string, string | number | boolean> {
  const query: Record<string, string | number | boolean> = {};
  if (params.search) query.search = params.search;
  if (params.status) query.status = params.status;
  if (params.priority) query.priority = params.priority;
  if (params.categoryId) query.categoryId = params.categoryId;
  if (params.assigneeId) query.assigneeId = params.assigneeId;
  if (params.assignedToMe) query.assignedToMe = true;
  if (params.unassignedOnly) query.unassignedOnly = true;
  if (params.createdFrom) query.createdFrom = params.createdFrom;
  if (params.createdTo) query.createdTo = params.createdTo;
  if (params.sortBy) query.sortBy = params.sortBy;
  if (params.sortDirection) query.sortDirection = params.sortDirection;
  if (params.page) query.page = params.page;
  if (params.pageSize) query.pageSize = params.pageSize;
  return query;
}

export async function getEmployeeDashboard(): Promise<EmployeeDashboard> {
  try {
    const { data } = await apiClient.get<ApiResponse<EmployeeDashboard>>('/api/dashboard/employee');
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load dashboard');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load dashboard'));
  }
}

export async function getSupportQueueDashboard(): Promise<SupportQueueDashboard> {
  try {
    const { data } = await apiClient.get<ApiResponse<SupportQueueDashboard>>('/api/dashboard/support-queue');
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load queue dashboard');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load queue dashboard'));
  }
}

export async function getManagerDashboard(): Promise<ManagerDashboard> {
  try {
    const { data } = await apiClient.get<ApiResponse<ManagerDashboard>>('/api/dashboard/manager');
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to load manager dashboard');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to load manager dashboard'));
  }
}

export async function searchTickets(params: TicketSearchParams): Promise<PagedTickets> {
  try {
    const { data } = await apiClient.get<ApiResponse<PagedTickets>>('/api/tickets/search', {
      params: toQueryParams(params),
    });
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to search tickets');
    }
    return data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to search tickets'));
  }
}

export async function bulkAssignTickets(request: BulkAssignRequest): Promise<number> {
  try {
    const { data } = await apiClient.post<ApiResponse<{ updatedCount: number }>>(
      '/api/tickets/bulk-assign',
      request,
    );
    if (!data.success || !data.data) {
      throw new Error(data.error?.message ?? 'Failed to assign tickets');
    }
    return data.data.updatedCount;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to assign tickets'));
  }
}
