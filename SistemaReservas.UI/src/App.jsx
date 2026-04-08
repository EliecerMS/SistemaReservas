import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Navbar from './components/Navbar';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ZonesPage from './pages/ZonesPage';
import ZoneDetailsPage from './pages/ZoneDetailsPage';
import ReservePage from './pages/ReservePage';
import MyReservationsPage from './pages/MyReservationsPage';
import AdminPage from './pages/AdminPage';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Navbar />
        <main>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/zones" element={<ZonesPage />} />
            <Route path="/zones/:zoneId" element={<ZoneDetailsPage />} />
            <Route path="/reserve/:zoneId" element={<ReservePage />} />
            <Route path="/my-reservations" element={<MyReservationsPage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Routes>
        </main>
      </AuthProvider>
    </BrowserRouter>
  );
}
