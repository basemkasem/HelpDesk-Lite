import type { TicketPriority, TicketStatus } from './ticket';

export interface StatusCount {
  status: string;
  count: number;
}

export interface DashboardActivity {
  id: string;
  ticketId: string;
  ticketNumber: string;
  ticketTitle: string;
  activityType: string;
  summary: string;
  actorName?: string | null;
  occurredAt: string;
}

export interface TicketListItem {
  id: string;
  ticketNumber: string;
  title: string;
  description: string;
  categoryId: string;
  categoryName: string;
  priority: TicketPriority;
  createdByUserId: string;
  createdByName: string;
  status: string;
  assigneeId?: string | null;
  assigneeName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  ageInDays: number;
  isDelayed: boolean;
}

export interface PagedTickets {
  items: TicketListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface TicketSearchParams {
  search?: string;
  status?: TicketStatus | '';
  priority?: TicketPriority | '';
  categoryId?: string;
  assigneeId?: string;
  assignedToMe?: boolean;
  unassignedOnly?: boolean;
  createdFrom?: string;
  createdTo?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export interface EmployeeDashboard {
  openTicketsCount: number;
  totalTicketsCount: number;
  statusOverview: StatusCount[];
  openTickets: TicketListItem[];
  recentActivity: DashboardActivity[];
}

export interface SupportQueueDashboard {
  openTicketsCount: number;
  assignedToMeCount: number;
  unassignedCount: number;
  criticalOpenCount: number;
  delayedCount: number;
  statusOverview: StatusCount[];
}

export interface AssigneeWorkload {
  assigneeId?: string | null;
  assigneeName: string;
  openTicketCount: number;
}

export interface ResolutionTrendPoint {
  date: string;
  resolvedCount: number;
}

export interface AgingBucket {
  label: string;
  count: number;
}

export interface ManagerDashboard {
  openTicketsCount: number;
  resolvedTicketsCount: number;
  closedTicketsCount: number;
  delayedTicketsCount: number;
  statusDistribution: StatusCount[];
  teamWorkload: AssigneeWorkload[];
  resolutionTrend: ResolutionTrendPoint[];
  agingBuckets: AgingBucket[];
  delayedTickets: TicketListItem[];
}

export interface BulkAssignRequest {
  ticketIds: string[];
  assigneeId: string | null;
}
