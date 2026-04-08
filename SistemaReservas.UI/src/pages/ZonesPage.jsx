import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getZones } from '../services/api';
import './ZonesPage.css';

export default function ZonesPage() {
    const [zones, setZones] = useState([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    const [total, setTotal] = useState(0);
    // when pageSize changes go back to first page
    useEffect(() => { setPage(1); }, [pageSize]);

    const loadZones = () => {
        setLoading(true);
        getZones(page, pageSize)
            .then(res => {
                setZones(res.data.items);
                setTotal(res.data.totalCount);
            })
            .catch(console.error)
            .finally(() => setLoading(false));
    };

    useEffect(() => {
        loadZones();
    }, [page, pageSize]);

    if (loading) return <div className="loading">Cargando zonas...</div>;

    return (
        <div className="zones-page">
            <h1>Nuestras Zonas</h1>
            <p className="zones-subtitle">Selecciona la zona perfecta para tu evento</p>
            <div className="page-controls">
                <label>
                    Elementos por página:
                    <select value={pageSize} onChange={e => setPageSize(parseInt(e.target.value))}>
                        <option value={2}>2</option>
                        <option value={5}>5</option>
                        <option value={10}>10</option>
                        <option value={20}>20</option>
                        <option value={50}>50</option>
                    </select>
                </label>
            </div>
            <div className="zones-grid">
                {zones.map(zone => (
                    <div key={zone.id} className="zone-card">
                        <div className="zone-image">
                            <img src={zone.images?.find(i => i.isPrimary)?.imageUrl || zone.images?.[0]?.imageUrl || 'https://placehold.co/800x600/333/white?text=Sin+imagen'} alt={zone.name} />
                            <div className="zone-price">₡{zone.pricePerHour.toLocaleString()}/hora</div>
                        </div>
                        <div className="zone-body">
                            <h3>{zone.name}</h3>
                            <p className="zone-desc">{zone.description}</p>
                            <div className="zone-meta">
                                <span>Hasta {zone.capacity} personas</span>
                            </div>
                            <div className="zone-tags">
                                {zone.eventTypes?.map(et => (
                                    <span key={et.id} className="tag">{et.name}</span>
                                ))}
                            </div>
                            <Link to={`/zones/${zone.id}`} className="btn-reserve">Ver más</Link>
                        </div>
                    </div>
                ))}
            </div>
            {/* simple pagination */}
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
