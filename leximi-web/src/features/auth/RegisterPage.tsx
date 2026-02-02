import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { authApi } from './api';
import { useAuth } from '../../providers/AuthProvider';

const registerSchema = z.object({
    email: z.string().email('Niepoprawny format email'),
    username: z.string().min(3, 'Nazwa użytkownika musi mieć co najmniej 3 znaki'),
    password: z.string().min(6, 'Hasło musi mieć co najmniej 6 znaków'),
    confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
    message: "Hasła nie są identyczne",
    path: ["confirmPassword"],
});

type RegisterForm = z.infer<typeof registerSchema>;

const RegisterPage = () => {
    const navigate = useNavigate();
    const { login: setAuth } = useAuth();

    const { register, handleSubmit, formState: { errors } } = useForm<RegisterForm>({
        resolver: zodResolver(registerSchema),
    });

    const mutation = useMutation({
        mutationFn: authApi.register,
        onSuccess: (data) => {
            setAuth(data.token, { id: '', username: data.username, email: data.email });
            navigate('/');
        },
    });

    const onSubmit = (data: RegisterForm) => {
        mutation.mutate(data);
    };

    return (
        <div className="auth-card">
            <h2>Rejestracja</h2>
            <form onSubmit={handleSubmit(onSubmit)}>
                <div className="form-group">
                    <label>Email</label>
                    <input {...register('email')} type="email" />
                    {errors.email && <span className="error">{errors.email.message}</span>}
                </div>

                <div className="form-group">
                    <label>Nazwa użytkownika</label>
                    <input {...register('username')} type="text" />
                    {errors.username && <span className="error">{errors.username.message}</span>}
                </div>

                <div className="form-group">
                    <label>Hasło</label>
                    <input {...register('password')} type="password" />
                    {errors.password && <span className="error">{errors.password.message}</span>}
                </div>

                <div className="form-group">
                    <label>Powtórz hasło</label>
                    <input {...register('confirmPassword')} type="password" />
                    {errors.confirmPassword && <span className="error">{errors.confirmPassword.message}</span>}
                </div>

                {mutation.isError && <div className="error-summary">Wystąpił błąd podczas rejestracji</div>}

                <button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Rejestracja...' : 'Zarejestruj'}
                </button>
            </form>
            <p>
                Masz już konto? <Link to="/login">Zaloguj się</Link>
            </p>
        </div>
    );
};

export default RegisterPage;
