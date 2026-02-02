import apiClient from '../../api/client';
import { Category, LearningSet } from '../../types';

export const catalogApi = {
    getCategories: async () => {
        const response = await apiClient.get<Category[]>('/categories');
        return response.data;
    },
    getSetsByCategory: async (categoryId: string) => {
        const response = await apiClient.get<LearningSet[]>(`/sets/category/${categoryId}`);
        return response.data;
    },
    getSetById: async (id: string) => {
        const response = await apiClient.get<LearningSet>(`/sets/${id}`);
        return response.data;
    }
};
