const statusColors: Record<string, string> = {
  New: 'bg-slate-100 text-slate-800',
  Assigned: 'bg-purple-100 text-purple-800',
  InProgress: 'bg-blue-100 text-blue-800',
  WaitingForUser: 'bg-amber-100 text-amber-800',
  Resolved: 'bg-emerald-100 text-emerald-800',
  Closed: 'bg-gray-200 text-gray-700',
};

export function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusColors[status] ?? 'bg-slate-100 text-slate-700'}`}
    >
      {status}
    </span>
  );
}
