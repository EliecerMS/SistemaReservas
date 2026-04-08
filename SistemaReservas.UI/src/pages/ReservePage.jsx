import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getZoneById, getEventTypes, createReservation } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Auth.css';

export default function ReservePage() {
    const { zoneId } = useParams();
    const { user } = useAuth();
    const navigate = useNavigate();
    const [zone, setZone] = useState(null);
    const [eventTypes, setEventTypes] = useState([]);
    const [form, setForm] = useState({ eventTypeId: '', startDate: '', startTime: '', endDate: '', endTime: '', guestCount: '', notes: '' });
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (!user) { navigate('/login'); return; }
        getZoneById(zoneId).then(res => setZone(res.data)).catch(() => navigate('/zones'));
        getEventTypes().then(res => setEventTypes(res.data));
    }, [zoneId, user, navigate]);

    const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

    const timeOptions = [];
    for (let hour = 7; hour <= 20; hour++) {
        const time24 = `${hour.toString().padStart(2, '0')}:00`;
        const time12 = hour <= 12 ? `${hour}:00 AM` : `${hour - 12}:00 PM`;
        timeOptions.push({ value: time24, label: time12 });
    };

    const calculatePrice = () => {
        if (!form.startDate || !form.startTime || !form.endDate || !form.endTime || !zone) return 0;
        const start = new Date(`${form.startDate}T${form.startTime}`);
        const end = new Date(`${form.endDate}T${form.endTime}`);
        const hours = (end - start) / 3600000;
        return hours > 0 ? (zone.pricePerHour * hours).toFixed(0) : 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(''); setSuccess(''); setLoading(true);
        try {
            const startDateTime = new Date(`${form.startDate}T${form.startTime}`).toISOString();
            const endDateTime = new Date(`${form.endDate}T${form.endTime}`).toISOString();
            const payload = {
                zoneId: parseInt(zoneId),
                eventTypeId: parseInt(form.eventTypeId),
                startDateTime,
                endDateTime,
                guestCount: parseInt(form.guestCount),
                notes: form.notes
            };
            await createReservation(payload);
            setSuccess('¡Reserva creada exitosamente!');
            setTimeout(() => navigate('/my-reservations'), 2000);
        } catch (err) {
            setError(err.response?.data?.message);
            console.log(err);
        } finally { setLoading(false); }
    };

    if (!zone) return <div className="loading">Cargando...</div>;

    return (
        <div className="auth-container">
            <form className="auth-form" onSubmit={handleSubmit} style={{ maxWidth: '550px' }}>
                <h2>Reservar: {zone.name}</h2>
                <p style={{ color: '#8892b0', textAlign: 'center', marginBottom: '1rem' }}>Capacidad: {zone.capacity} personas · ₡{zone.pricePerHour.toLocaleString()}/hora</p>
                {error && <div className="error-message">{error}</div>}
                {success && <div className="success-message">{success}</div>}
                <div className="form-group">
                    <label>Tipo de Evento</label>
                    <select name="eventTypeId" value={form.eventTypeId} onChange={handleChange} required>
                        <option value="">Seleccionar...</option>
                        {eventTypes.filter(et => zone.eventTypes?.some(z => z.id === et.id)).map(et => (
                            <option key={et.id} value={et.id}>{et.name}</option>
                        ))}
                    </select>
                </div>
                <div className="form-group">
                    <label>Fecha de Inicio</label>
                    <input type="date" name="startDate" value={form.startDate} onChange={handleChange} required />
                </div>
                <div className="form-group">
                    <label>Hora de Inicio</label>
                    <select name="startTime" value={form.startTime} onChange={handleChange} required>
                        <option value="">Seleccionar...</option>
                        {timeOptions.map(opt => (
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                    </select>
                </div>
                <div className="form-group">
                    <label>Fecha de Fin</label>
                    <input type="date" name="endDate" value={form.endDate} onChange={handleChange} required />
                </div>
                <div className="form-group">
                    <label>Hora de Fin</label>
                    <select name="endTime" value={form.endTime} onChange={handleChange} required>
                        <option value="">Seleccionar...</option>
                        {timeOptions.map(opt => (
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                    </select>
                </div>
                <div className="form-group">
                    <label>Número de Invitados</label>
                    <input type="number" name="guestCount" value={form.guestCount} onChange={handleChange} required min="1" max={zone.capacity} placeholder={`Máximo ${zone.capacity}`} />
                </div>
                <div className="form-group">
                    <label>Notas (opcional)</label>
                    <textarea name="notes" value={form.notes} onChange={handleChange} rows="3" placeholder="Detalles adicionales..." />
                </div>
                {calculatePrice() > 0 && (
                    <div style={{ textAlign: 'center', color: '#a8dadc', fontSize: '1.1rem', fontWeight: '700', margin: '0.5rem 0' }}>
                        Total estimado: ₡{parseInt(calculatePrice()).toLocaleString()}
                    </div>
                )}
                <button type="submit" className="btn-primary" disabled={loading}>{loading ? 'Reservando...' : 'Confirmar Reserva'}</button>
            </form>
        </div>
    );
}
