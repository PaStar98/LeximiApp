export interface UserDto {
    id: string;
    username: string;
    email: string;
}

export interface AuthResponseDto {
    token: string;
    username: string;
    email: string;
}

export interface LoginRequestDto {
    email: string;
    password: string;
}

export interface RegisterRequestDto {
    email: string;
    username: string;
    password: string;
}

export interface CategoryDto {
    id: string;
    name: string;
    description?: string;
}

export interface CreateCategoryDto {
    name: string;
    description?: string;
}

export interface LearningSetDto {
    id: string;
    title: string;
    description?: string;
    categoryId: string;
    type: string;
}

export interface CreateLearningSetDto {
    title: string;
    description?: string | null;
    categoryId: string;
    type: string;
    items?: UpdateLearningItemDto[];
}

export interface AnswerDto {
    id?: string;
    content: string;
    isCorrect: boolean;
}

export interface QuestionDto {
    id: string;
    content: string;
    answers: AnswerDto[];
}

export interface FlashcardDto {
    front: string;
    back: string;
}

export interface LearningItemDto {
    id: string;
    question?: QuestionDto;
    flashcard?: FlashcardDto;
}

export interface LearningSetDetailsDto extends LearningSetDto {
    items: LearningItemDto[];
}

export interface UpdateLearningItemDto {
    id?: string;
    questionContent?: string | null;
    answers?: AnswerDto[] | null;
    flashcardFront?: string | null;
    flashcardBack?: string | null;
}

export interface UpdateLearningSetDto {
    title: string;
    description?: string | null;
    type: string;
    items: UpdateLearningItemDto[];
}

export interface AttemptDto {
    id: string;
    setId: string;
    startedAt: string;
    finishedAt?: string;
    score: number;
}

export interface SubmitAnswerDto {
    questionId: string;
    answerId?: string;
    providedText?: string;
}

export interface AttemptHistoryDto {
    id: string;
    setId: string;
    setTitle: string;
    startedAt: string;
    finishedAt?: string;
    score: number;
}
// Note: AttemptDto in backend didn't have Title. GetUserHistoryAsync returns AttemptDto.
// If we want Title in history, we need to update backend AttemptDto or fetch separately.
// For now, I'll stick to AttemptDto structure which doesn't have Title.
// But the UI usually shows "You practiced Set X".
// I'll check AttemptEndpoint.GetUserHistoryAsync.
