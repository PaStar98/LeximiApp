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
                                <th>Typ</th>
                                <th>Data rozpoczęcia</th>
                                <th>Data zakończenia</th>
                                <th>Wynik</th>
                            </tr>
                        </thead>
                        <tbody>
                            {history.map((attempt) => (
                                <tr key={attempt.id}>
                                    <td>{attempt.setTitle}</td>
                                    <td>{attempt.setType}</td>
                                    <td>{new Date(attempt.startedAt).toLocaleString('pl-PL', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false }).replace(',', '')}</td>
                                    <td>{attempt.finishedAt ? new Date(attempt.finishedAt).toLocaleString('pl-PL', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false }).replace(',', '') : 'W trakcie'}</td>
                                    <td>{attempt.setType === 'Flashcards' ? '-' : `${attempt.score} / ${attempt.maxScore}`}</td>
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
