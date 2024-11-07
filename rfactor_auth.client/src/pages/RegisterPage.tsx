import React, { useState } from 'react';
import { browserName } from 'react-device-detect';

import '../styles/AuthForm.css';

interface AuthFormProps {
    onLogin: (username: string, password: string) => void;
}

const Register: React.FC<AuthFormProps> = ({ onLogin }) => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [fio, setFio] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showHint, setShowHint] = useState(false); 
    const [animationClass, setAnimationClass] = useState('');

    const handleFioFocus = () => {
        if (!fio) {
            setAnimationClass('hint-animated-in');
            setShowHint(true);
        }
    };

    const handleFioChange = (e) => {
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

    const handleSubmit = (event: React.FormEvent) =>
    {
        event.preventDefault();
        if (password === confirmPassword)
        {
            onRegister(username, password);
        }
        else {
            alert("Пароли не совпадают");
        }
    };




    const handleCancel = () => {
        window.location.href = '/'; };


    return (
        <div className="auth-form-container">
            <section className="authorization">
                <form onSubmit={handleSubmit} >
                    <div className="auth-form">
                        <h1>Добро пожаловать, {browserName}</h1>
                        <input type="text" placeholder="Please set the email" required value={email}
                            onChange={(e) => setEmail(e.target.value)} />
                        <input type="text" placeholder="Введите логин" required value={username}
                            onChange={(e) => setUsername(e.target.value)} />
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
                        <input type="password" placeholder="Введите пароль" required value={password}
                            onChange={(e) => setPassword(e.target.value)} />
                        <input type="password" placeholder="Подтвердите пароль" required value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)} />
                        <button type="submit"
                            className="btnBlue">Зарегистрироваться</button>
                        <button type="button"
                            className="btnWhite" onClick={handleCancel}>Отменить</button>
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
