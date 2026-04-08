import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyReservations, cancelReservation } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Tables.css';

export default function MyReservationsPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [reservations, setReservations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        if (!user) { navigate('/login'); return; }
        loadData();
    }, [user, navigate]);

    const loadData = () => {
        setLoading(true);
        getMyReservations().then(res => setReservations(res.data)).catch(console.error).finally(() => setLoading(false));
    };

    const handleCancel = async (id) => {
        if (!window.confirm('¿Estás seguro de que deseas cancelar esta reserva?')) return;
        setError('');
        try {
            await cancelReservation(id);
            loadData();
        } catch (err) {
            setError(err.response?.data?.message || 'Error al cancelar');
        }
    };

    const getStatusClass = (status) => {
        const map = { 'Pendiente': 'status-pending', 'Confirmada': 'status-confirmed', 'Cancelada': 'status-cancelled', 'Completada': 'status-completed' };
        return map[status] || '';
    };

    if (loading) return <div className="loading">Cargando...</div>;

    return (
        <div className="table-page">
            <h1>Mis Reservas</h1>
            {error && <div className="error-message" style={{ maxWidth: '600px', margin: '0 auto 1rem' }}>{error}</div>}
            {reservations.length === 0 ? (
                <p style={{ textAlign: 'center', color: '#8892b0' }}>No tienes reservas aún.</p>
            ) : (
                <div className="table-container">
                    <table>
                        <thead>
                            <tr><th>Zona</th><th>Evento</th><th>Inicio</th><th>Fin</th><th>Invitados</th><th>Total</th><th>Estado</th><th>Acción</th></tr>
                        </thead>
                        <tbody>
                            {reservations.map(r => (
                                <tr key={r.id}>
                                    <td>{r.zoneName}</td>
                                    <td>{r.eventTypeName}</td>
                                    <td>{new Date(r.startDateTime).toLocaleString('es-CR')}</td>
                                    <td>{new Date(r.endDateTime).toLocaleString('es-CR')}</td>
                                    <td>{r.guestCount}</td>
                                    <td>₡{r.totalPrice.toLocaleString()}</td>
                                    <td><span className={`status ${getStatusClass(r.statusName)}`}>{r.statusName}</span></td>
                                    <td>
                                        {(r.statusName === 'Pendiente' || r.statusName === 'Confirmada') && (
                                            <button onClick={() => handleCancel(r.id)} className="btn-cancel">Cancelar</button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
