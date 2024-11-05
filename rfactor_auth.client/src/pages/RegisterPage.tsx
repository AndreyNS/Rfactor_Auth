import React, { useState } from 'react';
import '../styles/AuthForm.css';

interface AuthFormProps {
    onLogin: (username: string, password: string) => void;
}

const Register: React.FC<AuthFormProps> = ({ onLogin }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    const handleSubmit = (event: React.FormEvent) => {
        event.preventDefault();
        onLogin(username, password);
    };

    const handleCancel = () => {
        navigate('/'); // Переход на главную страницу
    };

    return (
        <div className="auth-form-container">
            <section className="authorization">
                <form onSubmit={handleSubmit} >
                    <div className="auth-form">
                        <h1>Добро пожаловать</h1>
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
            </section>
        </div>
    );
};

export default Register;

















//import React from 'react';

//export function login() {
//    window.location.href = "https://localhost:7109/api/VoiceAuth/initiate";
//}

//function Register() {



//    return (
//        <div>
//            <h1>Мое приложение</h1>
//            <button onClick={login}>Войти</button>
//        </div>
//    );
//}

//export default Register;
