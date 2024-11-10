import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Route, Routes, useLocation, Navigate } from 'react-router-dom';

import VoicePage from './pages/VoiceAuth';
import Register from './pages/RegisterPage';
import Account from './pages/AccountPage';
import Sphere from './pages/SpherePage';
import Autherization from './pages/AutherizationPage';
import Header from './pages/Header';
import AboutUs from './pages/AboutUs';
import HomePage from './pages/HomePage';
import './App.css';

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);

    const onLogin = () => {
        setIsAuthenticated(true);
    };

    return (
        <Router>
            <MainContent isAuthenticated={isAuthenticated} onLogin={onLogin} />
        </Router>
    );
}

interface MainContentProps {
    isAuthenticated: boolean;
    onLogin: () => void;
}

function MainContent({ isAuthenticated, onLogin }: MainContentProps) {
    const location = useLocation();
    const isHeaderVisible = location.pathname !== '/account';

    return (
        <>
            {isHeaderVisible && <Header />}
            <div className="content-container">
                <Routes>
                    <Route
                        path="/register"
                        element={isAuthenticated ? <Navigate to="/account" /> : <Register onLogin={onLogin} />}
                    />
                    <Route
                        path="/login"
                        element={isAuthenticated ? <Navigate to="/login" /> : <Autherization onLogin={onLogin} />}
                    />

                    <Route path="/voice" element={<VoicePage />} />
                    <Route path="/" element={<HomePage />} />
                    <Route path="/sphere" element={<Sphere />} />
                    <Route path="/about" element={<AboutUs />} />

                    <Route
                        path="/account"
                        element={<Account /> }
                    />
                </Routes>
            </div>
        </>
    );
}

export default App;
