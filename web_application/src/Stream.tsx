import {useEffect, useRef, useState} from "react";

interface StreamProps {
    buildingId: number;
    roomId: number;
    cameraId: number;
}

function Stream({buildingId, roomId, cameraId}: StreamProps) {
    const [imageSrc, setImageSrc] = useState<string | null>(null);
    const ws = useRef<WebSocket | null>(null);

    useEffect(() => {
        const connectWebSocket = () => {
            ws.current = new WebSocket("ws://localhost:5050/receive");

            ws.current.onopen = () => {
                console.log("WebSocket connection established.");
                const message = {
                    token: localStorage.getItem("jwtToken"),
                    building_id: buildingId,
                    room_id: roomId,
                    camera_id: cameraId,
                };
                ws.current?.send(JSON.stringify(message));
            };

            ws.current.onmessage = (event) => {
                if (event.data instanceof Blob) {
                    const url = URL.createObjectURL(event.data);
                    setImageSrc((prevUrl) => {
                        if (prevUrl) {
                            URL.revokeObjectURL(prevUrl);
                        }
                        return url;
                    });
                }
            };

            ws.current.onerror = (error) => {
                console.error("WebSocket error:", error);
            };

            ws.current.onclose = () => {
                console.warn("WebSocket disconnected. Reconnecting...");
                setTimeout(connectWebSocket, 2000);
            };
        };

        connectWebSocket();

        return () => {
            if (ws.current) {
                ws.current.close();
            }
            if (imageSrc) URL.revokeObjectURL(imageSrc);
        };
    }, [buildingId, roomId, cameraId]);

    return (
        <div>
            {imageSrc ? (
                <img
                    src={imageSrc}
                    alt="Stream"
                    style={{width: "600px", height: "auto"}}
                />
            ) : (
                <p>Waiting for stream...</p>
            )}
        </div>
    );
}

export default Stream;
