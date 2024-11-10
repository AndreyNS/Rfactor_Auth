import React from 'react';
import '../styles/AboutUs.css';

const HomePage: React.FC = () => {
    return (
        <div className="about-container">
            <div className="team-title"> <h1>Трек <span className="blue-text">ЕПА</span>.КОМАНДА<br /> <span className="blue-text">Пожилая Ряженка.</span></h1>
            </div>
            <div className="team-member-container">
                <div className="team-member">
                    <img src="/images/photo1.jpg" alt="Member 1" />
                    <p>Андрей Радин<br />Fullstack<br />(React Native + .NET)</p>
                </div>
                <div className="team-member">
                    <img src="/images/photo2.jpg" alt="Member 2" />
                    <p>Кирилл Шинкарук<br />Backend<br />(Python)</p>
                </div>
                <div className="team-member">
                    <img src="/images/photo3.jpg" alt="Member 3" />
                    <p>Матвей Алексеев<br />DevOps, Backend<br />(Python)</p>
                </div>
                <div className="team-member">
                    <img src="/images/photo4.jpg" alt="Member 4" />
                    <p>Антон Поляковский<br />Frontend<br />(React Native)</p>
                </div>
            </div>
        </div>
    );
};

export default HomePage;
