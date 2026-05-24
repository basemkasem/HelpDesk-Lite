import type { Category } from '../../types/ticket';
import type { AgentUser } from '../../types/ticket';
import type { TicketSearchParams } from '../../types/dashboard';

const STATUSES = ['New', 'Assigned', 'InProgress', 'WaitingForUser', 'Resolved', 'Closed'] as const;
const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;

export function FilterPanel({
  filters,
  onChange,
  onApply,
  onReset,
  categories,
  agents,
  showAssigneeFilters = true,
}: {
  filters: TicketSearchParams;
  onChange: (next: TicketSearchParams) => void;
  onApply: () => void;
  onReset: () => void;
  categories?: Category[];
  agents?: AgentUser[];
  showAssigneeFilters?: boolean;
}) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <h3 className="mb-3 font-semibold text-slate-900">Filters</h3>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <label className="block text-sm sm:col-span-2 lg:col-span-4">
          <span className="text-slate-600">Search</span>
          <input
            type="search"
            value={filters.search ?? ''}
            onChange={(e) => onChange({ ...filters, search: e.target.value })}
            placeholder="Title, number, description..."
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="text-slate-600">Status</span>
          <select
            value={filters.status ?? ''}
            onChange={(e) => onChange({ ...filters, status: e.target.value as TicketSearchParams['status'] })}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="">All</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm">
          <span className="text-slate-600">Priority</span>
          <select
            value={filters.priority ?? ''}
            onChange={(e) => onChange({ ...filters, priority: e.target.value as TicketSearchParams['priority'] })}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="">All</option>
            {PRIORITIES.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm">
          <span className="text-slate-600">Category</span>
          <select
            value={filters.categoryId ?? ''}
            onChange={(e) => onChange({ ...filters, categoryId: e.target.value || undefined })}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="">All</option>
            {categories?.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </label>
        {showAssigneeFilters && (
          <label className="block text-sm">
            <span className="text-slate-600">Assignee</span>
            <select
              value={filters.assigneeId ?? ''}
              onChange={(e) =>
                onChange({
                  ...filters,
                  assigneeId: e.target.value || undefined,
                  assignedToMe: false,
                  unassignedOnly: false,
                })
              }
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              <option value="">All</option>
              {agents?.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.fullName}
                </option>
              ))}
            </select>
          </label>
        )}
        <label className="block text-sm">
          <span className="text-slate-600">From date</span>
          <input
            type="date"
            value={filters.createdFrom ?? ''}
            onChange={(e) => onChange({ ...filters, createdFrom: e.target.value || undefined })}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="text-slate-600">To date</span>
          <input
            type="date"
            value={filters.createdTo ?? ''}
            onChange={(e) => onChange({ ...filters, createdTo: e.target.value || undefined })}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="text-slate-600">Sort by</span>
          <select
            value={filters.sortBy ?? 'createdAt'}
            onChange={(e) => onChange({ ...filters, sortBy: e.target.value })}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="createdAt">Created</option>
            <option value="priority">Priority</option>
            <option value="status">Status</option>
            <option value="title">Title</option>
            <option value="assignee">Assignee</option>
          </select>
        </label>
        <label className="block text-sm">
          <span className="text-slate-600">Direction</span>
          <select
            value={filters.sortDirection ?? 'desc'}
            onChange={(e) =>
              onChange({ ...filters, sortDirection: e.target.value as 'asc' | 'desc' })
            }
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="desc">Newest first</option>
            <option value="asc">Oldest first</option>
          </select>
        </label>
      </div>
      {showAssigneeFilters && (
        <div className="mt-3 flex flex-wrap gap-4 text-sm">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={!!filters.assignedToMe}
              onChange={(e) =>
                onChange({
                  ...filters,
                  assignedToMe: e.target.checked,
                  assigneeId: undefined,
                  unassignedOnly: false,
                })
              }
            />
            Assigned to me
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={!!filters.unassignedOnly}
              onChange={(e) =>
                onChange({
                  ...filters,
                  unassignedOnly: e.target.checked,
                  assigneeId: undefined,
                  assignedToMe: false,
                })
              }
            />
            Unassigned only
          </label>
        </div>
      )}
      <div className="mt-4 flex gap-2">
        <button
          type="button"
          onClick={onApply}
          className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
        >
          Apply filters
        </button>
        <button
          type="button"
          onClick={onReset}
          className="rounded-lg border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50"
        >
          Reset
        </button>
      </div>
    </div>
  );
}
