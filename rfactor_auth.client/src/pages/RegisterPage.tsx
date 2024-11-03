import React from 'react';

export function login() {
    window.location.href = "https://localhost:7109/api/VoiceAuth/initiate";
}

function Register() {



    return (
        <div>
            <h1>Мое приложение</h1>
            <button onClick={login}>Войти</button>
        </div>
    );
}

export default Register;
