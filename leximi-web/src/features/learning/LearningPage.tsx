import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { attemptService, learningSetService } from '../../api/services';
import { LearningItemDto, AnswerDto, SubmitAnswerDto } from '../../types';

const LearningPage = () => {
    const { attemptId } = useParams<{ attemptId: string }>();
    const navigate = useNavigate();
    const [currentIndex, setCurrentIndex] = useState(0);
    const [selectedAnswerId, setSelectedAnswerId] = useState<string | null>(null);
    const [feedback, setFeedback] = useState<{ isCorrect: boolean; message: string } | null>(null);
    const [isFlipped, setIsFlipped] = useState(false);

    const { data: attempt, isLoading: isAttemptLoading } = useQuery({
        queryKey: ['attempt', attemptId],
        queryFn: () => attemptService.getById(attemptId!),
        enabled: !!attemptId,
    });

    const { data: set, isLoading: isSetLoading } = useQuery({
        queryKey: ['set', attempt?.setId],
        queryFn: () => learningSetService.getById(attempt!.setId),
        enabled: !!attempt?.setId,
    });

    const submitAnswerMutation = useMutation({
        mutationFn: ({ attemptId, data }: { attemptId: string; data: SubmitAnswerDto }) =>
            attemptService.submitAnswer(attemptId, data),
        onSuccess: () => { }
    });

    const finishAttemptMutation = useMutation({
        mutationFn: attemptService.finish,
        onSuccess: () => {
            navigate('/profile');
        }
    });

    if (isAttemptLoading || isSetLoading) return <div>Ładowanie danych nauki...</div>;
    if (!attempt || !set) return <div>Nie znaleziono danych.</div>;

    if (attempt.finishedAt) {
        return (
            <div className="learning-container">
                <h2>Nauka zakończona!</h2>
                <p>Twój wynik: {attempt.score}</p>
                <button onClick={() => navigate('/profile')}>Wróć do profilu</button>
            </div>
        );
    }

    const currentItem = set.items[currentIndex];
    const isLastItem = currentIndex === set.items.length - 1;

    const handleAnswerSelect = (answerId: string) => {
        if (feedback) return;
        setSelectedAnswerId(answerId);
    };

    const handleSubmitAnswer = () => {
        if (!currentItem.question || !selectedAnswerId) return;

        const selectedAnswer = currentItem.question.answers.find(a => a.id === selectedAnswerId);
        const isCorrect = selectedAnswer?.isCorrect || false;

        setFeedback({
            isCorrect,
            message: isCorrect ? 'Poprawna odpowiedź!' : 'Błędna odpowiedź.'
        });

        submitAnswerMutation.mutate({
            attemptId: attemptId!,
            data: {
                learningItemId: currentItem.id,
                answerId: selectedAnswerId
            }
        });
    };

    const handleNext = () => {
        setFeedback(null);
        setSelectedAnswerId(null);
        setIsFlipped(false);
        if (isLastItem) {
            finishAttemptMutation.mutate(attemptId!);
        } else {
            setCurrentIndex(prev => prev + 1);
        }
    };

    return (
        <div className="learning-container">
            <div className="progress-bar">
                Pytanie {currentIndex + 1} z {set.items.length}
            </div>

            <div className="learning-card">
                {currentItem.question && (
                    <div className="question-content">
                        <h3>{currentItem.question.content}</h3>
                        <div className="answers-list">
                            {currentItem.question.answers.map(answer => (
                                <div
                                    key={answer.id}
                                    className={`answer-option 
                                        ${selectedAnswerId === answer.id ? 'selected' : ''}
                                        ${feedback && answer.isCorrect ? 'correct' : ''}
                                        ${feedback && selectedAnswerId === answer.id && !feedback.isCorrect ? 'incorrect' : ''}
                                    `}
                                    onClick={() => handleAnswerSelect(answer.id || '')}
                                >
                                    {answer.content}
                                </div>
                            ))}
                        </div>

                        {!feedback ? (
                            <button
                                onClick={handleSubmitAnswer}
                                disabled={!selectedAnswerId || submitAnswerMutation.isPending}
                                className="action-button"
                            >
                                Sprawdź
                            </button>
                        ) : (
                            <div className={`feedback ${feedback.isCorrect ? 'success' : 'error'}`}>
                                <p>{feedback.message}</p>
                                <button
                                    onClick={handleNext}
                                    className="action-button"
                                    disabled={submitAnswerMutation.isPending || finishAttemptMutation.isPending}
                                >
                                    {(submitAnswerMutation.isPending || finishAttemptMutation.isPending) ? 'Przetwarzanie...' : (isLastItem ? 'Zakończ' : 'Następne')}
                                </button>
                            </div>
                        )}
                    </div>
                )}

                {currentItem.flashcard && (
                    <div className="flashcard-content">
                        <div
                            className={`flashcard ${isFlipped ? 'flipped' : ''}`}
                            onClick={() => setIsFlipped(!isFlipped)}
                        >
                            <div className="front">
                                <h3>Przód</h3>
                                <p>{currentItem.flashcard.front}</p>
                                <span className="hint">(Kliknij, aby zobaczyć tył)</span>
                            </div>
                            <div className="back">
                                <h3>Tył</h3>
                                <p>{currentItem.flashcard.back}</p>
                            </div>
                        </div>
                        <button onClick={handleNext} className="action-button">
                            {isLastItem ? 'Zakończ' : 'Następne'}
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
};

export default LearningPage;
