import { useQuery } from '@tanstack/react-query';
import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { bookingsApi } from '../api/bookingsApi';
import type { ApiError, Booking } from '../api/types';
import { ErrorAlert } from '../components/ErrorAlert';
import { GenericTable } from '../components/GenericTable';
import { StatusBadge } from '../components/StatusBadge';

export function BookingsPage() {
  const [params, setParams] = useSearchParams();
  const navigate = useNavigate();

  const filters = useMemo(
    () => ({
      resourceId: params.get('resourceId') ?? undefined,
      customerName: params.get('customerName') ?? undefined,
      from: params.get('from') ?? undefined,
      to: params.get('to') ?? undefined,
      page: Number(params.get('page') ?? '1'),
      pageSize: Number(params.get('pageSize') ?? '10'),
      sort: params.get('sort') ?? 'created_desc',
    }),
    [params],
  );

  const [resourceDraft, setResourceDraft] = useState(filters.resourceId ?? '');
  const [customerDraft, setCustomerDraft] = useState(filters.customerName ?? '');

  useEffect(() => {
    setResourceDraft(filters.resourceId ?? '');
    setCustomerDraft(filters.customerName ?? '');
  }, [filters.resourceId, filters.customerName]);

  useEffect(() => {
    const handle = window.setTimeout(() => {
      updateFilter('resourceId', resourceDraft.trim());
    }, 350);
    return () => window.clearTimeout(handle);
  }, [resourceDraft]);

  useEffect(() => {
    const handle = window.setTimeout(() => {
      updateFilter('customerName', customerDraft.trim());
    }, 350);
    return () => window.clearTimeout(handle);
  }, [customerDraft]);

  const query = useQuery({
    queryKey: ['bookings', filters],
    queryFn: () => bookingsApi.list(filters),
  });

  const updateFilter = (key: string, value: string) => {
    const current = params.get(key) ?? '';
    if (current === value) return;

    const next = new URLSearchParams(params);
    if (value) next.set(key, value);
    else next.delete(key);
    if (key !== 'page') next.set('page', '1');
    setParams(next);
  };

  const clearFilters = () => {
    setResourceDraft('');
    setCustomerDraft('');
    setParams(new URLSearchParams({ sort: 'created_desc' }));
  };

  const hasActiveFilters = Boolean(
    filters.resourceId || filters.customerName || filters.from || filters.to,
  );

  return (
    <section className="panel list-panel">
      <div className="panel-header">
        <div>
          <h1>Reservas</h1>
          <p className="muted">Ordenadas de la más nueva a la más antigua.</p>
        </div>
        <Link className="button" to="/bookings/new">
          Nueva reserva
        </Link>
      </div>

      <div className="filters-card">
        <div className="filters filters-grid">
          <label className="filter-field">
            <span>Recurso</span>
            <input
              value={resourceDraft}
              onChange={(e) => setResourceDraft(e.target.value)}
              placeholder="Ej. Room-A"
            />
          </label>

          <label className="filter-field filter-search">
            <span>Cliente</span>
            <input
              value={customerDraft}
              onChange={(e) => setCustomerDraft(e.target.value)}
              placeholder="Buscar por nombre"
            />
          </label>

          <label className="filter-field">
            <span>Desde</span>
            <input
              type="date"
              value={toDateInput(filters.from)}
              onChange={(e) => updateFilter('from', fromDateStart(e.target.value))}
            />
          </label>

          <label className="filter-field">
            <span>Hasta</span>
            <input
              type="date"
              value={toDateInput(filters.to)}
              onChange={(e) => updateFilter('to', toDateEnd(e.target.value))}
            />
          </label>
        </div>

        {hasActiveFilters && (
          <div className="filter-actions">
            <button type="button" className="button ghost" onClick={clearFilters}>
              Limpiar filtros
            </button>
          </div>
        )}
      </div>

      <ErrorAlert error={(query.error as ApiError) ?? null} />

      <GenericTable
        className="booking-list-table"
        items={query.data?.items ?? []}
        loading={query.isLoading}
        emptyText="No hay reservas para estos filtros"
        getRowKey={(item) => item.id}
        onRowClick={(item) => navigate(`/bookings/${item.id}`)}
        header={
          <tr>
            <th>Reserva</th>
            <th>Horario</th>
            <th>Estado</th>
          </tr>
        }
        renderRow={(item: Booking) => (
          <>
            <td>
              <div className={`list-card-accent status-${item.status.toLowerCase()}`} />
              <div className="list-card-main">
                <strong>{item.resourceId}</strong>
                <span>
                  {item.customerName} · {item.customerEmail}
                </span>
              </div>
            </td>
            <td>
              <div className="list-card-schedule">
                <span>Inicio: {formatLocal(item.start)}</span>
                <span>Fin: {formatLocal(item.end)}</span>
              </div>
            </td>
            <td>
              <StatusBadge status={item.status} />
            </td>
          </>
        )}
      />

      {query.data && (
        <div className="pager">
          <button
            disabled={filters.page <= 1}
            onClick={() => updateFilter('page', String(filters.page - 1))}
          >
            Anterior
          </button>
          <span>
            Página {query.data.page} · {query.data.totalCount} total
          </span>
          <button
            disabled={filters.page * filters.pageSize >= query.data.totalCount}
            onClick={() => updateFilter('page', String(filters.page + 1))}
          >
            Siguiente
          </button>
        </div>
      )}
    </section>
  );
}

function formatLocal(value: string) {
  return new Date(value).toLocaleString('es-MX', {
    dateStyle: 'medium',
    timeStyle: 'short',
  });
}

function toDateInput(value?: string) {
  if (!value) return '';
  const date = new Date(value);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function fromDateStart(value: string) {
  if (!value) return '';
  return new Date(`${value}T00:00:00`).toISOString();
}

function toDateEnd(value: string) {
  if (!value) return '';
  return new Date(`${value}T23:59:59.999`).toISOString();
}
