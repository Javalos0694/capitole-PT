export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled';

export interface Booking {
  id: string;
  resourceId: string;
  customerName: string;
  customerEmail: string;
  start: string;
  end: string;
  status: BookingStatus;
  createdAtUtc: string;
  confirmedAtUtc?: string | null;
  cancelledAtUtc?: string | null;
  cancellationReason?: string | null;
}

export interface PagedBookings {
  items: Booking[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateBookingPayload {
  resourceId: string;
  customerName: string;
  customerEmail: string;
  start: string;
  end: string;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly errors?: Record<string, string[]>;

  constructor(problem: ProblemDetails, status: number) {
    super(problem.detail ?? problem.title ?? 'Request failed');
    this.status = status;
    this.code = problem.code;
    this.errors = problem.errors;
  }
}
