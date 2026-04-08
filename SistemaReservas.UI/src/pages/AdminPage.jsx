import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getAllReservations, cancelReservation, confirmReservation } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Tables.css';

export default function AdminPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [reservations, setReservations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [filter, setFilter] = useState('');
    const [error, setError] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    // when filter or pageSize changes we go back to first page
    useEffect(() => { setPage(1); }, [filter, pageSize]);
    const [total, setTotal] = useState(0);

    useEffect(() => {
        if (!user || user.role !== 'Admin') { navigate('/'); return; }
        loadData();
    }, [user, navigate, filter, page, pageSize]);

    const loadData = () => {
        setLoading(true);
        getAllReservations(filter || undefined, page, pageSize)
            .then(res => {
                setReservations(res.data.items);
                setTotal(res.data.totalCount);
            })
            .catch(console.error)
            .finally(() => setLoading(false));
    };

    const handleConfirm = async (id) => {
        setError('');
        try { await confirmReservation(id); loadData(); } catch (err) { setError(err.response?.data?.message || 'Error'); }
    };

    const handleCancel = async (id) => {
        if (!window.confirm('¿Cancelar esta reserva?')) return;
        setError('');
        try { await cancelReservation(id); loadData(); } catch (err) { setError(err.response?.data?.message || 'Error'); }
    };

    const getStatusClass = (status) => {
        const map = { 'Pendiente': 'status-pending', 'Confirmada': 'status-confirmed', 'Cancelada': 'status-cancelled', 'Completada': 'status-completed' };
        return map[status] || '';
    };

    if (loading) return <div className="loading">Cargando...</div>;

    return (
        <div className="table-page">
            <h1>Panel de Administración</h1>
            <div className="admin-filters">
                <label>
                    Mostrar:
                    <select value={filter} onChange={e => setFilter(e.target.value)}>
                        <option value="">Todas</option>
                        <option value="1">Pendientes</option>
                        <option value="2">Confirmadas</option>
                        <option value="3">Canceladas</option>
                        <option value="4">Completadas</option>
                    </select>
                </label>
                <label style={{ marginLeft: '1rem' }}>
                    resultados por página:
                    <select value={pageSize} onChange={e => setPageSize(parseInt(e.target.value))}>
                        <option value={2}>2</option>
                        <option value={5}>5</option>
                        <option value={10}>10</option>
                        <option value={20}>20</option>
                        <option value={50}>50</option>
                    </select>
                </label>
            </div>
            {error && <div className="error-message" style={{ maxWidth: '600px', margin: '0 auto 1rem' }}>{error}</div>}
            <div className="table-container">
                <table>
                    <thead>
                        <tr><th>ID</th><th>Cliente</th><th>Zona</th><th>Evento</th><th>Inicio</th><th>Invitados</th><th>Total</th><th>Estado</th><th>Acciones</th></tr>
                    </thead>
                    <tbody>
                        {reservations.length === 0 ? (
                            <tr><td colSpan="9" style={{ textAlign: 'center', color: '#424242ff' }}>No hay reservas.</td></tr>
                        ) : (
                            reservations.map(r => (
                                <tr key={r.id}>
                                    <td>{r.id}</td>
                                    <td>{r.userFullName}</td>
                                    <td>{r.zoneName}</td>
                                    <td>{r.eventTypeName}</td>
                                    <td>{new Date(r.startDateTime).toLocaleString('es-CR')}</td>
                                    <td>{r.guestCount}</td>
                                    <td>₡{r.totalPrice.toLocaleString()}</td>
                                    <td><span className={`status ${getStatusClass(r.statusName)}`}>{r.statusName}</span></td>
                                    <td className="action-buttons">
                                        {r.statusName === 'Pendiente' && <button onClick={() => handleConfirm(r.id)} className="btn-confirm">Confirmar</button>}
                                        {(r.statusName === 'Pendiente' || r.statusName === 'Confirmada') && <button onClick={() => handleCancel(r.id)} className="btn-cancel">Cancelar</button>}
                                    </td>
                                </tr>
                            )))}
                    </tbody>
                </table>
            </div>
            <div className="pagination">
                <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Prev</button>
                <span className="page-info">
                    {page} / {Math.ceil(total / pageSize) || 1}
                </span>
                <button disabled={page >= Math.ceil(total / pageSize)} onClick={() => setPage(p => p + 1)}>Next</button>
            </div>
        </div>
    );
}
