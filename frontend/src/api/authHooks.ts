import { useMutation, useQuery } from '@tanstack/react-query';
import type { LoginRequest } from '../types/auth';
import { getCurrentUser, login } from './authApi';
import { TOKEN_KEY } from './client';

export function useLoginMutation() {
  return useMutation({
    mutationFn: (credentials: LoginRequest) => login(credentials),
  });
}

export function useCurrentUserQuery(enabled = true) {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: getCurrentUser,
    enabled: enabled && !!sessionStorage.getItem(TOKEN_KEY),
    retry: false,
  });
}
