import {useEffect, useRef, useState} from "react";

function Stream() {
    const [imageSrc, setImageSrc] = useState<string | null>(null);
    const ws = useRef<WebSocket | null>(null);

    useEffect(() => {
        const connectWebSocket = () => {
            ws.current = new WebSocket("ws://localhost:5050/receive");

            ws.current.onopen = () => {
                console.log("WebSocket connection established.");
                const message = {
                    token: localStorage.getItem("jwtToken"),
                    building_id: 1,
                    room_id: 1,
                    camera_id: 0,
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
    }, []);

    return (
        <div>
            {imageSrc ? (
                <img src={imageSrc} alt="Stream" style={{width: "100%"}}/>
            ) : (
                <p>Waiting for stream...</p>
            )}
        </div>
    );
}

export default Stream;
