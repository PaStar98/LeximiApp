export interface User {
    id: string;
    username: string;
    email: string;
}

export interface AuthResponse {
    token: string;
    username: string;
    email: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    email: string;
    username: string;
    password: string;
}
