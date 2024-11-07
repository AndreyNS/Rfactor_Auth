import React, { useState } from 'react';
import './UserProfile.css';

const UserProfile: React.FC = () => {
  const [userInfo, setUserInfo] = useState({
    fullName: 'Фамилия Имя Отчество',
    email: 'example@example.com',
    username: 'Ваш логин',
    password: '*********',
    photo: '/images/default-user.jpg' 
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setUserInfo({ ...userInfo, [name]: value });
  };

  const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      const reader = new FileReader();
      reader.onloadend = () => {
        setUserInfo({ ...userInfo, photo: reader.result as string });
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSave = () => {
    console.log('User info saved', userInfo);
  };

  return (
    <div className="user-profile-container">
      <div className="user-profile-section">
        <img src={userInfo.photo} alt="User" className="user-photo" />
        <input type="file" onChange={handlePhotoChange} />
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
        <button onClick={handleSave} className="save-button">Сохранить</button>
      </div>
      <div className="user-profile-section">
        <h2>Способы аутентификации</h2>
        <div className="form-group">
          <label htmlFor="authMethod1">Метод 1:</label>
          <input type="text" id="authMethod1" />
        </div>
        <div className="form-group">
          <label htmlFor="authMethod2">Метод 2:</label>
          <input type="text" id="authMethod2" />
        </div>
        <div className="form-group">
          <label htmlFor="authMethod3">Метод 3:</label>
          <input type="text" id="authMethod3" />
        </div>
      </div>
      <div className="user-profile-section">
        <h2>Дополнительные способы аутентификации</h2>
        <div className="form-group">
          <label htmlFor="authMethod4">Метод 4:</label>
          <input type="text" id="authMethod4" />
        </div>
        <div className="form-group">
          <label htmlFor="authMethod5">Метод 5:</label>
          <input type="text" id="authMethod5" />
        </div>
      </div>
    </div>
  );
};

export default UserProfile;
