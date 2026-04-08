import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Navbar.css';

export default function Navbar() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        navigate('/');
    };

    return (
        <nav className="navbar">
            <Link to="/" className="navbar-brand">
                SistemaReservas
            </Link>
            <div className="navbar-links">
                <Link to="/zones">Zonas</Link>
                {user ? (
                    <>
                        <Link to="/my-reservations">Mis Reservas</Link>
                        {user.role === 'Admin' && <Link to="/admin">Admin</Link>}
                        <span className="navbar-user">Hola, {user.fullName}</span>
                        <button onClick={handleLogout} className="btn-logout">Cerrar Sesión</button>
                    </>
                ) : (
                    <>
                        <Link to="/login">Iniciar Sesión</Link>
                        <Link to="/register" className="btn-register">Registrarse</Link>
                    </>
                )}
            </div>
        </nav>
    );
}
