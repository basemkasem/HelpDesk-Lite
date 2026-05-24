const priorityColors: Record<string, string> = {
  Low: 'bg-slate-100 text-slate-700',
  Medium: 'bg-blue-100 text-blue-800',
  High: 'bg-amber-100 text-amber-800',
  Critical: 'bg-red-100 text-red-800',
};

export function PriorityBadge({ priority }: { priority: string }) {
  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${priorityColors[priority] ?? 'bg-slate-100 text-slate-700'}`}
    >
      {priority}
    </span>
  );
}
