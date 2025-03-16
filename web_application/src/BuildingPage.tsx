import {useEffect, useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';

interface Building {
    id: string;
    name: string;
    address: string;
    postalCode: string;
    country: string;
}
interface Room {
    id: string;
    name: string;
}

const RoomPage = () => {
    const {buildingId} = useParams();

    const [building, setBuilding] = useState<Building>();
    const [rooms, setRooms] = useState<Room[]>([]);
    const [error, setError] = useState('');
    const [roomName, setRoomName] = useState('');
    const [buildingName, setBuildingName] = useState('');
    const [showRoomPopup, setShowRoomPopup] = useState(false);
    const [showBuildingPopup, setShowBuildingPopup] = useState(false);
    const [loading, setLoading] = useState(true);
    const [timer, setTimer] = useState<number | null>(null);

    const navigate = useNavigate();

    useEffect(() => {
        document.title = 'SmartHome - Building';

        if (!localStorage.getItem('jwtToken')) {
            navigate('/login');
        }

        fetchBuilding();
        fetchRooms();
        refreshToken();
    }, [navigate]);

    const refreshToken = async () => {
        try {
            const response = await fetch('http://localhost:5050/users/refresh', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || 'An unknown error occurred.');
                    throw new Error(errorMessage || 'An unknown error occurred.');
                });
            }

            const data = await response.json();
            const jwtToken = data.token;

            if (jwtToken) {
                localStorage.setItem('jwtToken', jwtToken);
            } else {
                setError('Token not found in the response.');
            }
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('An unknown error occurred.');
            }
        }
    }

    const fetchBuilding = async () => {
        setError('');
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || 'Failed to get building.');
                throw new Error(errorMessage || 'Failed to get building.');
            }

            const responseJson = await response.json();

            setBuilding(responseJson);
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('An unknown error occurred');
            }
        } finally {
            setLoading(false);
        }
    }

    const editBuilding = async () => {
        if (!buildingName) {
            setError('Name is required.');
            return;
        }

        setError('');
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
                body: JSON.stringify({"name": buildingName}),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || 'Failed to change name.');
                    throw new Error(errorMessage || 'Failed to change name.');
                });
            }

            setShowBuildingPopup(false);
            await fetchBuilding();
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('An unknown error occurred');
            }
        } finally {
            setLoading(false);
        }
    };

    const deleteBuilding = async () => {
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || 'Failed to delete building.');
                    throw new Error(errorMessage || 'Failed to delete building.');
                });
            }

            navigate('/');
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('An unknown error occurred');
            }
        } finally {
            setLoading(false);
        }
    };

    const fetchRooms = async () => {
        setError('');
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || 'Failed to get rooms.');
                throw new Error(errorMessage || 'Failed to get rooms.');
            }

            const responseText = await response.text();
            const responseJson = responseText ? JSON.parse(responseText) : [];

            if (responseJson.length > 0) {
                setRooms(responseJson);
            } else {
                setError('No rooms found.');
            }
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('An unknown error occurred');
            }
        } finally {
            setLoading(false);
        }
    }

    const addRoom = async () => {
        if (!roomName) {
            setError('Name is required.');
            return;
        }

        setError('');
        setLoading(true);

        try {
            const response = await fetch(`http://localhost:5050/buildings/${buildingId}/rooms`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
                body: JSON.stringify({"name": roomName}),
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || 'Failed to add room.');
                throw new Error(errorMessage || 'Failed to add room.');
            }

            await fetchRooms();
            setShowRoomPopup(false);
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError('An unknown error occurred');
            }
        } finally {
            setLoading(false);
        }
    };

    const handleMouseDown = () => {
        setError('Hold the button for 3 seconds to delete.');
        const newTimer = setTimeout(() => {
            deleteBuilding();
        }, 3000);
        setTimer(newTimer);
    };

    const handleMouseUpOrLeave = () => {
        setError('');
        if (timer) {
            clearTimeout(timer);
            setTimer(null);
        }
    };

    return (
        <div className='home-page'>
            <div className='content-container'>
                {loading ? (
                    <div className="loader"></div>
                ) : building ? (
                    <div>
                        <p>{building.name}</p>
                        <p>{building.address}</p>
                        <p>{building.postalCode}</p>
                        <p>{building.country}</p>
                    </div>
                ) : (
                    <p>No building data available</p>
                )}
                <div className='row'>
                    <button className={'secondary-button'} onClick={() => setShowBuildingPopup(!showBuildingPopup)}>Edit
                        building
                    </button>
                    <button className={'tertiary-button'}
                            onMouseDown={handleMouseDown}
                            onMouseUp={handleMouseUpOrLeave}
                            onMouseLeave={handleMouseUpOrLeave}>
                        Delete building
                    </button>
                </div>
                <h1>Rooms</h1>
                {loading ? (
                    <div className={'loader'}></div>
                ) : (
                    rooms.map((room, index) => (
                        <div className={'row'} key={index} onClick={() => navigate(`/building/${room.id}`)}>
                            <p>{room.name}</p>
                        </div>
                    ))
                )}
                <p className={'error-message'} style={{visibility: error ? 'visible' : 'hidden'}}>{error || ''}</p>
                <button className={'primary-button'} onClick={() => {
                    setShowRoomPopup(!showRoomPopup);
                    setError('')
                }}>Add room
                </button>

                {showRoomPopup && (
                    <>
                        <div className='popup'>
                            <div className='form-container'>
                                <h1>Add room</h1>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    addRoom();
                                }}
                                      noValidate={true}>
                                    <input
                                        type='text'
                                        placeholder='Name'
                                        value={roomName}
                                        onChange={(e) => setRoomName(e.target.value)}
                                    />
                                    <button className={'primary-button form-button'} type='submit' disabled={loading}>
                                        {loading ? 'Adding...' : 'Add'}
                                    </button>
                                </form>
                                <p className={'error-message'}
                                   style={{visibility: error ? 'visible' : 'hidden'}}>{error || ''}</p>
                            </div>
                        </div>
                        <div className='backdrop' onClick={() => {
                            setShowRoomPopup(!showRoomPopup);
                            setError('')
                        }}></div>
                    </>
                )}
                {showBuildingPopup && (
                    <>
                        <div className='popup'>
                            <div className='form-container'>
                                <h1>Edit building</h1>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    editBuilding();
                                }}
                                      noValidate={true}>
                                    <input
                                        type='text'
                                        placeholder='Name'
                                        value={buildingName}
                                        onChange={(e) => setBuildingName(e.target.value)}
                                    />
                                    <button className={'primary-button form-button'} type='submit' disabled={loading}>
                                        {loading ? 'Saving...' : 'Save'}
                                    </button>
                                </form>
                                <p className={'error-message'}
                                   style={{visibility: error ? 'visible' : 'hidden'}}>{error || ''}</p>
                            </div>
                        </div>
                        <div className='backdrop' onClick={() => {
                            setShowBuildingPopup(!showBuildingPopup);
                            setError('')
                        }}></div>
                    </>
                )}
            </div>
        </div>
    );
};

export default RoomPage;