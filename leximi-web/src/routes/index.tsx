import { Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import LoginPage from '../features/auth/LoginPage';
import RegisterPage from '../features/auth/RegisterPage';
import CatalogPage from '../features/catalog/CatalogPage';
import { useAuth } from '../providers/AuthProvider';

const AppRoutes = () => {
    const { isAuthenticated } = useAuth();

    return (
        <Routes>
            <Route element={<MainLayout />}>
                <Route path="/" element={<div><h2>Witaj w Leximi!</h2><p>Wybierz kategorię i zacznij się uczyć.</p></div>} />
                <Route path="/login" element={!isAuthenticated ? <LoginPage /> : <Navigate to="/" />} />
                <Route path="/register" element={!isAuthenticated ? <RegisterPage /> : <Navigate to="/" />} />

                {/* Protected Routes */}
                <Route path="/catalog" element={<CatalogPage />} />
                <Route path="/profile" element={isAuthenticated ? <div>Profil Użytkownika</div> : <Navigate to="/login" />} />
                <Route path="/sets/create" element={isAuthenticated ? <div>Tworzenie Zestawu</div> : <Navigate to="/login" />} />
            </Route>
        </Routes>
    );
};

export default AppRoutes;
