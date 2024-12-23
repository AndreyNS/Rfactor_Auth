import React, { useState, useEffect } from 'react';
import { browserName } from 'react-device-detect';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import '../styles/AuthForm.css';

interface AuthFormProps {
    onLogin: () => void;
}

const Register: React.FC<AuthFormProps> = ({ onLogin }) => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [fio, setFio] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showHint, setShowHint] = useState(false);
    const [animationClass, setAnimationClass] = useState('');
    const [showError, setShowError] = useState(false); 
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleFioFocus = () => {
        if (!fio) {
            setAnimationClass('hint-animated-in');
            setShowHint(true);
        }
    };

    const handleFioChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFio(e.target.value);
        if (e.target.value) {
            setAnimationClass('hint-animated-out');
            setTimeout(() => setShowHint(false), 300);
        }
    };

    const handleFioBlur = () => {
        setAnimationClass('hint-animated-out');
        setTimeout(() => setShowHint(false), 300);
    };

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        if (password !== confirmPassword) {
            alert("Пароли не совпадают");
            return;
        }

        try {
            const response = await axios.post('https://localhost:7109/account/register', {
                username,
                email,
                fio,
                password,
                confirmPassword
            });

            localStorage.setItem('accessToken', response.data.accessToken);
            localStorage.setItem('refreshToken', response.data.refreshToken);

            onLogin();
            navigate('/account');
        } catch (error) {
            if (axios.isAxiosError(error) && error.response) {
                setError(error.response.data);
            } else {
                setError("Ошибка соединения. Проверьте подключение к интернету.");
            }
            setShowError(true);
        }
    };

    useEffect(() => {
        if (showError) {
            const timer = setTimeout(() => {
                setShowError(false);
            }, 3000);
            return () => clearTimeout(timer);
        }
    }, [showError]);

    const handleCancel = () => {
        window.location.href = '/';
    };

    return (
        <div className="auth-form-container">
            <section className="authorization">
                <form onSubmit={handleSubmit} >
                    <div className="auth-form">
                        <h1>Добро пожаловать, {browserName}</h1>
                        <p className={`error-message ${showError ? 'show' : 'hide'}`}>{error}</p>
                        <input
                            type="text"
                            placeholder="Введите email"
                            required
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                        <input
                            type="text"
                            placeholder="Введите логин"
                            required
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                        />
                        <div className="input-with-hint">
                            <input
                                type="text"
                                placeholder="Введите ФИО"
                                required
                                value={fio}
                                onChange={handleFioChange}
                                onFocus={handleFioFocus}
                                onBlur={handleFioBlur}
                            />
                            {showHint && (
                                <div className={`hint ${animationClass}`}>
                                    Введите ФИО в любом порядке и на любом языке
                                </div>
                            )}
                        </div>
                        <input
                            type="password"
                            placeholder="Введите пароль"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        <input
                            type="password"
                            placeholder="Подтвердите пароль"
                            required
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                        <button type="submit" className="btnBlue">Зарегистрироваться</button>
                        <button type="button" className="btnWhite" onClick={handleCancel}>Отменить</button>
                    </div>
                </form>
            </section>
        </div>
    );
};

export default Register;
