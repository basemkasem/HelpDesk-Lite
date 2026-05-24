import { Link } from 'react-router-dom';
import { useManagerDashboardQuery } from '../../api/dashboardHooks';
import { KpiCard } from '../../components/dashboard/KpiCard';
import { SimpleBarChart } from '../../components/dashboard/SimpleBarChart';
import { StatusOverview } from '../../components/dashboard/StatusOverview';
import { TicketTable } from '../../components/dashboard/TicketTable';

export function ManagerDashboardView() {
  const { data, isLoading, error } = useManagerDashboardQuery();

  if (isLoading) {
    return <p className="text-slate-600">Loading manager dashboard...</p>;
  }

  if (error || !data) {
    return (
      <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
        {error instanceof Error ? error.message : 'Failed to load dashboard'}
      </p>
    );
  }

  const trendItems = data.resolutionTrend.map((p) => ({
    label: new Date(p.date).toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' }),
    count: p.resolvedCount,
  }));

  const workloadItems = data.teamWorkload.map((w) => ({
    label: w.assigneeName,
    count: w.openTicketCount,
  }));

  const agingItems = data.agingBuckets.map((b) => ({ label: b.label, count: b.count }));

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Manager dashboard</h1>
        <p className="text-sm text-slate-600">
          Workload, aging, and resolution trends across the support operation.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard label="Open tickets" value={data.openTicketsCount} accent="indigo" />
        <KpiCard label="Resolved" value={data.resolvedTicketsCount} accent="emerald" />
        <KpiCard label="Closed" value={data.closedTicketsCount} accent="slate" />
        <KpiCard label="Delayed (3d+)" value={data.delayedTicketsCount} accent="red" />
      </div>

      <section>
        <h2 className="mb-3 font-semibold text-slate-900">Status distribution</h2>
        <StatusOverview items={data.statusDistribution} />
      </section>

      <div className="grid gap-6 lg:grid-cols-2">
        <SimpleBarChart title="Team workload (open tickets)" items={workloadItems} />
        <SimpleBarChart title="Ticket aging (open)" items={agingItems} />
      </div>

      <SimpleBarChart title="Resolution trend (last 7 days)" items={trendItems} />

      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="font-semibold text-slate-900">Delayed tickets</h2>
          <Link to="/queue" className="text-sm text-indigo-600 hover:text-indigo-800">
            Open full queue
          </Link>
        </div>
        <TicketTable tickets={data.delayedTickets} showRequester />
      </section>
    </div>
  );
}
