import {useEffect, useRef, useState} from "react";

function Stream() {
    const [imageSrc, setImageSrc] = useState<string | null>(null);
    const ws = useRef<WebSocket | null>(null);

    useEffect(() => {
        const connectWebSocket = () => {
            ws.current = new WebSocket("ws://localhost:5050/receive");

            ws.current.onopen = () => {
                console.log("WebSocket connection established.");
                // Send a message when connection is opened
                const message = {
                    token: localStorage.getItem("jwtToken"),
                    selected_id: 0,
                };
                ws.current?.send(JSON.stringify(message)); // Make sure you send a JSON object
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
                setTimeout(connectWebSocket, 2000); // Attempt reconnect after 2 seconds
            };
        };

        connectWebSocket(); // Initial connection

        return () => {
            // Cleanup when component unmounts
            if (ws.current) {
                ws.current.close();
            }
            if (imageSrc) URL.revokeObjectURL(imageSrc);
        };
    }, []); // Empty dependency array ensures this runs only once


    return (
        <div>
            <h2>Live Stream</h2>
            {imageSrc ? (
                <img src={imageSrc} alt="Stream" style={{width: "100%"}}/>
            ) : (
                <p>Waiting for stream...</p>
            )}
        </div>
    );
}

export default Stream;
