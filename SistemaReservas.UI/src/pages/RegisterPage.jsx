import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Auth.css';

export default function RegisterPage() {
    const [form, setForm] = useState({ fullName: '', email: '', password: '', phone: '' });
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { register } = useAuth();
    const navigate = useNavigate();

    const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);
        try {
            await register(form);
            navigate('/login', { state: { message: '¡Registro exitoso! Inicia sesión.' } });
        } catch (err) {
            setError(err.response?.data?.message || 'Error al registrarse');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <form className="auth-form" onSubmit={handleSubmit}>
                <h2>Crear Cuenta</h2>
                {error && <div className="error-message">{error}</div>}
                <div className="form-group">
                    <label>Nombre Completo</label>
                    <input name="fullName" value={form.fullName} onChange={handleChange} required placeholder="Juan Pérez" />
                </div>
                <div className="form-group">
                    <label>Correo Electrónico</label>
                    <input type="email" name="email" value={form.email} onChange={handleChange} required placeholder="correo@ejemplo.com" />
                </div>
                <div className="form-group">
                    <label>Contraseña</label>
                    <input type="password" name="password" value={form.password} onChange={handleChange} required minLength={6} placeholder="Mínimo 6 caracteres" />
                </div>
                <div className="form-group">
                    <label>Teléfono (opcional)</label>
                    <input name="phone" value={form.phone} onChange={handleChange} placeholder="8888-8888" />
                </div>
                <button type="submit" className="btn-primary" disabled={loading}>{loading ? 'Registrando...' : 'Registrarse'}</button>
                <p className="auth-link">¿Ya tienes cuenta? <Link to="/login">Inicia sesión</Link></p>
            </form>
        </div>
    );
}
