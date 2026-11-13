import type { ApiError } from '../api/types';

const codeMessages: Record<string, string> = {
  'booking.overlap': 'Ya existe una reserva confirmada que se solapa con este recurso y horario.',
  'booking.invalid_transition': 'Ese cambio de estado no está permitido.',
  'booking.start_in_past': 'La fecha de inicio debe ser futura.',
  'booking.invalid_range': 'La fecha de inicio debe ser anterior a la de fin.',
  'booking.invalid_email': 'El formato del correo no es válido.',
  'booking.invalid_customer': 'Los datos del cliente no son válidos.',
  'booking.concurrency_conflict': 'La reserva fue modificada por otra operación. Inténtalo de nuevo.',
  'validation.failed': 'Revisa los campos del formulario.',
  not_found: 'No se encontró la reserva.',
  'server.error': 'Ocurrió un error inesperado. Inténtalo de nuevo.',
};

export function ErrorAlert({ error }: { error: ApiError | Error | null }) {
  if (!error) return null;

  const apiError = error as ApiError;
  const message =
    (apiError.code && codeMessages[apiError.code]) ||
    apiError.message ||
    'Algo salió mal. Inténtalo de nuevo.';

  return (
    <div className="alert" role="alert">
      <strong>{message}</strong>
    </div>
  );
}
