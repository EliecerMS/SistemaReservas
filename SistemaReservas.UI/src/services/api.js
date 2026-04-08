import axios from 'axios';

const API = axios.create({
    baseURL: 'https://localhost:7148/api', // HTTPS port from launchSettings
    headers: {
        'Content-Type': 'application/json'
    },
    withCredentials: true // Enable sending cookies with requests
});

// ── Refresh token interceptor ─────────────────────────────────────────────────
// Intercepts 401 responses; silently refreshes the session and retries once.
// Callers that register via setOnSessionExpired() are notified when refresh fails.

let _isRefreshing = false;
let _pendingQueue = []; // queued requests waiting for a refresh in progress
let _onSessionExpired = null;

export const setOnSessionExpired = (callback) => {
    _onSessionExpired = callback;
};

const processQueue = (error) => {
    _pendingQueue.forEach((p) => (error ? p.reject(error) : p.resolve()));
    _pendingQueue = [];
};

API.interceptors.response.use(
    (response) => response,
    async (error) => {
        const original = error.config;

        // Skip the interceptor for auth endpoints to avoid infinite loops
        const isAuthCall = original.url?.includes('/auth/login') ||
                           original.url?.includes('/auth/refresh');

        if (error.response?.status === 401 && !isAuthCall && !original._retry) {
            if (_isRefreshing) {
                // Another refresh is in-flight — queue this request
                return new Promise((resolve, reject) => {
                    _pendingQueue.push({ resolve, reject });
                }).then(() => API(original))
                  .catch((err) => Promise.reject(err));
            }

            original._retry   = true;
            _isRefreshing     = true;

            try {
                await API.post('/auth/refresh');  // sets new HttpOnly cookies
                processQueue(null);
                return API(original);             // retry the original request
            } catch (refreshError) {
                processQueue(refreshError);
                _onSessionExpired?.();            // notify AuthContext to clear state
                return Promise.reject(refreshError);
            } finally {
                _isRefreshing = false;
            }
        }

        return Promise.reject(error);
    }
);

// ── Auth ──────────────────────────────────────────────────────────────────────
export const login          = (data) => API.post('/auth/login', data);
export const register       = (data) => API.post('/auth/register', data);
export const logout         = ()     => API.post('/auth/logout');
export const refreshSession = ()     => API.post('/auth/refresh');

// ── Zones ─────────────────────────────────────────────────────────────────────
export const getZones      = (page = 1, pageSize = 20) =>
    API.get('/zones', { params: { page, pageSize } });
export const getZoneById   = (id) => API.get(`/zones/${id}`);
export const getEventTypes = ()   => API.get('/zones/event-types');

// ── Reservations ──────────────────────────────────────────────────────────────
export const createReservation  = (data) => API.post('/Reservations', data);
export const getMyReservations  = ()     => API.get('/reservations/my');
export const cancelReservation  = (id)   => API.delete(`/reservations/${id}`);
export const getAllReservations  = (statusId, page = 1, pageSize = 20) =>
    API.get('/reservations', { params: { ...(statusId ? { statusId } : {}), page, pageSize } });
export const confirmReservation = (id)   => API.put(`/reservations/${id}/confirm`);

export default API;

