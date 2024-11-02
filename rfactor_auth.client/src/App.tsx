import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Route, Link, Routes } from 'react-router-dom';
import './App.css';
import VoicePage from './pages/VoiceAuth.tsx';

interface Forecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();

    useEffect(() => {
        populateWeatherData();
    }, []);

    const contents = forecasts === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Summary</th>
                </tr>
            </thead>
            <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.date}>
                        <td>{forecast.date}</td>
                        <td>{forecast.temperatureC}</td>
                        <td>{forecast.temperatureF}</td>
                        <td>{forecast.summary}</td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <Router>
            <div>
                <nav>
                    <ul>
                        <li><Link to="/">Home</Link></li>
                        <li><Link to="/about">About</Link></li>
                    </ul>
                </nav>
                <Routes>
                    <Route path="/" element={
                        <div>
                            <h1 id="tableLabel">Weather forecast</h1>
                            <p>This component demonstrates fetching data from the server.</p>
                            {contents}
                        </div>
                    } />
                    <Route path="/about" element={<VoicePage />} />
                </Routes>
            </div>
        </Router>
    );

    async function populateWeatherData() {
        //const response = await fetch('weatherforecast');
        //const data = await response.json();
        //setForecasts(data);
    }
}

function About() {
    return (
        <div>
            <h2>About</h2>
            <p>This is the About page.</p>
        </div>
    );
}

export default App;
