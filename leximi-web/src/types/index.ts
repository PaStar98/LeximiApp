export interface Category {
    id: string;
    name: string;
    description?: string;
}

export interface LearningSet {
    id: string;
    title: string;
    description?: string;
    type: 'Exam' | 'Test' | 'Flashcards';
    categoryId: string;
}

export interface LearningItem {
    id: string;
    learningSetId: string;
    question?: Question;
    flashcard?: Flashcard;
}

export interface Question {
    id: string;
    content: string;
    answers: Answer[];
}

export interface Answer {
    id: string;
    content: string;
    isCorrect: boolean;
}

export interface Flashcard {
    id: string;
    front: string;
    back: string;
}
