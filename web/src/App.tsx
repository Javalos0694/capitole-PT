import { Link, Navigate, Route, Routes } from 'react-router-dom';
import { BookingDetailsPage } from './pages/BookingDetailsPage';
import { BookingFormPage } from './pages/BookingFormPage';
import { BookingsPage } from './pages/BookingsPage';

export function App() {
  return (
    <div className="app-shell">
      <header className="topbar">
        <Link to="/bookings" className="brand">
          pt-capitole
        </Link>
        <nav>
          <Link to="/bookings">Reservas</Link>
          <Link to="/bookings/new">Nueva</Link>
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<Navigate to="/bookings" replace />} />
          <Route path="/bookings" element={<BookingsPage />} />
          <Route path="/bookings/new" element={<BookingFormPage />} />
          <Route path="/bookings/:id" element={<BookingDetailsPage />} />
        </Routes>
      </main>
    </div>
  );
}
