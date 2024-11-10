import React from 'react';
import '../styles/AboutUs.css';

const HomePage: React.FC = () => {
    return (
        <div className="about-container">
            <div className="team-title"> <h1>���� <span className="blue-text">���</span>.�������<br /> <span className="blue-text">������� �������.</span></h1>
            </div>
            <div className="team-member-container">
                <div className="team-member">
                    <img src="/images/photo1.jpg" alt="Member 1" />
                    <p>������ �����<br />Fullstack<br />(React Native + .NET)</p>
                </div>
                <div className="team-member">
                    <img src="/images/photo2.jpg" alt="Member 2" />
                    <p>������ ��������<br />Backend<br />(Python)</p>
                </div>
                <div className="team-member">
                    <img src="/images/photo3.jpg" alt="Member 3" />
                    <p>������ ��������<br />DevOps, Backend<br />(Python)</p>
                </div>
                <div className="team-member">
                    <img src="/images/photo4.jpg" alt="Member 4" />
                    <p>����� �����������<br />Frontend<br />(React Native)</p>
                </div>
            </div>
        </div>
    );
};

export default HomePage;
