import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getCategories } from './categoriesApi';
import { getKnowledgeBaseSuggestions } from './knowledgeBaseApi';
import type { CreateTicketCommentRequest } from '../types/ticket';
import {
  addTicketComment,
  assignTicket,
  createTicket,
  getTicketDetail,
  getTicketHistory,
  getTickets,
  updateTicketStatus,
} from './ticketsApi';
import { getAgents } from './usersApi';
import type { CreateTicketForm } from '../types/ticket';

export function useCategoriesQuery() {
  return useQuery({ queryKey: ['categories'], queryFn: getCategories });
}

export function useTicketsQuery() {
  return useQuery({ queryKey: ['tickets'], queryFn: getTickets });
}

export function useTicketDetailQuery(id: string) {
  return useQuery({
    queryKey: ['tickets', id],
    queryFn: () => getTicketDetail(id),
    enabled: !!id,
  });
}

export function useTicketHistoryQuery(id: string) {
  return useQuery({
    queryKey: ['tickets', id, 'history'],
    queryFn: () => getTicketHistory(id),
    enabled: !!id,
  });
}

export function useAgentsQuery(enabled: boolean) {
  return useQuery({
    queryKey: ['agents'],
    queryFn: getAgents,
    enabled,
  });
}

export function useCreateTicketMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (form: CreateTicketForm) => createTicket(form),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets'] });
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useUpdateStatusMutation(ticketId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (status: string) => updateTicketStatus(ticketId, { status }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets', ticketId] });
      void queryClient.invalidateQueries({ queryKey: ['tickets', ticketId, 'history'] });
      void queryClient.invalidateQueries({ queryKey: ['tickets'] });
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useAssignTicketMutation(ticketId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (assigneeId: string | null) => assignTicket(ticketId, { assigneeId }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets', ticketId] });
      void queryClient.invalidateQueries({ queryKey: ['tickets', ticketId, 'history'] });
      void queryClient.invalidateQueries({ queryKey: ['tickets'] });
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useAddCommentMutation(ticketId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateTicketCommentRequest) => addTicketComment(ticketId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['tickets', ticketId, 'history'] });
      void queryClient.invalidateQueries({ queryKey: ['tickets', ticketId] });
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useKbSuggestionsQuery(description: string) {
  return useQuery({
    queryKey: ['kb-suggestions', description],
    queryFn: () => getKnowledgeBaseSuggestions(description),
    enabled: description.trim().length >= 3,
    staleTime: 10_000,
  });
}
