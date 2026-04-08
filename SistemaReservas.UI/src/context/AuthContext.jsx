import { createContext, useContext, useState, useEffect } from 'react';
import { login as loginApi, register as registerApi, logout as logoutApi, setOnSessionExpired } from '../services/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [user, setUser] = useState(() => {
        const saved = localStorage.getItem('user');
        return saved ? JSON.parse(saved) : null;
    });

    // Persist user info in localStorage so page refreshes don't log out the user
    useEffect(() => {
        if (user) {
            localStorage.setItem('user', JSON.stringify(user));
        } else {
            localStorage.removeItem('user');
        }
    }, [user]);

    // Register the session-expired handler once on mount.
    // When the Axios interceptor cannot refresh the token it calls this,
    // which clears local state — protected routes then redirect to /login.
    useEffect(() => {
        setOnSessionExpired(() => {
            setUser(null);
        });
    }, []);

    const loginFn = async (email, password) => {
        const res = await loginApi({ email, password });
        setUser(res.data);
        return res.data;
    };

    const registerFn = async (data) => {
        await registerApi(data);
    };

    const logoutFn = async () => {
        await logoutApi();
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, login: loginFn, register: registerFn, logout: logoutFn }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => useContext(AuthContext);
