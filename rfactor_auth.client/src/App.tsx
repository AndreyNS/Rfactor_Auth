import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Route, Link, Routes } from 'react-router-dom';
import './App.css';
import VoicePage from './pages/VoiceAuth.tsx';
import Register from './pages/RegisterPage.tsx';

import AuthForm from './pages/AuthForm';
import Header from './pages/Header';
import AboutUs from './pages/AboutUs';

function App() {

    const handleLogin = (username: string, password: string) => {
        console.log('Username:', username);
        console.log('Password:', password);
    };

    return (
        <Router>
          <Header/>
          <div>
            <Routes>
               <Route path="/register" element={<Register/>} />
               <Route path="/voice" element={<VoicePage />} />
               <Route path="/" element={<AuthForm onLogin={handleLogin} />} />
               <Route path="/about" element={<AboutUs />} />   
            </Routes>
          </div>
        </Router>
    );

}

export default App;
