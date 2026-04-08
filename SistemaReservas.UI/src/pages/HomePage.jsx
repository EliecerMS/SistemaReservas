import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './HomePage.css';

export default function HomePage() {
    const { user } = useAuth();

    return (
        <div className="home">
            <section className="hero">
                <div className="hero-content">
                    <h1>Reserva tu espacio perfecto</h1>
                    <p>Fiestas, bodas y eventos recreativos en las mejores zonas de Costa Rica. Reserva en minutos, disfruta por horas.</p>
                    <div className="hero-buttons">
                        <Link to="/zones" className="btn-hero-outline">Ver Zonas</Link>
                        {!user && <Link to="/register" className="btn-hero-outline">Crear Cuenta</Link>}
                    </div>
                </div>
            </section>

            <section className="features">
                <div className="feature-card">
                    <div className="feature-icon"></div>
                    <h3>3 Zonas Únicas</h3>
                    <p>Jardín, Salón Principal y área de Piscina, cada una diseñada para diferentes tipos de evento.</p>
                </div>
                <div className="feature-card">
                    <div className="feature-icon"></div>
                    <h3>Reserva Instantánea</h3>
                    <p>Selecciona tu zona, elige fecha y hora, y confirma tu reserva en segundos.</p>
                </div>
                <div className="feature-card">
                    <div className="feature-icon"></div>
                    <h3>Seguro y Confiable</h3>
                    <p>Sistema con protección contra doble reserva y cancelaciones flexibles hasta 48h antes.</p>
                </div>
            </section>
        </div>
    );
}
