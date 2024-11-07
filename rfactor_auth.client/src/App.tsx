import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Route, Link, Routes, useLocation } from 'react-router-dom';
import './App.css';
import VoicePage from './pages/VoiceAuth.tsx';
import Register from './pages/RegisterPage.tsx';
import Account from './pages/AccountPage.tsx';
import Sphere from './pages/SpherePage.tsx';

import AuthForm from './pages/AuthForm';
import Header from './pages/Header';
import AboutUs from './pages/AboutUs';

function App() {
    return (
        <Router>
            <MainContent />
        </Router>
    );
}

function MainContent() {
    const location = useLocation();

    const isHeaderVisible = location.pathname !== '/account';

    return (
        <>
            {isHeaderVisible && <Header />}
            <div className="content-container">
                <Routes>
                    <Route path="/register" element={<Register />} />
                    <Route path="/voice" element={<VoicePage />} />
                    <Route path="/account" element={<Account />} />
                    <Route path="/sphere" element={<Sphere />} />
                    <Route path="/" element={<AuthForm />} />
                    <Route path="/about" element={<AboutUs />} />
                </Routes>
            </div>
        </>
    );
}

export default App;
