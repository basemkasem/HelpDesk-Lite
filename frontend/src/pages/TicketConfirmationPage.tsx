import { Link, useLocation, useParams } from 'react-router-dom';
import { useTicketDetailQuery } from '../api/ticketHooks';
import type { Ticket } from '../types/ticket';

export function TicketConfirmationPage() {
  const { id } = useParams<{ id: string }>();
  const location = useLocation();
  const stateTicket = (location.state as { ticket?: Ticket } | null)?.ticket;
  const { data: fetchedTicket, isLoading } = useTicketDetailQuery(id ?? '');

  const ticket = stateTicket ?? fetchedTicket;

  if (isLoading && !ticket) {
    return <p className="text-slate-600">Loading confirmation...</p>;
  }

  if (!ticket) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-6 text-center">
        <p className="text-slate-600">Ticket not found.</p>
        <Link to="/tickets" className="mt-4 inline-block text-indigo-600 hover:text-indigo-800">
          View my tickets
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-lg rounded-2xl border border-emerald-200 bg-white p-8 shadow-sm">
      <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-emerald-100 text-emerald-700">
        <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden>
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
        </svg>
      </div>
      <h1 className="text-2xl font-bold text-slate-900">Ticket submitted</h1>
      <p className="mt-2 text-slate-600">
        Your request has been received. A support agent will review it shortly.
      </p>

      <dl className="mt-6 space-y-3 rounded-lg bg-slate-50 p-4 text-sm">
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">Ticket ID</dt>
          <dd className="font-mono font-medium text-slate-900">{ticket.ticketNumber}</dd>
        </div>
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">Subject</dt>
          <dd className="text-right font-medium text-slate-900">{ticket.title}</dd>
        </div>
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">Category</dt>
          <dd className="text-slate-900">{ticket.categoryName}</dd>
        </div>
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">Priority</dt>
          <dd className="text-slate-900">{ticket.priority}</dd>
        </div>
        <div className="flex justify-between gap-4">
          <dt className="text-slate-500">Status</dt>
          <dd className="text-slate-900">{ticket.status}</dd>
        </div>
        {ticket.attachments.length > 0 && (
          <div>
            <dt className="text-slate-500">Attachments</dt>
            <dd className="mt-1 text-slate-900">{ticket.attachments.length} file(s) uploaded</dd>
          </div>
        )}
      </dl>

      <div className="mt-6 flex flex-col gap-2 sm:flex-row">
        <Link
          to="/tickets/new"
          className="rounded-lg border border-slate-300 px-4 py-2 text-center text-sm font-medium hover:bg-slate-50"
        >
          Submit another ticket
        </Link>
        <Link
          to="/tickets"
          className="rounded-lg bg-indigo-600 px-4 py-2 text-center text-sm font-medium text-white hover:bg-indigo-700"
        >
          View my tickets
        </Link>
      </div>
    </div>
  );
}
