import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { categoryService } from '../../api/services';
import {
    LearningSetDetailsDto,
    UpdateLearningSetDto,
    UpdateLearningItemDto,
    AnswerDto,
    CategoryDto
} from '../../types';

interface SetEditorProps {
    initialData?: LearningSetDetailsDto;
    onSubmit: (data: UpdateLearningSetDto) => void;
    isSubmitting: boolean;
}

const SetEditor = ({ initialData, onSubmit, isSubmitting }: SetEditorProps) => {
    const [title, setTitle] = useState(initialData?.title || '');
    const [description, setDescription] = useState(initialData?.description || '');
    const [categoryId, setCategoryId] = useState(initialData?.categoryId || ''); // Note: We need categoryId in DTOs if we want to change it, but UpdateDTO doesn't have it currently. Assuming we can't change category for now or relying on initial create. 
    // Wait, UpdateLearningSetDto (backend) doesn't have CategoryId. create does. 
    // Let's assume for Edit we don't change category, but for Create we need it. 
    // Actually, SetEditor might be used for Create too. 
    // If it's create, we need to pass categoryId back separately or handle it in parent.
    // Let's stick to the props interface. The parent handles the API call structure. 
    // But `UpdateLearningSetDto` is what we pass back. It has title, description, type, items.

    // For `CreateSet`, we usually pick a category first or in the form. 
    // Simplification: `SetEditor` returns `UpdateLearningSetDto`. Parent adds `categoryId` if creating.

    const [type, setType] = useState('Flashcards');
    const [items, setItems] = useState<UpdateLearningItemDto[]>([]);

    useEffect(() => {
        if (initialData) {
            setTitle(initialData.title || '');
            setDescription(initialData.description || '');
            setCategoryId(initialData.categoryId || '');
            setType(initialData.type || 'Flashcards');

            const normalizedItems = initialData.items.map(i => {
                // Handle potential PascalCase from API if it happens
                const question = i.question;
                const flashcard = i.flashcard;

                return {
                    id: i.id,
                    questionContent: question?.content,
                    flashcardFront: flashcard?.front,
                    flashcardBack: flashcard?.back,
                    answers: question?.answers?.map(a => ({
                        id: a.id,
                        content: a.content,
                        isCorrect: a.isCorrect
                    })) || []
                };
            });
            setItems(normalizedItems);
        }
    }, [initialData]);

    const { data: categories } = useQuery({
        queryKey: ['categories'],
        queryFn: categoryService.getAll
    });

    const handleAddItem = () => {
        setItems([...items, {
            questionContent: '',
            answers: [],
            flashcardFront: '',
            flashcardBack: ''
        }]);
    };

    const handleRemoveItem = (index: number) => {
        const newItems = [...items];
        newItems.splice(index, 1);
        setItems(newItems);
    };

    const handleItemChange = (index: number, field: keyof UpdateLearningItemDto, value: any) => {
        const newItems = [...items];
        newItems[index] = { ...newItems[index], [field]: value };
        setItems(newItems);
    };

    const handleAnswerChange = (itemIndex: number, answerIndex: number, field: keyof AnswerDto, value: any) => {
        const newItems = [...items];
        const answers = [...(newItems[itemIndex].answers || [])];
        answers[answerIndex] = { ...answers[answerIndex], [field]: value };
        newItems[itemIndex].answers = answers;
        setItems(newItems);
    };

    const handleAddAnswer = (itemIndex: number) => {
        const newItems = [...items];
        const answers = newItems[itemIndex].answers || [];
        answers.push({ content: '', isCorrect: false }); // No ID for new answers
        newItems[itemIndex].answers = answers;
        setItems(newItems);
    };

    const handleRemoveAnswer = (itemIndex: number, answerIndex: number) => {
        const newItems = [...items];
        const answers = [...(newItems[itemIndex].answers || [])];
        answers.splice(answerIndex, 1);
        newItems[itemIndex].answers = answers;
        setItems(newItems);
    };

    const isFormValid = () => {
        if (!title.trim() || items.length === 0) return false;

        return items.every(item => {
            if (type === 'Flashcards') {
                return item.flashcardFront?.trim() && item.flashcardBack?.trim();
            } else {
                const hasQuestionContent = !!item.questionContent?.trim();
                const hasAnswers = item.answers && item.answers.length > 0;
                const allAnswersHaveContent = item.answers?.every(a => !!a.content?.trim());
                const hasCorrectAnswer = item.answers?.some(a => a.isCorrect);

                return hasQuestionContent && hasAnswers && allAnswersHaveContent && hasCorrectAnswer;
            }
        });
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!isFormValid()) return;
        
        onSubmit({
            title,
            description,
            type,
            categoryId,
            items
        });
    };

    return (
        <form onSubmit={handleSubmit} className="set-editor">
            <div className="form-group">
                <label>Tytuł</label>
                <input
                    type="text"
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                    required
                    className="input-field"
                />
            </div>

            <div className="form-group">
                <label>Opis</label>
                <textarea
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                    className="input-field"
                />
            </div>

            {!initialData && (
                <div className="form-group">
                    <label>Typ</label>
                    <select
                        value={type}
                        onChange={e => {
                            setType(e.target.value);
                            setItems([]); // Clear items on type change for simplicity
                        }}
                        className="input-field"
                    >
                        <option value="Flashcards">Fiszki</option>
                        <option value="Quiz">Quiz</option>
                    </select>
                </div>
            )}

            <h3>Elementy ({type !== 'Flashcards' ? 'Pytania' : 'Fiszki'})</h3>

            <div className="items-list">
                {items.map((item, index) => (
                    <div key={index} className="editor-item-card">
                        <div className="item-header">
                            <span>#{index + 1}</span>
                            <button type="button" onClick={() => handleRemoveItem(index)} className="btn-remove">Usuń</button>
                        </div>

                        {type !== 'Flashcards' ? (
                            <>
                                <input
                                    type="text"
                                    placeholder="Treść pytania"
                                    value={item.questionContent || ''}
                                    onChange={e => handleItemChange(index, 'questionContent', e.target.value)}
                                    className="input-field mb-2"
                                />
                                <div className="answers-section">
                                    <label>Odpowiedzi:</label>
                                    {(item.answers || []).map((answer, ansIndex) => (
                                        <div key={ansIndex} className="answer-row">
                                            <input
                                                type="text"
                                                placeholder="Odpowiedź"
                                                value={answer.content}
                                                onChange={e => handleAnswerChange(index, ansIndex, 'content', e.target.value)}
                                                className="input-field"
                                            />
                                            <label className="checkbox-label">
                                                <input
                                                    type="checkbox"
                                                    checked={answer.isCorrect}
                                                    disabled={!answer.isCorrect && item.answers?.some(a => a.isCorrect)}
                                                    onChange={e => handleAnswerChange(index, ansIndex, 'isCorrect', e.target.checked)}
                                                />
                                                Poprawna
                                            </label>
                                            <button type="button" onClick={() => handleRemoveAnswer(index, ansIndex)} className="btn-icon">×</button>
                                        </div>
                                    ))}
                                    {!initialData && (
                                        <button type="button" onClick={() => handleAddAnswer(index)} className="btn-small">+ Dodaj odpowiedź</button>
                                    )}
                                </div>
                            </>
                        ) : (
                            <>
                                <input
                                    type="text"
                                    placeholder="Przód (Awers)"
                                    value={item.flashcardFront || ''}
                                    onChange={e => handleItemChange(index, 'flashcardFront', e.target.value)}
                                    className="input-field mb-2"
                                />
                                <input
                                    type="text"
                                    placeholder="Tył (Rewers)"
                                    value={item.flashcardBack || ''}
                                    onChange={e => handleItemChange(index, 'flashcardBack', e.target.value)}
                                    className="input-field"
                                />
                            </>
                        )}
                    </div>
                ))}
            </div>

            {!initialData && (
                <button type="button" onClick={handleAddItem} className="btn-secondary w-full mt-4">+ Dodaj element</button>
            )}
            <button type="submit" disabled={isSubmitting || !isFormValid()} className="btn-primary w-full mt-4">
                {isSubmitting ? 'Zapisywanie...' : 'Zapisz Zestaw'}
            </button>
        </form>
    );
};

export default SetEditor;
