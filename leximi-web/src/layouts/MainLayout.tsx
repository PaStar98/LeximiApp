import { Outlet, Link } from 'react-router-dom';
import { useAuth } from '../providers/AuthProvider';

const MainLayout = () => {
    const { user, logout, isAuthenticated } = useAuth();

    return (
        <div className="app-container">
            <header className="app-header">
                <nav>
                    <Link to="/" className="logo">Leximi</Link>
                    <div className="nav-links">
                        <Link to="/catalog">Katalog</Link>
                        {isAuthenticated ? (
                            <>
                                <Link to="/sets/create">Nowy zestaw</Link>
                                <Link to="/profile">Profil ({user?.username})</Link>
                                <button onClick={logout}>Wyloguj</button>
                            </>
                        ) : (
                            <>
                                <Link to="/login">Logowanie</Link>
                                <Link to="/register">Rejestracja</Link>
                            </>
                        )}
                    </div>
                </nav>
            </header>
            <main className="app-main">
                <Outlet />
            </main>
            <footer className="app-footer">
                <p>&copy; 2026 Leximi - Twój pomocnik w nauce</p>
            </footer>
        </div>
    );
};

export default MainLayout;
