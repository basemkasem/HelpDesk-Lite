import { Link } from 'react-router-dom';
import { useTicketsQuery } from '../api/ticketHooks';
import { useAuth } from '../auth/AuthContext';

const priorityColors: Record<string, string> = {
  Low: 'bg-slate-100 text-slate-700',
  Medium: 'bg-blue-100 text-blue-800',
  High: 'bg-amber-100 text-amber-800',
  Critical: 'bg-red-100 text-red-800',
};

export function TicketsPage() {
  const { user } = useAuth();
  const { data: tickets, isLoading, error } = useTicketsQuery();

  return (
    <div>
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">My Tickets</h1>
          <p className="text-sm text-slate-600">
            {user?.role === 'Employee'
              ? 'Tickets you have submitted.'
              : 'All submitted tickets.'}
          </p>
        </div>
        <Link
          to="/tickets/new"
          className="inline-flex items-center justify-center rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700"
        >
          + New ticket
        </Link>
      </div>

      {isLoading && <p className="text-slate-600">Loading tickets...</p>}
      {error && (
        <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof Error
            ? error.message
            : 'Failed to load tickets'}
        </p>
      )}

      {tickets && tickets.length === 0 && (
        <div className="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center">
          <p className="text-slate-600">No tickets yet.</p>
          <Link to="/tickets/new" className="mt-3 inline-block text-indigo-600 hover:text-indigo-800">
            Submit your first ticket
          </Link>
        </div>
      )}

      <ul className="space-y-3">
        {tickets?.map((ticket) => (
          <li key={ticket.id}>
            <Link
              to={`/tickets/${ticket.id}`}
              className="block rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:border-indigo-300 hover:shadow-md"
            >
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div className="min-w-0 flex-1">
                <p className="font-mono text-xs text-slate-500">{ticket.ticketNumber}</p>
                <h2 className="font-medium text-slate-900">{ticket.title}</h2>
                <p className="mt-1 line-clamp-2 text-sm text-slate-600">{ticket.description}</p>
                <p className="mt-2 text-xs text-slate-500">
                  {ticket.categoryName} · {new Date(ticket.createdAt).toLocaleString()}
                </p>
              </div>
              <div className="flex flex-col items-end gap-1">
                <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-xs font-medium text-slate-700">
                  {ticket.status}
                </span>
                <span
                  className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${priorityColors[ticket.priority] ?? 'bg-slate-100 text-slate-700'}`}
                >
                  {ticket.priority}
                </span>
              </div>
            </div>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
