import { useQuery } from '@tanstack/react-query';
import { authService, attemptService } from '../../api/services';

const ProfilePage = () => {
    const { data: user, isLoading: isUserLoading } = useQuery({
        queryKey: ['me'],
        queryFn: authService.me,
    });

    const { data: history, isLoading: isHistoryLoading } = useQuery({
        queryKey: ['history'],
        queryFn: attemptService.getHistory,
    });

    if (isUserLoading || isHistoryLoading) return <div>Ładowanie profilu...</div>;

    return (
        <div className="profile-container">
            <div className="user-info">
                <h2>Profil Użytkownika</h2>
                <p><strong>Nazwa:</strong> {user?.username}</p>
                <p><strong>Email:</strong> {user?.email}</p>
            </div>

            <div className="history-section">
                <h3>Historia Nauki</h3>
                {history && history.length > 0 ? (
                    <table className="history-table">
                        <thead>
                            <tr>
                                <th>Zestaw</th>
                                <th>Data rozpoczęcia</th>
                                <th>Data zakończenia</th>
                                <th>Wynik</th>
                            </tr>
                        </thead>
                        <tbody>
                            {history.map((attempt) => (
                                <tr key={attempt.id}>
                                    <td>{attempt.setTitle}</td>
                                    <td>{new Date(attempt.startedAt).toLocaleString()}</td>
                                    <td>{attempt.finishedAt ? new Date(attempt.finishedAt).toLocaleString() : 'W trakcie'}</td>
                                    <td>{attempt.score}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                ) : (
                    <p>Brak historii nauki.</p>
                )}
            </div>
        </div>
    );
};

export default ProfilePage;
