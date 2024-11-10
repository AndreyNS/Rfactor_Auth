import React, { useState, useEffect } from 'react';
import { browserName } from 'react-device-detect';
import { useNavigate } from 'react-router-dom';
import Lottie from 'react-lottie-player';
import axios from 'axios';
import '../styles/AuthForm.css';
import loadingJson from '../assets/loading.json';


interface AutherizationPageProps {
    onLogin: () => void;
}

const AutherizationPage: React.FC<AutherizationPageProps> = ({ onLogin }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [showError, setShowError] = useState(false);
    const [loading, setLoading] = useState<boolean>(true);
    const navigate = useNavigate();

    const checkAuth = async () => {
        try {
            const token = localStorage.getItem("accessToken");
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 5000); 

            const response = await axios.get("https://localhost:7109/account/authorize",
                {
                    headers:
                    {
                        "Authorization": `Bearer ${token}`,
                        "Content-Type": "application/json"
                    },
                    signal: controller.signal
                });

            clearTimeout(timeoutId);

            if (response.status === 200) {
                setTimeout(() => {
                    onLogin();
                    navigate('/account');   
                }, 2000); 
                
            } else {
                setLoading(false);
            }
        } catch (error) {
            console.error("Пользователь не авторизован или токен истек", error);
            setLoading(false);
        }
    };

    useEffect(() => {
        checkAuth();
    }, []);

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();
        setLoading(true);

        try {
            const response = await axios.post('https://localhost:7109/account/login', {
                username,
                password,
            });
            localStorage.setItem('accessToken', response.data.accessToken);
            localStorage.setItem('refreshToken', response.data.refreshToken);

            onLogin();
            navigate('/account');
        } catch (error) {
            setError('Неверный логин или пароль');
            setShowError(true);
            setLoading(false);
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
        navigate('/');
    };

    function getGreeting() {
        const currentHour = new Date().getHours();

        if (currentHour >= 5 && currentHour < 12) {
            return 'Доброе утро';
        } else if (currentHour >= 12 && currentHour < 18) {
            return 'Добрый день';
        } else if (currentHour >= 18 && currentHour < 22) {
            return 'Добрый вечер';
        } else {
            return 'Доброй ночи';
        }
    }

    return (
        <div className="auth-form-container">
            <section className="authorization">
                {loading ? (
                    <div>
                        <Lottie
                            loop
                            animationData={loadingJson}
                            play
                        />
                    </div>
                ) : (
                    <form onSubmit={handleSubmit}>
                        <div className="auth-form">
                            <h1>{getGreeting()}, {browserName}</h1>
                            <p className={`error-message ${showError ? 'show' : 'hide'}`}>{error}</p>
                            <input
                                type="text"
                                placeholder="Введите логин"
                                required
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                            />
                            <input
                                type="password"
                                placeholder="Введите пароль"
                                required
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            <button type="submit" className="btnBlue">Войти</button>
                            <button type="button" className="btnWhite" onClick={handleCancel}>Отменить</button>
                        </div>
                    </form>
                )}
            </section>
        </div>
    );

};

export default AutherizationPage;
