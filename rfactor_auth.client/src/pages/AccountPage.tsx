import React, { useState, useEffect } from 'react';
import Lottie from 'react-lottie-player';
import '../styles/Account.css';

import voiceImage from '../../public/gifs/voice_a.gif';
import imageImage from '../../public/gifs/image_a.gif';
import searchImage from '../../public/gifs/search_a.gif';
import sphereImage from '../../public/gifs/sphere_a.gif';
import shakeImage from '../assets/shake.json';
import axios from 'axios';

import loadingJson from '../assets/loading.json';

const Account: React.FC = () => {
    const [userInfo, setUserInfo] = useState({
        fullName: '',
        email: '',
        username: '',
        photo: '../public/images/avatar.png'
    });
    const [loading, setLoading] = useState(true);

    const getUserData = async () => {
        try {
            const token = localStorage.getItem("accessToken");
            const response = await axios.get("https://localhost:7109/account/home", {
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
            });

            if (response.status === 200) {
                setUserInfo({
                    fullName: response.data.fio,
                    email: response.data.email,
                    username: response.data.userName,
                    photo: '../public/images/avatar.png'
                });
                setLoading(false);  
            } else {
                window.location.href = '/login';  
            }
        } catch (error) {
            window.location.href = '/login'; 
        }
    };

    useEffect(() => {
        getUserData();
    }, []);

    const handleButtonClick = () => {
        window.location.href = '/';
    };


    const logoutClick = async () => {
        try {
            const token = localStorage.getItem("accessToken");
            const response = await axios.get("https://localhost:7109/account/logout", {
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
            });

            if (response.status === 200) {
                window.location.href = '/';
            } else {
   
            }
        } catch (error) {
            window.location.href = '/login';
        }
    };

    const oauthMethod = async (method : string) => {
        try {
            window.location.href = `https://localhost:7109/api/protected/initiate/${method}?username=${userInfo.username}`; 
        } catch (error) {
            
        }
    };

    const handleVoice = () => oauthMethod("voice");
    const handleImage = () => oauthMethod("image");
    const handleOdonometry = () => oauthMethod("odonometry");
    const handleSphere = () => oauthMethod("sphere");
    const handleEnvironment = () => oauthMethod("environment");

    if (loading) return (
        <div className="auth-form-container">
            <section className="authorization">
                    <div>
                        <Lottie
                            loop
                            animationData={loadingJson}
                            play
                        />
                    </div>

            </section>
        </div>
    ); 

    return (
        <div className="user-profile-container">
            <div className="button-container">
                <button onClick={handleButtonClick} className="top-right-button">В меню</button>
                <button onClick={logoutClick} className="top-right-button">На выход</button>
            </div>
            <div className="user-profile-section-user">
                <img src={userInfo.photo} alt="User" className="user-photo" />
                <div className="form-group">
                    <label htmlFor="fullName">ФИО:</label>
                    <span>{userInfo.fullName}</span>
                </div>
                <div className="form-group">
                    <label htmlFor="email">Email:</label>
                    <span>{userInfo.email}</span>
                </div>
                <div className="form-group">
                    <label htmlFor="username">Логин:</label>
                    <span>{userInfo.username}</span>
                </div>
            </div>

            <div className="auth-section">
                <div className="user-profile-section">
                    <h2>Основные способы аутентификации</h2>
                    <div className="auth-method-tile" onClick={handleVoice}>
                        <img src={voiceImage} alt="Auth Method 1" />
                        <div className="auth-method-text">
                            <p>Голосовое снисхождение</p>
                        </div>
                    </div>
                    <div className="auth-method-tile" onClick={handleImage}>
                        <img src={imageImage} alt="Auth Method 2" />
                        <div className="auth-method-text">
                            <p>Изображение изображаемого</p>
                        </div>
                    </div>
                    <div className="auth-method-tile" onClick={handleOdonometry}>
                        <Lottie
                            loop
                            animationData={shakeImage}
                            play
                            style={{ height: '70px' }}
                        />
                        <div className="auth-method-text">
                            <p>Тряска</p>
                        </div>
                    </div>
                </div>

                <div className="user-profile-section">
                    <h2>Дополнительные способы аутентификации</h2>
                    <div className="auth-method-tile" onClick={handleSphere}>
                        <img src={sphereImage} alt="Auth Method 3" />
                        <div className="auth-method-text">
                            <p>3Dушнина</p>
                        </div>
                    </div>
                    <div className="auth-method-tile">
                        <img src={searchImage} alt="Auth Method 4" onClick={handleEnvironment} />
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
