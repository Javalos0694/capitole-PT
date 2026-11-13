import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { useEffect } from 'react';

type BookingChangedPayload = {
  bookingId: string;
  status: string;
  changeType: string;
};

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080';

export function BookingRealtimeSync() {
  const queryClient = useQueryClient();

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/bookings`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('BookingChanged', (payload: BookingChangedPayload) => {
      void queryClient.invalidateQueries({ queryKey: ['bookings'] });
      if (payload?.bookingId) {
        void queryClient.invalidateQueries({ queryKey: ['booking', payload.bookingId] });
      }
    });

    const start = async () => {
      if (connection.state === HubConnectionState.Disconnected) {
        await connection.start();
      }
    };

    void start();

    return () => {
      void connection.stop();
    };
  }, [queryClient]);

  return null;
}
