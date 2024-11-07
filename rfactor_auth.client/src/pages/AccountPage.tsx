import React, { useState } from 'react';
import '../styles/Account.css';

import voiceImage from '../../public/gifs/voice_a.gif';
import imageImage from '../../public/gifs/image_a.gif';
import searchImage from '../../public/gifs/search_a.gif';
import sphereImage from '../../public/gifs/sphere_a.gif';

const Account: React.FC = () => {
    const [userInfo, setUserInfo] = useState({
        fullName: 'Фамилия Имя Отчество',
        email: 'example@example.com',
        username: 'Ваш логин',
        password: '*********',
        photo: '../public/images/avatar.png'
    });

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setUserInfo({ ...userInfo, [name]: value });
    };

    const handleButtonClick = () => {
        window.location.href = '/';
    };

    return (
        <div className="user-profile-container">
            <div className="button-container">
                <button onClick={handleButtonClick} className="top-right-button">На выход</button>
            </div>
            <div className="user-profile-section-user">
                <img src={userInfo.photo} alt="User" className="user-photo" />
                <div className="form-group">
                    <label htmlFor="fullName">ФИО:</label>
                    <input
                        type="text"
                        id="fullName"
                        name="fullName"
                        value={userInfo.fullName}
                        onChange={handleInputChange}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="email">Email:</label>
                    <input
                        type="email"
                        id="email"
                        name="email"
                        value={userInfo.email}
                        onChange={handleInputChange}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="username">Логин:</label>
                    <input
                        type="text"
                        id="username"
                        name="username"
                        value={userInfo.username}
                        onChange={handleInputChange}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="password">Пароль:</label>
                    <input
                        type="password"
                        id="password"
                        name="password"
                        value={userInfo.password}
                        onChange={handleInputChange}
                    />
                </div>
            </div>

            <div className="auth-section">
                <div className="user-profile-section">
                    <h2>Основные cпособы аутентификации</h2>
                    <div className="auth-method-tile">
                        <img src={ voiceImage} alt="Auth Method 1" />
                        <div className="auth-method-text">
                            <p>Голосовое снисхождение</p>
                        </div>
                    </div>
                    <div className="auth-method-tile">
                        <img src={imageImage} alt="Auth Method 2" />
                        <div className="auth-method-text">
                            <p>Изображение изображаемого</p>
                        </div>
                    </div>
                </div>

                <div className="user-profile-section">
                    <h2>Дополнительные способы аутентификации</h2>
                    <div className="auth-method-tile">
                        <img src={sphereImage} alt="Auth Method 3" />
                        <div className="auth-method-text">
                            <p>3Dушнина</p>
                        </div>
                    </div>
                    <div className="auth-method-tile">
                        <img src={ searchImage} alt="Auth Method 4" />
                        <div className="auth-method-text">
                            <p>Обход окружения?</p>
                        </div>
                    </div>
                </div>
            </div>

        </div>
    );
};

export default Account;
