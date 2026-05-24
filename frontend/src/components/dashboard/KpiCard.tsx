export function KpiCard({
  label,
  value,
  hint,
  accent = 'indigo',
}: {
  label: string;
  value: number | string;
  hint?: string;
  accent?: 'indigo' | 'amber' | 'emerald' | 'red' | 'slate';
}) {
  const accents: Record<string, string> = {
    indigo: 'border-indigo-200 bg-indigo-50 text-indigo-900',
    amber: 'border-amber-200 bg-amber-50 text-amber-900',
    emerald: 'border-emerald-200 bg-emerald-50 text-emerald-900',
    red: 'border-red-200 bg-red-50 text-red-900',
    slate: 'border-slate-200 bg-slate-50 text-slate-900',
  };

  return (
    <div className={`rounded-xl border p-4 shadow-sm ${accents[accent]}`}>
      <p className="text-sm font-medium opacity-80">{label}</p>
      <p className="mt-1 text-3xl font-bold">{value}</p>
      {hint && <p className="mt-1 text-xs opacity-70">{hint}</p>}
    </div>
  );
}
