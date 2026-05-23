import { isAxiosError } from 'axios';
import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCategoriesQuery, useCreateTicketMutation, useKbSuggestionsQuery } from '../api/ticketHooks';
import type { ApiResponse } from '../types/auth';
import type { Ticket, TicketPriority } from '../types/ticket';

const PRIORITIES: { value: TicketPriority; label: string }[] = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' },
];

const MAX_FILE_SIZE_MB = 5;
const MAX_FILES = 5;

function useDebounce(value: string, delayMs: number) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs);
    return () => clearTimeout(timer);
  }, [value, delayMs]);
  return debounced;
}

export function CreateTicketPage() {
  const navigate = useNavigate();
  const { data: categories, isLoading: categoriesLoading } = useCategoriesQuery();
  const createMutation = useCreateTicketMutation();

  const [title, setTitle] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<TicketPriority>('Medium');
  const [attachments, setAttachments] = useState<File[]>([]);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  const debouncedDescription = useDebounce(description, 400);
  const { data: suggestions, isFetching: suggestionsLoading } = useKbSuggestionsQuery(debouncedDescription);

  useEffect(() => {
    if (categories?.length && !categoryId) {
      setCategoryId(categories[0].id);
    }
  }, [categories, categoryId]);

  const totalAttachmentSize = useMemo(
    () => attachments.reduce((sum, f) => sum + f.size, 0),
    [attachments],
  );

  const validateClient = (): boolean => {
    const errors: Record<string, string> = {};
    if (!title.trim()) errors.title = 'Subject is required.';
    if (!categoryId) errors.categoryId = 'Category is required.';
    if (!description.trim()) errors.description = 'Description is required.';
    else if (description.trim().length < 10) errors.description = 'Description must be at least 10 characters.';
    if (attachments.length > MAX_FILES) errors.attachments = `Maximum ${MAX_FILES} files allowed.`;
    const oversized = attachments.find((f) => f.size > MAX_FILE_SIZE_MB * 1024 * 1024);
    if (oversized) errors.attachments = `Each file must be under ${MAX_FILE_SIZE_MB} MB.`;
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleFileChange = (fileList: FileList | null) => {
    if (!fileList) return;
    setAttachments(Array.from(fileList));
    setFieldErrors((prev) => ({ ...prev, attachments: '' }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSubmitError(null);
    if (!validateClient()) return;

    try {
      const ticket = await createMutation.mutateAsync({
        title: title.trim(),
        categoryId,
        description: description.trim(),
        priority,
        attachments,
      });
      navigate(`/tickets/confirmation/${ticket.id}`, { state: { ticket } });
    } catch (err) {
      if (isAxiosError(err)) {
        const apiError = err.response?.data as ApiResponse<Ticket> | undefined;
        if (apiError?.error?.errors) {
          const mapped: Record<string, string> = {};
          for (const [key, messages] of Object.entries(apiError.error.errors)) {
            mapped[key.charAt(0).toLowerCase() + key.slice(1)] = messages[0] ?? 'Invalid value';
          }
          setFieldErrors(mapped);
        }
        const status = err.response?.status;
        const fallback =
          status === 403
            ? 'You do not have permission to submit tickets. Try logging in again.'
            : `Failed to submit ticket${status ? ` (${status})` : ''}. Please try again.`;
        setSubmitError(apiError?.error?.message ?? fallback);
      } else {
        setSubmitError(err instanceof Error ? err.message : 'Failed to submit ticket.');
      }
    }
  };

  return (
    <div className="mx-auto max-w-2xl">
      <div className="mb-6">
        <Link to="/tickets" className="text-sm text-indigo-600 hover:text-indigo-800">
          ← Back to tickets
        </Link>
        <h1 className="mt-2 text-2xl font-bold text-slate-900">Submit a support ticket</h1>
        <p className="text-sm text-slate-600">Describe your issue and we will route it to the right team.</p>
      </div>

      <form onSubmit={(e) => void handleSubmit(e)} className="space-y-5 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <div>
          <label htmlFor="title" className="mb-1 block text-sm font-medium text-slate-700">
            Subject <span className="text-red-500">*</span>
          </label>
          <input
            id="title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
            placeholder="Brief summary of the issue"
          />
          {fieldErrors.title && <p className="mt-1 text-sm text-red-600">{fieldErrors.title}</p>}
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="category" className="mb-1 block text-sm font-medium text-slate-700">
              Category <span className="text-red-500">*</span>
            </label>
            <select
              id="category"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              disabled={categoriesLoading}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
            >
              {categories?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
            {fieldErrors.categoryId && <p className="mt-1 text-sm text-red-600">{fieldErrors.categoryId}</p>}
          </div>

          <div>
            <label htmlFor="priority" className="mb-1 block text-sm font-medium text-slate-700">
              Priority <span className="text-red-500">*</span>
            </label>
            <select
              id="priority"
              value={priority}
              onChange={(e) => setPriority(e.target.value as TicketPriority)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
            >
              {PRIORITIES.map((p) => (
                <option key={p.value} value={p.value}>
                  {p.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label htmlFor="description" className="mb-1 block text-sm font-medium text-slate-700">
            Description <span className="text-red-500">*</span>
          </label>
          <textarea
            id="description"
            rows={6}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200"
            placeholder="Include steps to reproduce, error messages, and when the issue started..."
          />
          {fieldErrors.description && <p className="mt-1 text-sm text-red-600">{fieldErrors.description}</p>}

          {(suggestionsLoading || (suggestions && suggestions.length > 0)) && debouncedDescription.length >= 3 && (
            <div className="mt-3 rounded-lg border border-indigo-100 bg-indigo-50 p-3">
              <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-indigo-800">
                Suggested knowledge base articles
              </p>
              {suggestionsLoading && <p className="text-sm text-indigo-700">Searching...</p>}
              <ul className="space-y-2">
                {suggestions?.map((s) => (
                  <li key={s.id} className="rounded-md bg-white/80 p-2 text-sm">
                    <p className="font-medium text-slate-900">{s.title}</p>
                    <p className="text-slate-600">{s.summary}</p>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>

        <div>
          <label htmlFor="attachments" className="mb-1 block text-sm font-medium text-slate-700">
            Attachments <span className="font-normal text-slate-500">(optional)</span>
          </label>
          <input
            id="attachments"
            type="file"
            multiple
            accept=".pdf,.png,.jpg,.jpeg,.txt,.docx"
            onChange={(e) => handleFileChange(e.target.files)}
            className="block w-full text-sm text-slate-600 file:mr-3 file:rounded-lg file:border-0 file:bg-indigo-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-indigo-700"
          />
          <p className="mt-1 text-xs text-slate-500">
            Up to {MAX_FILES} files, {MAX_FILE_SIZE_MB} MB each. PDF, images, TXT, DOCX.
          </p>
          {attachments.length > 0 && (
            <ul className="mt-2 space-y-1 text-sm text-slate-600">
              {attachments.map((f) => (
                <li key={`${f.name}-${f.size}`}>
                  {f.name} ({(f.size / 1024).toFixed(1)} KB)
                </li>
              ))}
            </ul>
          )}
          {fieldErrors.attachments && <p className="mt-1 text-sm text-red-600">{fieldErrors.attachments}</p>}
          {totalAttachmentSize > MAX_FILE_SIZE_MB * 1024 * 1024 * MAX_FILES && (
            <p className="mt-1 text-sm text-amber-700">Total upload size is large; submission may fail.</p>
          )}
        </div>

        {submitError && (
          <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
            {submitError}
          </p>
        )}

        <button
          type="submit"
          disabled={createMutation.isPending}
          className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 font-medium text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {createMutation.isPending ? 'Submitting...' : 'Submit ticket'}
        </button>
      </form>
    </div>
  );
}
