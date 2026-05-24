import { Link } from 'react-router-dom';
import type { TicketListItem } from '../../types/dashboard';
import { PriorityBadge } from './PriorityBadge';
import { StatusBadge } from './StatusBadge';

export function TicketTable({
  tickets,
  isLoading,
  selectable = false,
  selectedIds,
  onToggle,
  onToggleAll,
  showRequester = true,
}: {
  tickets: TicketListItem[];
  isLoading?: boolean;
  selectable?: boolean;
  selectedIds?: Set<string>;
  onToggle?: (id: string) => void;
  onToggleAll?: (checked: boolean) => void;
  showRequester?: boolean;
}) {
  if (isLoading) {
    return <p className="text-slate-600">Loading tickets...</p>;
  }

  if (tickets.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center text-slate-600">
        No tickets match your filters.
      </div>
    );
  }

  const allSelected =
    selectable && selectedIds && tickets.length > 0 && tickets.every((t) => selectedIds.has(t.id));

  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
      <table className="min-w-full text-left text-sm">
        <thead className="border-b border-slate-200 bg-slate-50 text-slate-600">
          <tr>
            {selectable && (
              <th className="px-3 py-3">
                <input
                  type="checkbox"
                  checked={!!allSelected}
                  onChange={(e) => onToggleAll?.(e.target.checked)}
                  aria-label="Select all"
                />
              </th>
            )}
            <th className="px-3 py-3">Ticket</th>
            <th className="px-3 py-3">Status</th>
            <th className="px-3 py-3">Priority</th>
            <th className="px-3 py-3">Category</th>
            {showRequester && <th className="px-3 py-3">Requester</th>}
            <th className="px-3 py-3">Assignee</th>
            <th className="px-3 py-3">Age</th>
          </tr>
        </thead>
        <tbody>
          {tickets.map((ticket) => (
            <tr
              key={ticket.id}
              className={`border-b border-slate-100 hover:bg-slate-50 ${ticket.isDelayed ? 'bg-amber-50/50' : ''}`}
            >
              {selectable && (
                <td className="px-3 py-3">
                  <input
                    type="checkbox"
                    checked={selectedIds?.has(ticket.id) ?? false}
                    onChange={() => onToggle?.(ticket.id)}
                    aria-label={`Select ${ticket.ticketNumber}`}
                  />
                </td>
              )}
              <td className="px-3 py-3">
                <Link to={`/tickets/${ticket.id}`} className="font-medium text-indigo-600 hover:text-indigo-800">
                  <span className="font-mono text-xs text-slate-500">{ticket.ticketNumber}</span>
                  <br />
                  {ticket.title}
                </Link>
                {ticket.isDelayed && (
                  <span className="ml-2 inline-flex rounded bg-amber-200 px-1.5 py-0.5 text-xs font-medium text-amber-900">
                    Delayed
                  </span>
                )}
              </td>
              <td className="px-3 py-3">
                <StatusBadge status={ticket.status} />
              </td>
              <td className="px-3 py-3">
                <PriorityBadge priority={ticket.priority} />
              </td>
              <td className="px-3 py-3 text-slate-700">{ticket.categoryName}</td>
              {showRequester && <td className="px-3 py-3 text-slate-700">{ticket.createdByName}</td>}
              <td className="px-3 py-3 text-slate-700">{ticket.assigneeName ?? '—'}</td>
              <td className="px-3 py-3 text-slate-600">{ticket.ageInDays}d</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
