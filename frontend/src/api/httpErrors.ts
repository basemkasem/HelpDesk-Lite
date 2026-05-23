import { isAxiosError } from 'axios';
import type { ApiResponse } from '../types/auth';

export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (isAxiosError(error)) {
    const api = error.response?.data as ApiResponse<unknown> | undefined;
    if (api?.error?.message) {
      return api.error.message;
    }
    if (error.response?.status === 403) {
      return 'Access denied. Log out, sign in again, and retry.';
    }
    if (error.response?.status === 401) {
      return 'Session expired. Please sign in again.';
    }
  }
  if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
}
