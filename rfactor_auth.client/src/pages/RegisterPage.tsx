import React from 'react';

export function login() {
    window.location.href = "https://localhost:7109/api/VoiceAuth/initiate";
}

function Register() {



    return (
        <div>
            <h1>��� ����������</h1>
            <button onClick={login}>�����</button>
        </div>
    );
}

export default Register;
