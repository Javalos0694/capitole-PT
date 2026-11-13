import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { z } from 'zod';
import { bookingsApi } from '../api/bookingsApi';
import type { ApiError } from '../api/types';
import { ErrorAlert } from '../components/ErrorAlert';
import { useToast } from '../ui/ToastContext';

const schema = z
  .object({
    resourceId: z.string().min(1, 'El recurso es obligatorio'),
    customerName: z.string().min(1, 'El nombre es obligatorio').max(100, 'Máximo 100 caracteres'),
    customerEmail: z.string().email('El formato del correo no es válido'),
    startDate: z.string().min(1, 'La fecha de inicio es obligatoria'),
    startTime: z.string().min(1, 'La hora de inicio es obligatoria'),
    endDate: z.string().min(1, 'La fecha de fin es obligatoria'),
    endTime: z.string().min(1, 'La hora de fin es obligatoria'),
  })
  .superRefine((value, ctx) => {
    const start = combineDateTime(value.startDate, value.startTime);
    const end = combineDateTime(value.endDate, value.endTime);

    if (!start || Number.isNaN(start.getTime())) {
      ctx.addIssue({ code: 'custom', path: ['startDate'], message: 'Fecha de inicio inválida' });
      return;
    }

    if (!end || Number.isNaN(end.getTime())) {
      ctx.addIssue({ code: 'custom', path: ['endDate'], message: 'Fecha de fin inválida' });
      return;
    }

    if (!(start < end)) {
      ctx.addIssue({
        code: 'custom',
        path: ['endTime'],
        message: 'El fin debe ser posterior al inicio',
      });
    }

    if (!(start > new Date())) {
      ctx.addIssue({
        code: 'custom',
        path: ['startTime'],
        message: 'La fecha de inicio debe ser futura',
      });
    }
  });

type FormValues = z.infer<typeof schema>;

export function BookingFormPage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const defaults = defaultSchedule();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: defaults,
  });

  const mutation = useMutation({
    mutationFn: bookingsApi.create,
    onSuccess: (booking) => {
      showToast('Reserva creada');
      navigate(`/bookings/${booking.id}`);
    },
    onError: (error: ApiError) => {
      if (error.errors) {
        Object.entries(error.errors).forEach(([field, messages]) => {
          const key = mapServerField(field);
          if (key) setError(key, { message: messages[0] });
        });
      }
    },
  });

  const onSubmit = handleSubmit((values) => {
    const start = combineDateTime(values.startDate, values.startTime)!;
    const end = combineDateTime(values.endDate, values.endTime)!;

    mutation.mutate({
      resourceId: values.resourceId.trim(),
      customerName: values.customerName.trim(),
      customerEmail: values.customerEmail.trim(),
      start: start.toISOString(),
      end: end.toISOString(),
    });
  });

  return (
    <section className="form-page">
      <div className="detail-toolbar">
        <Link to="/bookings" className="back-link">
          ← Volver al listado
        </Link>
      </div>

      <article className="form-card">
        <header className="form-card-header">
          <div>
            <p className="eyebrow">Nueva</p>
            <h1>Crear reserva</h1>
            <p className="muted">Define el recurso, el cliente y el horario.</p>
          </div>
        </header>

        <ErrorAlert error={(mutation.error as ApiError) ?? null} />

        <form className="create-form" onSubmit={onSubmit}>
          <section className="form-section">
            <h2>Recurso</h2>
            <label className="form-field">
              <span>Identificador del recurso</span>
              <input {...register('resourceId')} placeholder="Ej. Room-A, Desk-12" />
              {errors.resourceId && <span className="field-error">{errors.resourceId.message}</span>}
            </label>
          </section>

          <section className="form-section">
            <h2>Cliente</h2>
            <div className="form-grid-2">
              <label className="form-field">
                <span>Nombre</span>
                <input {...register('customerName')} placeholder="Nombre completo" />
                {errors.customerName && (
                  <span className="field-error">{errors.customerName.message}</span>
                )}
              </label>
              <label className="form-field">
                <span>Correo</span>
                <input
                  type="email"
                  {...register('customerEmail')}
                  placeholder="correo@ejemplo.com"
                />
                {errors.customerEmail && (
                  <span className="field-error">{errors.customerEmail.message}</span>
                )}
              </label>
            </div>
          </section>

          <section className="form-section">
            <h2>Horario</h2>
            <div className="schedule-grid">
              <div className="schedule-block">
                <p className="schedule-label">Inicio</p>
                <div className="form-grid-2">
                  <label className="form-field">
                    <span>Fecha</span>
                    <input type="date" {...register('startDate')} />
                    {errors.startDate && (
                      <span className="field-error">{errors.startDate.message}</span>
                    )}
                  </label>
                  <label className="form-field">
                    <span>Hora</span>
                    <input type="time" {...register('startTime')} />
                    {errors.startTime && (
                      <span className="field-error">{errors.startTime.message}</span>
                    )}
                  </label>
                </div>
              </div>

              <div className="schedule-block">
                <p className="schedule-label">Fin</p>
                <div className="form-grid-2">
                  <label className="form-field">
                    <span>Fecha</span>
                    <input type="date" {...register('endDate')} />
                    {errors.endDate && (
                      <span className="field-error">{errors.endDate.message}</span>
                    )}
                  </label>
                  <label className="form-field">
                    <span>Hora</span>
                    <input type="time" {...register('endTime')} />
                    {errors.endTime && (
                      <span className="field-error">{errors.endTime.message}</span>
                    )}
                  </label>
                </div>
              </div>
            </div>
          </section>

          <div className="form-actions">
            <Link to="/bookings" className="button ghost">
              Cancelar
            </Link>
            <button className="button" type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? 'Creando…' : 'Crear reserva'}
            </button>
          </div>
        </form>
      </article>
    </section>
  );
}

function combineDateTime(date: string, time: string) {
  if (!date || !time) return null;
  return new Date(`${date}T${time}:00`);
}

function defaultSchedule(): Partial<FormValues> {
  const start = new Date();
  start.setMinutes(0, 0, 0);
  start.setHours(start.getHours() + 1);

  const end = new Date(start);
  end.setHours(end.getHours() + 1);

  return {
    startDate: toDateValue(start),
    startTime: toTimeValue(start),
    endDate: toDateValue(end),
    endTime: toTimeValue(end),
  };
}

function toDateValue(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function toTimeValue(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function mapServerField(field: string): keyof FormValues | null {
  const normalized = field.charAt(0).toLowerCase() + field.slice(1);
  switch (normalized) {
    case 'resourceId':
    case 'customerName':
    case 'customerEmail':
      return normalized;
    case 'start':
      return 'startTime';
    case 'end':
      return 'endTime';
    default:
      return null;
  }
}
