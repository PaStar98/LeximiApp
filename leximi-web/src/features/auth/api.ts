import apiClient from '../../api/client';
import { AuthResponse, LoginRequest, RegisterRequest } from './types';

export const authApi = {
    login: async (data: LoginRequest) => {
        const response = await apiClient.post<AuthResponse>('/auth/login', data);
        return response.data;
    },
    register: async (data: RegisterRequest) => {
        const response = await apiClient.post<AuthResponse>('/auth/register', data);
        return response.data;
    },
    getMe: async () => {
        const response = await apiClient.get<AuthResponse>('/auth/me');
        return response.data;
    },
};
