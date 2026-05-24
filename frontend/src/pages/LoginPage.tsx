import { useState, type FormEvent } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useLoginMutation } from '../api/authHooks';
import { useAuth } from '../auth/AuthContext';
import type { ApiResponse, LoginResponse } from '../types/auth';
import { isAxiosError } from 'axios';

export function LoginPage() {
  const navigate = useNavigate();
  const { isAuthenticated, login } = useAuth();
  const loginMutation = useLoginMutation();
  const [email, setEmail] = useState('employee@helpdesk.local');
  const [password, setPassword] = useState('Employee123!');
  const [error, setError] = useState<string | null>(null);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      const result = await loginMutation.mutateAsync({ email, password });
      login({ email, password }, result.accessToken, result.user);
      navigate('/dashboard');
    } catch (err) {
      if (isAxiosError(err)) {
        const apiError = err.response?.data as ApiResponse<LoginResponse> | undefined;
        setError(apiError?.error?.message ?? 'Invalid email or password.');
      } else {
        setError(err instanceof Error ? err.message : 'Login failed.');
      }
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="mb-2 text-2xl font-bold text-slate-900">HelpDesk Lite</h1>
        <p className="mb-6 text-sm text-slate-600">Sign in to access your tickets</p>

        <form onSubmit={(e) => void handleSubmit(e)} className="space-y-4">
          <div>
            <label htmlFor="email" className="mb-1 block text-sm font-medium text-slate-700">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
              required
            />
          </div>
          <div>
            <label htmlFor="password" className="mb-1 block text-sm font-medium text-slate-700">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
              required
            />
          </div>

          {error && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loginMutation.isPending}
            className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
          >
            {loginMutation.isPending ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <div className="mt-6 rounded-lg bg-slate-50 p-3 text-xs text-slate-600">
          <p className="font-medium text-slate-700">Demo accounts</p>
          <p>employee@helpdesk.local / Employee123!</p>
          <p>agent@helpdesk.local / Agent123!</p>
          <p>admin@helpdesk.local / Admin123!</p>
        </div>
      </div>
    </div>
  );
}
