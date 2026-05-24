import { useAuth } from '../auth/AuthContext';
import { EmployeeDashboardView } from './dashboard/EmployeeDashboardView';
import { ManagerDashboardView } from './dashboard/ManagerDashboardView';
import { SupportQueueDashboardView } from './dashboard/SupportQueueDashboardView';

export function DashboardPage() {
  const { user } = useAuth();

  if (user?.role === 'ManagerAdmin') {
    return <ManagerDashboardView />;
  }

  if (user?.role === 'SupportAgent') {
    return <SupportQueueDashboardView />;
  }

  return <EmployeeDashboardView />;
}
