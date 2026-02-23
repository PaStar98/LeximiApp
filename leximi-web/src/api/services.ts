import apiClient from './client';
import {
    LoginRequestDto, RegisterRequestDto, AuthResponseDto, UserDto,
    CategoryDto, CreateCategoryDto,
    LearningSetDto, LearningSetDetailsDto, CreateLearningSetDto, UpdateLearningSetDto,
    AttemptDto, SubmitAnswerDto, AttemptHistoryDto
} from '../types';

export const authService = {
    register: async (data: RegisterRequestDto): Promise<AuthResponseDto> => {
        const response = await apiClient.post<AuthResponseDto>('/auth/register', data);
        return response.data;
    },
    login: async (data: LoginRequestDto): Promise<AuthResponseDto> => {
        const response = await apiClient.post<AuthResponseDto>('/auth/login', data);
        return response.data;
    },
    me: async (): Promise<UserDto> => {
        const response = await apiClient.get<UserDto>('/auth/me');
        return response.data;
    }
};

export const categoryService = {
    getAll: async (): Promise<CategoryDto[]> => {
        const response = await apiClient.get<CategoryDto[]>('/categories');
        return response.data;
    },
    create: async (data: CreateCategoryDto): Promise<CategoryDto> => {
        const response = await apiClient.post<CategoryDto>('/categories', data);
        return response.data;
    }
};

export const learningSetService = {
    getById: async (id: string): Promise<LearningSetDetailsDto> => {
        const response = await apiClient.get<LearningSetDetailsDto>(`/sets/${id}`);
        return response.data;
    },
    getByCategory: async (categoryId: string): Promise<LearningSetDto[]> => {
        const response = await apiClient.get<LearningSetDto[]>(`/sets/category/${categoryId}`);
        return response.data;
    },
    create: async (data: CreateLearningSetDto): Promise<LearningSetDto> => {
        const response = await apiClient.post<LearningSetDto>('/sets', data);
        return response.data;
    },
    update: async (id: string, data: UpdateLearningSetDto): Promise<LearningSetDetailsDto> => {
        const response = await apiClient.put<LearningSetDetailsDto>(`/sets/${id}`, data);
        return response.data;
    },
};

export const attemptService = {
    start: async (setId: string): Promise<AttemptDto> => {
        const response = await apiClient.post<AttemptDto>(`/attempts/start/${setId}`);
        return response.data;
    },
    getById: async (id: string): Promise<AttemptDto> => {
        const response = await apiClient.get<AttemptDto>(`/attempts/${id}`);
        return response.data;
    },
    submitAnswer: async (attemptId: string, data: SubmitAnswerDto): Promise<AttemptDto> => {
        const response = await apiClient.post<AttemptDto>(`/attempts/${attemptId}/answer`, data);
        return response.data;
    },
    finish: async (attemptId: string): Promise<AttemptDto> => {
        const response = await apiClient.post<AttemptDto>(`/attempts/${attemptId}/finish`);
        return response.data;
    },
    getHistory: async (): Promise<AttemptHistoryDto[]> => {
        const response = await apiClient.get<AttemptHistoryDto[]>('/attempts/history');
        return response.data;
    }
};
