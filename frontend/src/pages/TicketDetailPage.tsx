import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import {
  useAddCommentMutation,
  useAgentsQuery,
  useAssignTicketMutation,
  useTicketDetailQuery,
  useTicketHistoryQuery,
  useUpdateStatusMutation,
} from '../api/ticketHooks';
import { useAuth } from '../auth/AuthContext';

const statusColors: Record<string, string> = {
  New: 'bg-slate-100 text-slate-800',
  Assigned: 'bg-purple-100 text-purple-800',
  InProgress: 'bg-blue-100 text-blue-800',
  WaitingForUser: 'bg-amber-100 text-amber-800',
  Resolved: 'bg-emerald-100 text-emerald-800',
  Closed: 'bg-gray-200 text-gray-700',
};

const activityIcons: Record<string, string> = {
  Created: '●',
  StatusChange: '↻',
  Assignment: '👤',
  Comment: '💬',
  InternalComment: '🔒',
  Attachment: '📎',
};

export function TicketDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const isStaff = user?.role === 'SupportAgent' || user?.role === 'ManagerAdmin';

  const { data: ticket, isLoading, error } = useTicketDetailQuery(id ?? '');
  const { data: history, isLoading: historyLoading } = useTicketHistoryQuery(id ?? '');
  const { data: agents } = useAgentsQuery(isStaff);

  const updateStatus = useUpdateStatusMutation(id ?? '');
  const assignTicket = useAssignTicketMutation(id ?? '');
  const addComment = useAddCommentMutation(id ?? '');

  const [newStatus, setNewStatus] = useState('');
  const [assigneeId, setAssigneeId] = useState('');
  const [comment, setComment] = useState('');
  const [isInternal, setIsInternal] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  if (isLoading) {
    return <p className="text-slate-600">Loading ticket...</p>;
  }

  if (error || !ticket) {
    return (
      <div className="rounded-xl border border-red-200 bg-red-50 p-6">
        <p className="text-red-700">{error instanceof Error ? error.message : 'Ticket not found'}</p>
        <Link to="/tickets" className="mt-3 inline-block text-indigo-600">
          Back to tickets
        </Link>
      </div>
    );
  }

  const handleStatusUpdate = async () => {
    if (!newStatus) return;
    setActionError(null);
    try {
      await updateStatus.mutateAsync(newStatus);
      setNewStatus('');
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Status update failed');
    }
  };

  const handleAssign = async () => {
    setActionError(null);
    try {
      await assignTicket.mutateAsync(assigneeId || null);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Assignment failed');
    }
  };

  const handleComment = async () => {
    if (!comment.trim()) return;
    setActionError(null);
    try {
      await addComment.mutateAsync({ comment: comment.trim(), isInternal });
      setComment('');
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Failed to add comment');
    }
  };

  return (
    <div className="grid gap-6 lg:grid-cols-3">
      <div className="space-y-6 lg:col-span-2">
        <div>
          <Link to="/tickets" className="text-sm text-indigo-600 hover:text-indigo-800">
            ← Back to tickets
          </Link>
          <div className="mt-2 flex flex-wrap items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-900">{ticket.title}</h1>
            <span className="font-mono text-sm text-slate-500">{ticket.ticketNumber}</span>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${statusColors[ticket.status] ?? 'bg-slate-100'}`}>
              {ticket.status}
            </span>
          </div>
        </div>

        <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-3 font-semibold text-slate-900">Description</h2>
          <p className="whitespace-pre-wrap text-slate-700">{ticket.description}</p>
          <dl className="mt-4 grid gap-2 text-sm sm:grid-cols-2">
            <div>
              <dt className="text-slate-500">Category</dt>
              <dd className="font-medium">{ticket.categoryName}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Priority</dt>
              <dd className="font-medium">{ticket.priority}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Created by</dt>
              <dd className="font-medium">{ticket.createdByName}</dd>
            </div>
            <div>
              <dt className="text-slate-500">Assignee</dt>
              <dd className="font-medium">{ticket.assigneeName ?? 'Unassigned'}</dd>
            </div>
          </dl>
        </section>

        <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 font-semibold text-slate-900">Activity timeline</h2>
          {historyLoading && <p className="text-sm text-slate-500">Loading activity...</p>}
          <ol className="relative border-l border-slate-200 pl-6">
            {history?.map((item) => (
              <li key={`${item.activityType}-${item.id}`} className="mb-6 last:mb-0">
                <span className="absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full bg-indigo-100 text-xs text-indigo-700">
                  {activityIcons[item.activityType] ?? '•'}
                </span>
                <p className="text-sm font-medium text-slate-900">{item.summary}</p>
                {item.detail && (
                  <p className="mt-1 whitespace-pre-wrap text-sm text-slate-600">{item.detail}</p>
                )}
                <p className="mt-1 text-xs text-slate-500">
                  {item.actorName && `${item.actorName} · `}
                  {new Date(item.occurredAt).toLocaleString()}
                  {item.isInternal && ' · Internal'}
                </p>
              </li>
            ))}
          </ol>
        </section>
      </div>

      <aside className="space-y-4">
        {actionError && (
          <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{actionError}</p>
        )}

        {isStaff && (
          <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
            <h3 className="mb-3 font-semibold text-slate-900">Update status</h3>
            <select
              value={newStatus}
              onChange={(e) => setNewStatus(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              <option value="">Select new status</option>
              {ticket.allowedNextStatuses.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
            <button
              type="button"
              disabled={!newStatus || updateStatus.isPending}
              onClick={() => void handleStatusUpdate()}
              className="mt-2 w-full rounded-lg bg-indigo-600 px-3 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
            >
              {updateStatus.isPending ? 'Updating...' : 'Update status'}
            </button>
          </section>
        )}

        {isStaff && (
          <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
            <h3 className="mb-3 font-semibold text-slate-900">Assign ticket</h3>
            <select
              value={assigneeId}
              onChange={(e) => setAssigneeId(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              <option value="">Unassigned</option>
              {agents?.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.fullName}
                </option>
              ))}
            </select>
            <button
              type="button"
              disabled={assignTicket.isPending}
              onClick={() => void handleAssign()}
              className="mt-2 w-full rounded-lg border border-indigo-600 px-3 py-2 text-sm font-medium text-indigo-700 hover:bg-indigo-50 disabled:opacity-60"
            >
              {assignTicket.isPending ? 'Saving...' : 'Save assignment'}
            </button>
          </section>
        )}

        <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <h3 className="mb-3 font-semibold text-slate-900">Add comment</h3>
          <textarea
            rows={4}
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder={isStaff ? 'Reply to employee or add internal note...' : 'Add a comment...'}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
          {isStaff && (
            <label className="mt-2 flex items-center gap-2 text-sm text-slate-600">
              <input
                type="checkbox"
                checked={isInternal}
                onChange={(e) => setIsInternal(e.target.checked)}
              />
              Internal note (hidden from employee)
            </label>
          )}
          <button
            type="button"
            disabled={!comment.trim() || addComment.isPending}
            onClick={() => void handleComment()}
            className="mt-2 w-full rounded-lg bg-slate-800 px-3 py-2 text-sm font-medium text-white hover:bg-slate-900 disabled:opacity-60"
          >
            {addComment.isPending ? 'Posting...' : 'Post comment'}
          </button>
        </section>
      </aside>
    </div>
  );
}
