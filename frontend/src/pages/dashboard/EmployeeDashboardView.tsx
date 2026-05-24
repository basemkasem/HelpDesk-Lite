import { Link } from 'react-router-dom';
import { useEmployeeDashboardQuery } from '../../api/dashboardHooks';
import { KpiCard } from '../../components/dashboard/KpiCard';
import { PriorityBadge } from '../../components/dashboard/PriorityBadge';
import { StatusBadge } from '../../components/dashboard/StatusBadge';
import { StatusOverview } from '../../components/dashboard/StatusOverview';

export function EmployeeDashboardView() {
  const { data, isLoading, error } = useEmployeeDashboardQuery();

  if (isLoading) {
    return <p className="text-slate-600">Loading your dashboard...</p>;
  }

  if (error || !data) {
    return (
      <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
        {error instanceof Error ? error.message : 'Failed to load dashboard'}
      </p>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">My dashboard</h1>
          <p className="text-sm text-slate-600">Track your submitted tickets and recent updates.</p>
        </div>
        <Link
          to="/tickets/new"
          className="inline-flex justify-center rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700"
        >
          + New ticket
        </Link>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <KpiCard label="Open tickets" value={data.openTicketsCount} accent="indigo" />
        <KpiCard label="Total submitted" value={data.totalTicketsCount} accent="slate" />
        <KpiCard
          label="In progress"
          value={data.statusOverview.find((s) => s.status === 'InProgress')?.count ?? 0}
          accent="amber"
        />
      </div>

      <section>
        <h2 className="mb-3 font-semibold text-slate-900">Status overview</h2>
        <StatusOverview items={data.statusOverview} />
      </section>

      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="font-semibold text-slate-900">My open tickets</h2>
          <Link to="/tickets" className="text-sm text-indigo-600 hover:text-indigo-800">
            View all
          </Link>
        </div>
        {data.openTickets.length === 0 ? (
          <p className="text-sm text-slate-600">No open tickets. You are all caught up.</p>
        ) : (
          <ul className="space-y-3">
            {data.openTickets.map((ticket) => (
              <li key={ticket.id}>
                <Link
                  to={`/tickets/${ticket.id}`}
                  className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm hover:border-indigo-300"
                >
                  <div>
                    <p className="font-mono text-xs text-slate-500">{ticket.ticketNumber}</p>
                    <p className="font-medium text-slate-900">{ticket.title}</p>
                    <p className="text-xs text-slate-500">{ticket.categoryName}</p>
                  </div>
                  <div className="flex gap-2">
                    <StatusBadge status={ticket.status} />
                    <PriorityBadge priority={ticket.priority} />
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section>
        <h2 className="mb-3 font-semibold text-slate-900">Recent activity</h2>
        {data.recentActivity.length === 0 ? (
          <p className="text-sm text-slate-600">No recent updates.</p>
        ) : (
          <ul className="space-y-2">
            {data.recentActivity.map((activity) => (
              <li
                key={`${activity.activityType}-${activity.id}`}
                className="rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm"
              >
                <Link to={`/tickets/${activity.ticketId}`} className="font-medium text-indigo-600">
                  {activity.ticketNumber}
                </Link>
                <span className="text-slate-700"> — {activity.summary}</span>
                <p className="mt-1 text-xs text-slate-500">
                  {activity.actorName && `${activity.actorName} · `}
                  {new Date(activity.occurredAt).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
