import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getZoneById } from '../services/api';
import './ZoneDetailsPage.css';
import { EmblaCarousel } from '../components/Carousel';

export default function ZoneDetailsPage() {
    const { zoneId } = useParams();
    const [zone, setZone] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getZoneById(zoneId)
            .then(res => setZone(res.data))
            .catch(console.error)
            .finally(() => setLoading(false));
    }, [zoneId]);

    if (loading) return <div className="loading">Cargando detalles de la zona...</div>;
    if (!zone) return <div className="error">Zona no encontrada.</div>;

    const primaryImage = zone.images?.find(i => i.isPrimary)?.imageUrl || zone.images?.[0]?.imageUrl || 'https://placehold.co/1200x600/333/white?text=Sin+imagen';
    const secondaryImages = zone.images?.filter(i => !i.isPrimary && i.imageUrl !== primaryImage) || [];

    return (
        <div className="zone-details-page">
            <div className="zone-details-header">
                <h1>{zone.name}</h1>
                <div className="zone-meta-top">
                    <span>Hasta {zone.capacity} personas</span>
                    <span className="dot-separator">I</span>
                    <span>{zone.location || 'Locación central'}</span>
                </div>
            </div>



            <div className="zone-carousel-container">
                <EmblaCarousel slides={zone.images?.length > 0 ? zone.images.map(img => img.imageUrl) : [primaryImage]} options={{ loop: true }} />
            </div>

            <div className="zone-content-wrapper">
                <div className="zone-main-info">
                    <section className="info-section">
                        <h2>Acerca de este espacio</h2>
                        <p>{zone.description}</p>
                    </section>

                    {zone.eventTypes && zone.eventTypes.length > 0 && (
                        <section className="info-section">
                            <h2>Ideal para</h2>
                            <div className="zone-tags-large">
                                {zone.eventTypes.map(et => (
                                    <span key={et.id} className="tag-large">{et.name}</span>
                                ))}
                            </div>
                        </section>
                    )}
                </div>

                <div className="zone-sidebar">
                    <div className="reservation-card">
                        <div className="card-header">
                            <span className="price">₡{zone.pricePerHour.toLocaleString()}</span>
                            <span className="per-night">/ hora</span>
                        </div>
                        <div className="card-body">
                            <div className="info-row">
                                <span className="label">Capacidad máxima</span>
                                <span className="value">{zone.capacity} personas</span>
                            </div>
                            <Link to={`/reserve/${zone.id}`} className="btn-reserve-large">Reservar ahora</Link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
