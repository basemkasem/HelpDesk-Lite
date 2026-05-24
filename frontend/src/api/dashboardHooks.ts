import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { BulkAssignRequest, TicketSearchParams } from '../types/dashboard';
import {
  bulkAssignTickets,
  getEmployeeDashboard,
  getManagerDashboard,
  getSupportQueueDashboard,
  searchTickets,
} from './dashboardApi';

const REFRESH_MS = 30_000;

export function useEmployeeDashboardQuery() {
  return useQuery({
    queryKey: ['dashboard', 'employee'],
    queryFn: getEmployeeDashboard,
    refetchInterval: REFRESH_MS,
  });
}

export function useSupportQueueDashboardQuery(enabled = true) {
  return useQuery({
    queryKey: ['dashboard', 'support-queue'],
    queryFn: getSupportQueueDashboard,
    enabled,
    refetchInterval: REFRESH_MS,
  });
}

export function useManagerDashboardQuery(enabled = true) {
  return useQuery({
    queryKey: ['dashboard', 'manager'],
    queryFn: getManagerDashboard,
    enabled,
    refetchInterval: REFRESH_MS,
  });
}

export function useTicketSearchQuery(params: TicketSearchParams, enabled = true) {
  return useQuery({
    queryKey: ['tickets', 'search', params],
    queryFn: () => searchTickets(params),
    enabled,
    refetchInterval: REFRESH_MS,
    placeholderData: (prev) => prev,
  });
}

export function useBulkAssignMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: BulkAssignRequest) => bulkAssignTickets(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['tickets'] });
      await queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}
