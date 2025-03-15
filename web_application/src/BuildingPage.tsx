import {useEffect, useState} from 'react';
import {useNavigate, useParams} from 'react-router-dom';

interface Room {
    id: string;
    name: string;
}

const RoomPage = () => {
    const {buildingId} = useParams();

    const [rooms, setRooms] = useState<Room[]>([]);
    const [error, setError] = useState('');
    const [name, setName] = useState('');
    const [showPopup, setShowPopup] = useState(false);
    const [loading, setLoading] = useState(true);

    const navigate = useNavigate();

    useEffect(() => {
        document.title = 'SmartHome - Building';

        if (!localStorage.getItem('jwtToken')) {
            navigate('/login');
        }

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

            const responseJson = await response.json();

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

    const handleSubmit = async () => {
        if (!name) {
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
                body: JSON.stringify({name}),
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || 'Failed to add room.');
                throw new Error(errorMessage || 'Failed to add room.');
            }

            await fetchRooms();
            setShowPopup(false);
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

    return (
        <div className='home-page'>
            <div className='content-container'>
                <h1>Your rooms</h1>
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
                    setShowPopup(!showPopup);
                    setError('')
                }}>Add room
                </button>

                {showPopup && (
                    <>
                        <div className='popup'>
                            <div className='form-container'>
                                <h1>Add room</h1>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    handleSubmit();
                                }}
                                      noValidate={true}>
                                    <input
                                        type='text'
                                        placeholder='Name'
                                        value={name}
                                        onChange={(e) => setName(e.target.value)}
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
                            setShowPopup(!showPopup);
                            setError('')
                        }}></div>
                    </>
                )}
            </div>
        </div>
    );
};

export default RoomPage;