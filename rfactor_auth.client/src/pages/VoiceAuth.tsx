import React, { useState, useEffect, useRef } from 'react';
import Config from "react-native-config";

const VoiceAuth: React.FC = () => {
    const [recording, setRecording] = useState<boolean>(false);
    const [mediaRecorder, setMediaRecorder] = useState<MediaRecorder | null>(null);
    const [audioBlob, setAudioBlob] = useState<Blob | null>(null);
    const [audioContext, setAudioContext] = useState<AudioContext | null>(null);
    const [analyser, setAnalyser] = useState<AnalyserNode | null>(null);
    const [audioData, setAudioData] = useState<Uint8Array | null>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    useEffect(() => {
        if (analyser) {
            const bufferLength = analyser.frequencyBinCount;
            const dataArray = new Uint8Array(bufferLength);
            setAudioData(dataArray);
            draw();
        }
    }, [analyser]);

    const startRecording = async () => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const audioCtx = new AudioContext();
            const source = audioCtx.createMediaStreamSource(stream);
            const analyserNode = audioCtx.createAnalyser();
            source.connect(analyserNode);

            setAudioContext(audioCtx);
            setAnalyser(analyserNode);

            const recorder = new MediaRecorder(stream);
            recorder.ondataavailable = (event) => {
                setAudioBlob(event.data);
            };

            recorder.start();
            setMediaRecorder(recorder);
            setRecording(true);
            console.log('Recording started');
        } catch (err) {
            console.error('Failed to start recording', err);
        }
    };

    const stopRecording = () => {
        if (mediaRecorder) {
            mediaRecorder.stop();
            setMediaRecorder(null);
            setRecording(false);
            if (audioContext) {
                audioContext.close();
            }
            console.log('Recording stopped');
        }
    };

    const sendAudioToBackend = async (blob: Blob) => {
        const formData = new FormData();
        const apiUrl = Config.BACKEND_URL;
        console.log(`API URL: ${apiUrl}`);

        formData.append('audio', blob, 'recording.wav');

        const response = await fetch('https://your-backend-url/api/voiceauth', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        console.log('Response from backend', data);
    };

    const draw = () => {
        if (!analyser || !audioData || !canvasRef.current) {
            return;
        }

        const canvas = canvasRef.current;
        const canvasCtx = canvas.getContext('2d');

        const drawVisual = () => {
            if (!analyser || !audioData || !canvasCtx) {
                return;
            }

            requestAnimationFrame(drawVisual);

            analyser.getByteFrequencyData(audioData);
            canvasCtx.fillStyle = 'rgb(200, 200, 200)';
            canvasCtx.fillRect(0, 0, canvas.width, canvas.height);

            const barWidth = (canvas.width / audioData.length) * 2.5;
            let barHeight;
            let x = 0;

            for (let i = 0; i < audioData.length; i++) {
                barHeight = audioData[i];
                canvasCtx.fillStyle = 'rgb(' + (barHeight + 100) + ',50,50)';
                canvasCtx.fillRect(x, canvas.height - barHeight / 2, barWidth, barHeight / 2);

                x += barWidth + 1;
            }
        };

        drawVisual();
    };

    return (
        <div>
            <button onClick={startRecording} disabled={recording}>Start Voice Auth</button>
            {recording && <button onClick={stopRecording}>Stop Recording</button>}
            {!recording && audioBlob && <button onClick={() => sendAudioToBackend(audioBlob)}>Send Recording</button>}
            <div>
                <canvas ref={canvasRef} width="600" height="100"></canvas>
            </div>
        </div>
    );
};

export default VoiceAuth;
