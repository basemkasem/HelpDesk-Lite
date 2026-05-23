export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type TicketStatus =
  | 'New'
  | 'Assigned'
  | 'InProgress'
  | 'WaitingForUser'
  | 'Resolved'
  | 'Closed';

export interface Category {
  id: string;
  name: string;
  description?: string;
}

export interface TicketAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
}

export interface Ticket {
  id: string;
  ticketNumber: string;
  title: string;
  description: string;
  categoryId: string;
  categoryName: string;
  priority: TicketPriority;
  createdByUserId: string;
  status: string;
  assigneeId?: string | null;
  assigneeName?: string | null;
  createdAt: string;
  attachments: TicketAttachment[];
}

export interface TicketDetail extends Ticket {
  createdByName: string;
  updatedAt?: string | null;
  allowedNextStatuses: string[];
}

export interface TicketActivity {
  id: string;
  activityType: string;
  summary: string;
  detail?: string | null;
  actorId?: string | null;
  actorName?: string | null;
  occurredAt: string;
  isInternal: boolean;
}

export interface TicketComment {
  id: string;
  ticketId: string;
  authorId: string;
  authorName: string;
  comment: string;
  isInternal: boolean;
  createdAt: string;
}

export interface AgentUser {
  id: string;
  fullName: string;
  email: string;
}

export interface CreateTicketForm {
  title: string;
  categoryId: string;
  description: string;
  priority: TicketPriority;
  attachments: File[];
}

export interface UpdateTicketStatusRequest {
  status: string;
}

export interface AssignTicketRequest {
  assigneeId: string | null;
}

export interface CreateTicketCommentRequest {
  comment: string;
  isInternal: boolean;
}

export interface KnowledgeBaseSuggestion {
  id: string;
  title: string;
  summary: string;
  url?: string;
  relevanceScore: number;
}
