import React, { useState } from 'react';
import { browserName } from 'react-device-detect';
import '../styles/AuthForm.css';


interface AuthFormProps {
  onLogin: (username: string, password: string) => void;
}

const AuthForm: React.FC<AuthFormProps> = ({ onLogin }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    onLogin(username, password);
  };

    const handleCancel = () => {
        navigate('/'); 
    };
    function getGreeting() {
        const currentHour = new Date().getHours();

        if (currentHour >= 5 && currentHour < 12) {
            return "Доброе утро";
        } else if (currentHour >= 12 && currentHour < 18) {
            return "Добрый день";
        } else if (currentHour >= 18 && currentHour < 22) {
            return "Добрый вечер";
        } else {
            return "Доброй ночи";
        }
    }

  return (
    <div className="auth-form-container">
      <section className= "authorization">
        <form onSubmit={handleSubmit} >
            <div className="auth-form">
                <h1>{getGreeting()}, {browserName}</h1>
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

export default AuthForm;
