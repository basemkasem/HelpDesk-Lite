import { Link, NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const navClass = ({ isActive }: { isActive: boolean }) =>
  `text-sm font-medium ${isActive ? 'text-indigo-600' : 'text-slate-600 hover:text-slate-900'}`;

export function AppLayout() {
  const { user, logout } = useAuth();
  const isStaff = user?.role === 'SupportAgent' || user?.role === 'ManagerAdmin';

  return (
    <div className="min-h-screen">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-4 px-4 py-4">
          <div className="flex flex-wrap items-center gap-6">
            <Link to="/dashboard" className="text-lg font-semibold text-slate-900">
              HelpDesk Lite
            </Link>
            <nav className="flex flex-wrap gap-4">
              <NavLink to="/dashboard" className={navClass}>
                Dashboard
              </NavLink>
              {isStaff && (
                <NavLink to="/queue" className={navClass}>
                  Queue
                </NavLink>
              )}
              <NavLink to="/tickets" className={navClass}>
                Tickets
              </NavLink>
            </nav>
          </div>
          <div className="flex items-center gap-4">
            {user && (
              <span className="rounded-full bg-indigo-100 px-3 py-1 text-sm font-medium text-indigo-800">
                {user.role}
              </span>
            )}
            <span className="text-sm text-slate-600">{user?.fullName}</span>
            <button
              type="button"
              onClick={() => void logout()}
              className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm hover:bg-slate-50"
            >
              Logout
            </button>
          </div>
        </div>
      </header>
      <main className="mx-auto max-w-7xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
