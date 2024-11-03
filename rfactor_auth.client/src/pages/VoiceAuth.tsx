import React, { useState, useEffect, useRef } from 'react';

const VoiceAuth: React.FC = () => {
    const [recording, setRecording] = useState<boolean>(false);
    const mediaRecorderRef = useRef<MediaRecorder | null>(null);
    const [audioBlob, setAudioBlob] = useState<Blob | null>(null);
    const [audioContext, setAudioContext] = useState<AudioContext | null>(null);
    const [analyser, setAnalyser] = useState<AnalyserNode | null>(null);
    const [audioData, setAudioData] = useState<Uint8Array | null>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const silenceThreshold = 5;  // Порог тишины
    const silenceDuration = 2000;  // Длительность тишины (3 секунды)
    let silenceTimer: NodeJS.Timeout | null = null;

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
            mediaRecorderRef.current = recorder;
            setRecording(true);
            console.log('Recording started');

            monitorSilence(analyserNode);
        } catch (err) {
            console.error('Failed to start recording', err);
        }
    };

    const stopRecording = () => {
        if (mediaRecorderRef.current) {
            mediaRecorderRef.current.stop();
            mediaRecorderRef.current = null;
            setRecording(false);
            if (audioContext) {
                audioContext.close();
            }
            console.log('Recording stopped');
        }
    };

    const monitorSilence = (analyserNode: AnalyserNode) => {
        const bufferLength = analyserNode.frequencyBinCount;
        const dataArray = new Uint8Array(bufferLength);

        const checkSilence = () => {
            analyserNode.getByteFrequencyData(dataArray);

            const averageVolume = dataArray.reduce((sum, value) => sum + value, 0) / dataArray.length;
            if (averageVolume < silenceThreshold) {
                if (!silenceTimer) {
                    silenceTimer = setTimeout(() => {
                        stopRecording();
                        silenceTimer = null;
                    }, silenceDuration);
                }
            } else if (silenceTimer) {
                clearTimeout(silenceTimer);
                silenceTimer = null;
            }

            if (recording) {
                requestAnimationFrame(checkSilence);
            }
        };

        checkSilence();
    };

    const sendAudioToBackend = async (blob: Blob) => {
        const formData = new FormData();
        formData.append('audio', blob);

        const statusElement = document.createElement('div');
        statusElement.style.position = 'fixed';
        statusElement.style.top = '10px';
        statusElement.style.right = '10px';
        statusElement.style.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        statusElement.style.color = 'white';
        statusElement.style.padding = '10px';
        statusElement.style.borderRadius = '5px';
        statusElement.style.zIndex = '1000';
        statusElement.innerText = 'Uploading...';
        document.body.appendChild(statusElement);

        try {
            const response = await fetch('https://localhost:7109/api/VoiceAuth/setvoice', {
                method: 'POST',
                body: formData
            });

            const data = await response.json();
            console.log('Response from backend', data);

            if (response.ok) {
                statusElement.innerText = 'Upload successful!';
            } else {
                statusElement.innerText = 'Upload failed!';
            }
        } catch (error) {
            console.error('Error uploading audio:', error);
            statusElement.innerText = 'Upload failed!';
        } finally {
            setTimeout(() => {
                document.body.removeChild(statusElement);
            }, 3000);
        }
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

    const playRecording = () => {
        if (audioBlob) {
            const audioUrl = URL.createObjectURL(audioBlob);
            const audio = new Audio(audioUrl);
            audio.play();
        }
    };

    return (
        <div>
            <button onClick={startRecording} disabled={recording}>Start Voice Auth</button>
            {recording && <button onClick={stopRecording}>Stop Recording</button>}
            {!recording && audioBlob && <button onClick={() => sendAudioToBackend(audioBlob)}>Send Recording</button>}
            {!recording && audioBlob && <button onClick={playRecording}>Play Recording</button>}
            <div>
                <canvas ref={canvasRef} width="600" height="100"></canvas>
            </div>
        </div>
    );
};

export default VoiceAuth;
