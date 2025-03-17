import {useEffect, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";
import Stream from "./Stream.tsx";

interface Room {
    name: string;
}

interface Device {
    id: string;
    name: string;
    type: string;
}

const RoomPage = () => {
    const {buildingId} = useParams();
    const {roomId} = useParams();

    const [room, setRoom] = useState<Room>();
    const [device, setDevice] = useState<Device[]>([]);
    const [error, setError] = useState("");
    const [roomName, setRoomName] = useState("");
    const [deviceName, setDeviceName] = useState("");
    const [deviceType, setDeviceType] = useState("");
    const [showRoomPopup, setShowRoomPopup] = useState(false);
    const [showDevicePopup, setShowDevicePopup] = useState(false);
    const [loading, setLoading] = useState(true);
    const [timer, setTimer] = useState<number | null>(null);

    const navigate = useNavigate();

    useEffect(() => {
        document.title = "SmartHome - Building";

        if (!localStorage.getItem("jwtToken")) {
            navigate("/login");
        }

        fetchRoom();
        fetchDevices();
    }, [navigate]);

    const fetchRoom = async () => {
        setError("");
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms/${roomId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || "Failed to get room.");
            }

            const responseJson = await response.json();

            setRoom(responseJson);
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setLoading(false);
        }
    }

    const editRoom = async () => {
        if (!roomName) {
            setError("Name is required.");
            return;
        }

        setError("");
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms/${roomId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
                body: JSON.stringify({"name": roomName}),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to change name.");
                    throw new Error(errorMessage || "Failed to change name.");
                });
            }

            setShowRoomPopup(false);
            await fetchRoom();
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setLoading(false);
        }
    };

    const deleteRoom = async () => {
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms/${roomId}`, {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to delete room.");
                    throw new Error(errorMessage || "Failed to delete room.");
                });
            }

            navigate(`/buildings/${buildingId}`);
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setLoading(false);
        }
    };

    const fetchDevices = async () => {
        setError("");
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms/${roomId}/devices`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || "Failed to get devices.");
            }

            const responseText = await response.text();
            const responseJson = responseText ? JSON.parse(responseText) : [];

            if (responseJson.length > 0) {
                setDevice(responseJson);
            } else {
                setError("No devices found.");
            }
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setLoading(false);
        }
    }

    const addDevice = async () => {
        if (!deviceName) {
            setError("Name is required.");
            return;
        }

        setError("");
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms/${roomId}/devices`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
                body: JSON.stringify({"name": deviceName, "type": deviceType}),
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || "Failed to add room.");
            }

            await fetchDevices();
            setShowDevicePopup(false);
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setLoading(false);
        }
    };

    const handleMouseDown = () => {
        setError("Hold the button for 3 seconds to delete.");
        const newTimer = setTimeout(() => {
            deleteRoom();
        }, 3000);
        setTimer(newTimer);
    };

    const handleMouseUpOrLeave = () => {
        setError("");
        if (timer) {
            clearTimeout(timer);
            setTimer(null);
        }
    };

    return (
        <div className="home-page">
            <div className="content-container">
                {loading ? (
                    <div className="loader"></div>
                ) : room ? (
                    <div>
                        <p>{room.name}</p>
                    </div>
                ) : (
                    <p>No room data available</p>
                )}
                <div className="row">
                    <button className={"secondary-button"}
                            onClick={() => setShowRoomPopup(!showRoomPopup)}>
                        Edit room
                    </button>
                    <button className={"tertiary-button"}
                            onMouseDown={handleMouseDown}
                            onMouseUp={handleMouseUpOrLeave}
                            onMouseLeave={handleMouseUpOrLeave}>
                        Delete room
                    </button>
                </div>

                <h1>Devices</h1>
                {loading ? (
                    <div className={"loader"}></div>
                ) : (
                    device.map((device, index) => (
                        <>
                            <div className={"row"} key={index}
                                 onClick={() => navigate(`/buildings/${buildingId}/rooms/${roomId}/devices/${device.id}`)}>
                                <p>{device.name}</p>
                                <p>{device.type}</p>
                            </div>
                            device.type == "camera" && (
                            <Stream
                                buildingId={parseInt(buildingId ?? "0")}
                                roomId={parseInt(roomId ?? "0")}
                                cameraId={parseInt(device.name ?? "0")}
                            />
                            )
                        </>
                    ))
                )}
                <p className={"error-message"} style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
                <button className={"primary-button"} onClick={() => {
                    setShowDevicePopup(!showDevicePopup);
                    setError("")
                }}>Add device
                </button>

                {showDevicePopup && (
                    <>
                        <div className="popup">
                            <div className="form-container">
                                <h1>Add device</h1>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    addDevice();
                                }}
                                      noValidate={true}>
                                    <input
                                        type="text"
                                        placeholder="Name"
                                        value={deviceName}
                                        onChange={(e) => setDeviceName(e.target.value)}
                                    />
                                    <input
                                        type="text"
                                        placeholder="Type"
                                        value={deviceType}
                                        onChange={(e) => setDeviceType(e.target.value)}
                                    />
                                    <button className={"primary-button form-button"} type="submit" disabled={loading}>
                                        {loading ? "Adding..." : "Add"}
                                    </button>
                                </form>
                                <p className={"error-message"}
                                   style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
                            </div>
                        </div>
                        <div className="backdrop" onClick={() => {
                            setShowDevicePopup(!showDevicePopup);
                            setError("")
                        }}></div>
                    </>
                )}
                {showRoomPopup && (
                    <>
                        <div className="popup">
                            <div className="form-container">
                                <h1>Edit room</h1>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    editRoom();
                                }}
                                      noValidate={true}>
                                    <input
                                        type="text"
                                        placeholder="Name"
                                        value={roomName}
                                        onChange={(e) => setRoomName(e.target.value)}
                                    />
                                    <button className={"primary-button form-button"} type="submit" disabled={loading}>
                                        {loading ? "Saving..." : "Save"}
                                    </button>
                                </form>
                                <p className={"error-message"}
                                   style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
                            </div>
                        </div>
                        <div className="backdrop" onClick={() => {
                            setShowRoomPopup(!showRoomPopup);
                            setError("")
                        }}></div>
                    </>
                )}
            </div>
        </div>
    );
};

export default RoomPage;