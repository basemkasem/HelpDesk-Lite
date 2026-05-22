import { useQuery } from '@tanstack/react-query';
import { getTickets } from '../api/ticketsApi';
import { useAuth } from '../auth/AuthContext';

export function TicketsPage() {
  const { user } = useAuth();
  const { data: tickets, isLoading, error } = useQuery({
    queryKey: ['tickets'],
    queryFn: getTickets,
  });

  return (
    <div>
      <h1 className="mb-2 text-2xl font-bold text-slate-900">My Tickets</h1>
      <p className="mb-6 text-sm text-slate-600">
        {user?.role === 'Employee'
          ? 'Showing tickets you created.'
          : 'Showing all tickets (agent/admin view).'}
      </p>

      {isLoading && <p className="text-slate-600">Loading tickets...</p>}
      {error && (
        <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof Error ? error.message : 'Failed to load tickets'}
        </p>
      )}

      {tickets && tickets.length === 0 && (
        <p className="text-slate-600">No tickets found.</p>
      )}

      <ul className="space-y-3">
        {tickets?.map((ticket) => (
          <li
            key={ticket.id}
            className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
          >
            <div className="flex items-center justify-between gap-4">
              <h2 className="font-medium text-slate-900">{ticket.title}</h2>
              <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-700">
                {ticket.status}
              </span>
            </div>
            <p className="mt-1 text-xs text-slate-500">ID: {ticket.id}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
