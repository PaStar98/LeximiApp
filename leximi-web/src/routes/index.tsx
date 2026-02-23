import { Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import LoginPage from '../features/auth/LoginPage';
import RegisterPage from '../features/auth/RegisterPage';
import CatalogPage from '../features/catalog/CatalogPage';
import CategorySetsPage from '../features/catalog/CategorySetsPage';
import SetDetailsPage from '../features/sets/SetDetailsPage';
import CreateSetPage from '../features/sets/CreateSetPage';
import EditSetPage from '../features/sets/EditSetPage';
import LearningPage from '../features/learning/LearningPage';
import ProfilePage from '../features/profile/ProfilePage';
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
                <Route path="/catalog/:categoryId" element={isAuthenticated ? <CategorySetsPage /> : <Navigate to="/login" />} />
                <Route path="/sets/:id" element={isAuthenticated ? <SetDetailsPage /> : <Navigate to="/login" />} />
                <Route path="/sets/:id/edit" element={isAuthenticated ? <EditSetPage /> : <Navigate to="/login" />} />
                <Route path="/learning/:attemptId" element={isAuthenticated ? <LearningPage /> : <Navigate to="/login" />} />
                <Route path="/profile" element={isAuthenticated ? <ProfilePage /> : <Navigate to="/login" />} />

                <Route path="/sets/create" element={isAuthenticated ? <CreateSetPage /> : <Navigate to="/login" />} />
            </Route>
        </Routes>
    );
};

export default AppRoutes;
