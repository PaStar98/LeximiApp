import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { authApi } from './api';
import { useAuth } from '../../providers/AuthProvider';

const loginSchema = z.object({
    email: z.string().email('Niepoprawny format email'),
    password: z.string().min(6, 'Hasło musi mieć co najmniej 6 znaków'),
});

type LoginForm = z.infer<typeof loginSchema>;

const LoginPage = () => {
    const navigate = useNavigate();
    const { login: setAuth } = useAuth();

    const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>({
        resolver: zodResolver(loginSchema),
    });

    const mutation = useMutation({
        mutationFn: authApi.login,
        onSuccess: (data) => {
            setAuth(data.token, { id: '', username: data.username, email: data.email });
            navigate('/');
        },
    });

    const onSubmit = (data: LoginForm) => {
        mutation.mutate(data);
    };

    return (
        <div className="auth-card">
            <h2>Logowanie</h2>
            <form onSubmit={handleSubmit(onSubmit)}>
                <div className="form-group">
                    <label>Email</label>
                    <input {...register('email')} type="email" />
                    {errors.email && <span className="error">{errors.email.message}</span>}
                </div>

                <div className="form-group">
                    <label>Hasło</label>
                    <input {...register('password')} type="password" />
                    {errors.password && <span className="error">{errors.password.message}</span>}
                </div>

                {mutation.isError && <div className="error-summary">Błędny email lub hasło</div>}

                <button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Logowanie...' : 'Zaloguj'}
                </button>
            </form>
            <p>
                Nie masz konta? <Link to="/register">Zarejestruj się</Link>
            </p>
        </div>
    );
};

export default LoginPage;
