import { apiRequest } from './httpClient';
import type { Booking, CreateBookingPayload, PagedBookings } from './types';

export interface BookingListParams {
  resourceId?: string;
  customerName?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
  sort?: string;
}

function toQuery(params: BookingListParams): string {
  const q = new URLSearchParams();
  if (params.resourceId) q.set('resourceId', params.resourceId);
  if (params.customerName) q.set('customerName', params.customerName);
  if (params.from) q.set('from', params.from);
  if (params.to) q.set('to', params.to);
  if (params.page) q.set('page', String(params.page));
  if (params.pageSize) q.set('pageSize', String(params.pageSize));
  if (params.sort) q.set('sort', params.sort);
  const value = q.toString();
  return value ? `?${value}` : '';
}

export const bookingsApi = {
  list(params: BookingListParams) {
    return apiRequest<PagedBookings>(`/api/bookings${toQuery(params)}`);
  },
  getById(id: string) {
    return apiRequest<Booking>(`/api/bookings/${id}`);
  },
  create(payload: CreateBookingPayload) {
    return apiRequest<Booking>('/api/bookings', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },
  confirm(id: string) {
    return apiRequest<Booking>(`/api/bookings/${id}/confirm`, { method: 'POST' });
  },
  cancel(id: string, reason?: string) {
    return apiRequest<Booking>(`/api/bookings/${id}/cancel`, {
      method: 'POST',
      body: JSON.stringify({ reason }),
    });
  },
};
