import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { bookingsApi } from '../api/bookingsApi';
import type { ApiError, BookingStatus } from '../api/types';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ErrorAlert } from '../components/ErrorAlert';
import { StatusBadge } from '../components/StatusBadge';
import { useToast } from '../ui/ToastContext';

type PendingAction = 'confirm' | 'cancel' | null;

export function BookingDetailsPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [actionError, setActionError] = useState<ApiError | null>(null);
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);

  const query = useQuery({
    queryKey: ['booking', id],
    queryFn: () => bookingsApi.getById(id),
    enabled: Boolean(id),
  });

  const goToList = async (message: string) => {
    setActionError(null);
    setPendingAction(null);
    await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    showToast(message);
    navigate('/bookings');
  };

  const confirmMutation = useMutation({
    mutationFn: () => bookingsApi.confirm(id),
    onSuccess: () => {
      void goToList('Reserva confirmada');
    },
    onError: (error: ApiError) => {
      setPendingAction(null);
      setActionError(error);
    },
  });

  const cancelMutation = useMutation({
    mutationFn: () => bookingsApi.cancel(id),
    onSuccess: () => {
      void goToList('Reserva cancelada');
    },
    onError: (error: ApiError) => {
      setPendingAction(null);
      setActionError(error);
    },
  });

  if (query.isLoading) return <div className="state">Cargando detalle…</div>;
  if (query.error) return <ErrorAlert error={query.error as ApiError} />;
  if (!query.data) return <div className="state">Reserva no encontrada</div>;

  const booking = query.data;
  const busy = confirmMutation.isPending || cancelMutation.isPending;

  const dialog =
    pendingAction === 'confirm'
      ? {
          title: 'Confirmar reserva',
          message: '¿Seguro que quieres confirmar esta reserva?',
          confirmLabel: 'Sí, confirmar',
          onConfirm: () => confirmMutation.mutate(),
        }
      : pendingAction === 'cancel'
        ? {
            title: 'Cancelar reserva',
            message: '¿Seguro que quieres cancelar esta reserva?',
            confirmLabel: 'Sí, cancelar',
            onConfirm: () => cancelMutation.mutate(),
          }
        : null;

  return (
    <section className="detail-page">
      <div className="detail-toolbar">
        <Link to="/bookings" className="back-link">
          ← Volver al listado
        </Link>
      </div>

      <ErrorAlert error={actionError} />

      <article className={`booking-card status-${booking.status.toLowerCase()}`}>
        <header className="booking-card-header">
          <div>
            <p className="eyebrow">Reserva</p>
            <h1>{booking.resourceId}</h1>
            <p className="muted">{booking.customerName} · {booking.customerEmail}</p>
          </div>
          <StatusBadge status={booking.status} />
        </header>

        <div className="booking-card-body">
          <div className="booking-meta">
            <div>
              <span>Inicio</span>
              <strong>{fmt(booking.start)}</strong>
            </div>
            <div>
              <span>Fin</span>
              <strong>{fmt(booking.end)}</strong>
            </div>
          </div>

          <div className="booking-timeline">
            <h2>Historial</h2>
            <ul>
              <li>
                <span>Creada</span>
                <strong>{fmt(booking.createdAtUtc)}</strong>
              </li>
              <li>
                <span>Confirmada</span>
                <strong>{fmt(booking.confirmedAtUtc)}</strong>
              </li>
              <li>
                <span>Cancelada</span>
                <strong>{fmt(booking.cancelledAtUtc)}</strong>
              </li>
            </ul>
          </div>

          <p className="booking-id">ID: {booking.id}</p>
        </div>

        <footer className="booking-card-actions">
          {booking.status === 'Pending' && (
            <button
              className="button"
              disabled={busy}
              onClick={() => {
                setActionError(null);
                setPendingAction('confirm');
              }}
            >
              Confirmar
            </button>
          )}
          {(booking.status === 'Pending' || booking.status === 'Confirmed') && (
            <button
              className="button secondary"
              disabled={busy}
              onClick={() => {
                setActionError(null);
                setPendingAction('cancel');
              }}
            >
              Cancelar
            </button>
          )}
          {isTerminal(booking.status) && (
            <p className="muted">Esta reserva ya no admite cambios de estado.</p>
          )}
        </footer>
      </article>

      <ConfirmDialog
        open={dialog !== null}
        title={dialog?.title ?? ''}
        message={dialog?.message ?? ''}
        confirmLabel={dialog?.confirmLabel}
        busy={busy}
        onConfirm={() => dialog?.onConfirm()}
        onCancel={() => setPendingAction(null)}
      />
    </section>
  );
}

function isTerminal(status: BookingStatus) {
  return status === 'Cancelled';
}

function fmt(value?: string | null) {
  return value ? new Date(value).toLocaleString('es-MX') : '—';
}
