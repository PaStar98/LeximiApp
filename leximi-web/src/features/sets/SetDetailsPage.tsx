import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { learningSetService, attemptService } from '../../api/services';

const SetDetailsPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const { data: set, isLoading, error } = useQuery({
        queryKey: ['set', id],
        queryFn: () => learningSetService.getById(id!),
        enabled: !!id,
    });

    const startAttemptMutation = useMutation({
        mutationFn: attemptService.start,
        onSuccess: (attempt) => {
            navigate(`/learning/${attempt.id}`);
        },
        onError: () => {
            alert("Nie udało się rozpocząć nauki.");
        }
    });

    const handleStart = () => {
        if (id) {
            startAttemptMutation.mutate(id);
        }
    };

    if (isLoading) return <div>Ładowanie zestawu...</div>;
    if (error) return <div>Wystąpił błąd podczas pobierania zestawu.</div>;
    if (!set) return <div>Zestaw nie znaleziony.</div>;

    return (
        <div className="set-details-container">
            <h1>{set.title}</h1>
            <p className="description">{set.description}</p>
            <div className="set-info">
                <span>Typ: {set.type}</span>
                <span>Liczba elementów: {set.items.length}</span>
            </div>

            <button
                className="start-button"
                onClick={handleStart}
                disabled={startAttemptMutation.isPending}
            >
                {startAttemptMutation.isPending ? 'Rozpoczynanie...' : 'Zacznij naukę'}
            </button>

            <div className="items-preview">
                <h3>Podgląd elementów:</h3>
                <ul>
                    {set.items.slice(0, 5).map(item => (
                        <li key={item.id}>
                            {item.question ? item.question.content : item.flashcard?.front}
                        </li>
                    ))}
                    {set.items.length > 5 && <li>... i {set.items.length - 5} więcej</li>}
                </ul>
            </div>
        </div>
    );
};

export default SetDetailsPage;
