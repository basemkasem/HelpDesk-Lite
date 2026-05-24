import { useMemo, useState } from 'react';
import {
  useBulkAssignMutation,
  useSupportQueueDashboardQuery,
  useTicketSearchQuery,
} from '../../api/dashboardHooks';
import { useAgentsQuery, useCategoriesQuery } from '../../api/ticketHooks';
import { FilterPanel } from '../../components/dashboard/FilterPanel';
import { KpiCard } from '../../components/dashboard/KpiCard';
import { Pagination } from '../../components/dashboard/Pagination';
import { StatusOverview } from '../../components/dashboard/StatusOverview';
import { TicketTable } from '../../components/dashboard/TicketTable';
import type { TicketSearchParams } from '../../types/dashboard';

const defaultFilters: TicketSearchParams = {
  page: 1,
  pageSize: 15,
  sortBy: 'createdAt',
  sortDirection: 'desc',
};

export function SupportQueueDashboardView() {
  const { data: summary } = useSupportQueueDashboardQuery();
  const { data: categories } = useCategoriesQuery();
  const { data: agents } = useAgentsQuery(true);

  const [draftFilters, setDraftFilters] = useState<TicketSearchParams>(defaultFilters);
  const [appliedFilters, setAppliedFilters] = useState<TicketSearchParams>(defaultFilters);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [bulkAssigneeId, setBulkAssigneeId] = useState('');
  const [bulkError, setBulkError] = useState<string | null>(null);

  const { data: paged, isLoading, error } = useTicketSearchQuery(appliedFilters);
  const bulkAssign = useBulkAssignMutation();

  const selectedIds = useMemo(() => Array.from(selected), [selected]);

  const toggle = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const toggleAll = (checked: boolean) => {
    if (!paged?.items) return;
    setSelected(checked ? new Set(paged.items.map((t) => t.id)) : new Set());
  };

  const handleBulkAssign = async () => {
    if (selectedIds.length === 0) return;
    setBulkError(null);
    try {
      await bulkAssign.mutateAsync({
        ticketIds: selectedIds,
        assigneeId: bulkAssigneeId || null,
      });
      setSelected(new Set());
    } catch (e) {
      setBulkError(e instanceof Error ? e.message : 'Bulk assign failed');
    }
  };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Support queue</h1>
        <p className="text-sm text-slate-600">
          Filter, search, and manage the ticket queue. Updates refresh every 30 seconds.
        </p>
      </div>

      {summary && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
          <KpiCard label="Open" value={summary.openTicketsCount} accent="indigo" />
          <KpiCard label="Assigned to me" value={summary.assignedToMeCount} accent="emerald" />
          <KpiCard label="Unassigned" value={summary.unassignedCount} accent="amber" />
          <KpiCard label="Critical open" value={summary.criticalOpenCount} accent="red" />
          <KpiCard label="Delayed (3d+)" value={summary.delayedCount} accent="red" hint="Open > 3 days" />
        </div>
      )}

      {summary && (
        <section>
          <h2 className="mb-3 font-semibold text-slate-900">Queue by status</h2>
          <StatusOverview items={summary.statusOverview} />
        </section>
      )}

      <FilterPanel
        filters={draftFilters}
        onChange={setDraftFilters}
        onApply={() => setAppliedFilters({ ...draftFilters, page: 1 })}
        onReset={() => {
          setDraftFilters(defaultFilters);
          setAppliedFilters(defaultFilters);
        }}
        categories={categories}
        agents={agents}
      />

      {selectedIds.length > 0 && (
        <div className="flex flex-wrap items-end gap-3 rounded-xl border border-indigo-200 bg-indigo-50 p-4">
          <p className="text-sm font-medium text-indigo-900">{selectedIds.length} ticket(s) selected</p>
          <select
            value={bulkAssigneeId}
            onChange={(e) => setBulkAssigneeId(e.target.value)}
            className="rounded-lg border border-slate-300 px-3 py-2 text-sm"
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
            disabled={bulkAssign.isPending}
            onClick={() => void handleBulkAssign()}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
          >
            {bulkAssign.isPending ? 'Assigning...' : 'Bulk assign'}
          </button>
          {bulkError && <p className="text-sm text-red-700">{bulkError}</p>}
        </div>
      )}

      {error && (
        <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
          {error instanceof Error ? error.message : 'Search failed'}
        </p>
      )}

      <TicketTable
        tickets={paged?.items ?? []}
        isLoading={isLoading}
        selectable
        selectedIds={selected}
        onToggle={toggle}
        onToggleAll={toggleAll}
      />

      {paged && (
        <Pagination
          page={paged.page}
          totalPages={paged.totalPages}
          totalCount={paged.totalCount}
          onPageChange={(page) => setAppliedFilters((f) => ({ ...f, page }))}
        />
      )}
    </div>
  );
}
