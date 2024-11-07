import React, { useMemo, useState, useRef, useEffect, useCallback } from 'react';
import { Canvas, useFrame } from '@react-three/fiber';
import { OrbitControls, Html } from '@react-three/drei';
import * as THREE from 'three';

interface Point {
    position: [number, number, number];
    label: number;
}


const Globe: React.FC<{
    selectedPoints: Point[];
    setSelectedPoints: React.Dispatch<React.SetStateAction<Point[]>>;
    isSending: boolean; }> = ({ selectedPoints, setSelectedPoints, isSending }) => {
    const radius = 2.8;
    const [isRotating, setIsRotating] = useState(true);
    const timerRef = useRef(0)

    const points = useMemo(() => {
        const pts: Point[] = [];
        const latStep = Math.PI / 8;
        const longStep = Math.PI / 8;
        let index = 1;

        for (let lat = -Math.PI / 2 + latStep; lat <= Math.PI / 2 - latStep; lat += latStep) {
            for (let lon = 0; lon < Math.PI * 2; lon += longStep) {
                const x = radius * Math.cos(lat) * Math.cos(lon);
                const y = radius * Math.sin(lat);
                const z = radius * Math.cos(lat) * Math.sin(lon);
                pts.push({ position: [x, y, z], label: index });
                index++;
            }
        }
        return pts;
    }, [radius]);

    const handleClick = (point: Point) => {
        if (isSending) return; 
        setSelectedPoints((prev) => {
            const exists = prev.find(p => p.label === point.label);
            if (exists) {
                return prev.filter(p => p.label !== point.label);
            } else {
                return [...prev, point];
            }
        });

        clickedSphere();
    };


    const startTimer = useCallback((delay : Int16Array) => {
        if (timerRef.current)
        {
            clearTimeout(timerRef.current);
        }
        timerRef.current = setTimeout(() =>
        {
            setIsRotating(true);
        }, delay);
    }, []);


    const clickedSphere = () => {
        setIsRotating(false);
        startTimer(10000);       
    }

    const computeArcPoints = (start: THREE.Vector3, end: THREE.Vector3, segments = 50) => {
        const arcPoints = [];
        const angle = start.angleTo(end);
        const axis = new THREE.Vector3().crossVectors(start, end).normalize();

        for (let i = 0; i <= segments; i++) {
            const t = i / segments;
            const vector = start.clone().applyAxisAngle(axis, angle * t).normalize().multiplyScalar(radius);
            arcPoints.push(vector);
        }
        return arcPoints;
    };

    const groupRef = useRef<THREE.Group>(null);
    const [sphereScale, setSphereScale] = useState(1);

    useFrame((state, delta) => {
        if (isSending && groupRef.current) {
            groupRef.current.rotation.y += delta * 5;

            if (sphereScale > 0.7) {
                setSphereScale(prev => Math.max(prev - delta * 0.2, 0.7));
            }

            groupRef.current.scale.set(sphereScale, sphereScale, sphereScale);
        } else if (groupRef.current) {

            if (sphereScale < 1) {
                setSphereScale(prev => Math.min(prev + delta * 0.2, 1));
                groupRef.current.scale.set(sphereScale, sphereScale, sphereScale);
            }

            if (isRotating && groupRef.current) {
                groupRef.current.rotation.y += delta * 0.1;
            }
        }
    });

    return (
        <group ref={groupRef}>
            <mesh onClick={clickedSphere }>
                <sphereGeometry args={[radius, 32, 32]} />
                <meshPhysicalMaterial
                    color="#000000"
                    transparent
                    opacity={0.3}
                    roughness={0.9}
                    metalness={0.0}
                    transmission={0.8}
                    thickness={0.5}
                />
            </mesh>

            {!isSending && points.map((point, idx) => (
                <mesh
                    key={idx}
                    position={point.position}
                    onClick={() => handleClick(point)}
                >
                    <sphereGeometry args={[0.05, 8, 8]} />
                    <meshStandardMaterial
                        color={
                            selectedPoints.some(p => p.label === point.label)
                                ? "#6FF233"
                                : "#3A6FFF"
                        }
                    />
                    <Html position={[0.1, 0.15, 0]}>
                        <div
                            onClick={(e) => {
                                e.stopPropagation();
                                handleClick(point);
                            }}
                            style={{
                                color: 'white',
                                fontSize: '12px',
                                pointerEvents: 'auto',
                                userSelect: 'none',
                            }}
                        >
                            {point.label}
                        </div>
                    </Html>
                </mesh>
            ))}

            {selectedPoints.length >= 2 &&
                selectedPoints.map((point, idx) => {
                    if (idx === 0) return null;
                    const start = new THREE.Vector3(...selectedPoints[idx - 1].position);
                    const end = new THREE.Vector3(...point.position);
                    const arcPoints = computeArcPoints(start, end);

                    return (
                        <mesh key={`line-${idx}`}>
                            <tubeGeometry
                                args={[
                                    new THREE.CatmullRomCurve3(arcPoints),
                                    64,
                                    0.02,
                                    8,
                                    false,
                                ]}
                            />
                            <meshStandardMaterial color="#FF0000" />
                        </mesh>
                    );
                })}
        </group>
    );
};

const SpherePage: React.FC = () => {
    const [selectedPoints, setSelectedPoints] = useState<Point[]>([]);
    const [isSending, setIsSending] = useState(false);

    const handleClear = () => {
        setSelectedPoints([]);
        setIsSending(false);
    };

    const handleSend = () => {
        setIsSending(true);

        setTimeout(() => {
            setIsSending(false);
            setSelectedPoints([]); 
        }, 5000);
    };

    return (
        <div style={{ position: 'relative', width: '100%', height: '95vh' }}>
            <Canvas>
                <ambientLight intensity={0.5} />
                <pointLight position={[5, 5, 5]} />
                <Globe
                    selectedPoints={selectedPoints}
                    setSelectedPoints={setSelectedPoints}
                    isSending={isSending}
                />
                <OrbitControls enableZoom={false} enablePan={!isSending} enableRotate={!isSending} />
            </Canvas>
            <div
                style={{
                    position: 'absolute',
                    top: 20,
                    right: 20,
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '10px',
                }}
            >
                <button onClick={handleSend}>Отправить</button>
                <button onClick={handleClear}>Очистить</button>
            </div>
        </div>
    );
};

export default SpherePage;
