import { StatusBadge } from './StatusBadge';

export function StatusOverview({ items }: { items: { status: string; count: number }[] }) {
  if (items.length === 0) {
    return <p className="text-sm text-slate-500">No tickets yet.</p>;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {items.map((item) => (
        <div
          key={item.status}
          className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm"
        >
          <StatusBadge status={item.status} />
          <span className="font-semibold text-slate-900">{item.count}</span>
        </div>
      ))}
    </div>
  );
}
