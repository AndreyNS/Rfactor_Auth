import React from 'react';
import { Link } from 'react-router-dom';
import '../styles/Header.css';
import logovtb from '../assets/vtb_logo.svg'
import titlecom from '../assets/titlecommand.png'
import { useNavigate } from 'react-router-dom';

const Header: React.FC = () => {
    const navigate = useNavigate();

    const handleLogoClick = () => {
        navigate('/');
    };

  return (
    <header className="header">
          <div className="logo-container">
              <img src={logovtb} alt="Logo" className="header-logo" onClick={handleLogoClick} />
              <img src={titlecom} alt="Title" className="header-title" />
          </div>
      <nav>
        <ul className="nav-list">
          <li>
            <Link to="/register">Регистрация</Link>
            </li>
            <li>
                <Link to="/login">Вход</Link>
            </li>
            <li>
                <Link to="/voice">Тесты</Link>
            </li>
            <li>
                <Link to="/sphere">сфера</Link>
            </li>
          <li>
            <Link to="/about">О нас</Link>
          </li>
        </ul>
      </nav>
    </header>
  );
};

export default Header;