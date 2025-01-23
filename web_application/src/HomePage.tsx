import {useEffect, useState} from 'react';
import {useNavigate} from 'react-router-dom';

interface Building {
    name: string;
    address: string;
    postalCode: string;
    country: string;
}

const HomePage = () => {
    const [buildings, setBuildings] = useState<Building[]>([]);
    const [error, setError] = useState('');
    const [name, setName] = useState('');
    const [address, setAddress] = useState('');
    const [postalCode, setPostalCode] = useState('');
    const [country, setCountry] = useState('');
    const [showPopup, setShowPopup] = useState(false);
    const [loading, setLoading] = useState(true);
    
    const navigate = useNavigate();

    useEffect(() => {
        document.title = 'SmartHome - Home';

        if (!localStorage.getItem('jwtToken')) {
            navigate('/login');
        }

        fetchBuildings();
    }, [navigate]);

    const fetchBuildings = async () => {
        setError('');
        setLoading(true);

        try {
            const response = await fetch('http://localhost:5050/buildings', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || 'Failed to get buildings.');
                throw new Error(errorMessage || 'Failed to get buildings.');
            }

            const buildingsData = await response.json();
            if (buildingsData && buildingsData.length > 0) {
                setBuildings(buildingsData);
            } else {
                setError('No buildings found.');
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
        if (!address) {
            setError('Address is required.');
            return;
        }
        if (!postalCode) {
            setError('Postal code is required.');
            return;
        }
        if (!country) {
            setError('Country is required.');
            return;
        }

        setError('');
        setLoading(true);

        try {
            const response = await fetch('http://localhost:5050/buildings', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
                },
                body: JSON.stringify({name, address, postalCode, country}),
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                setError(errorMessage || 'Failed to add building.');
                throw new Error(errorMessage || 'Failed to add building.');
            }

            await fetchBuildings();
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
                <h1>Home</h1>
                {loading ? (
                    <div className={'loader'}></div>
                ) : (
                    buildings.map((building, index) => (
                        <div className={'row'} key={index}>
                            <p>{building.name}</p>
                            <p>{building.address}</p>
                            <p>{building.postalCode}</p>
                            <p>{building.country}</p>
                        </div>
                    ))
                )}
                <p className={'error-message'} style={{visibility: error ? 'visible' : 'hidden'}}>{error || ''}</p>
                <button className={'primary-button'} onClick={() => {
                    setShowPopup(!showPopup);
                    setError('')
                }}>Add building
                </button>
                <div className='row'>
                    <button className={'secondary-button'} onClick={() => navigate('/account')}>Account</button>
                    <button className={'tertiary-button'} onClick={() => {
                        localStorage.removeItem('jwtToken');
                        navigate('/login')
                    }}>Log out
                    </button>
                </div>
                {showPopup && (
                    <>
                        <div className='popup'>
                            <div className='form-container'>
                                <h1>Add building</h1>
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
                                    <br/>
                                    <input
                                        type='text'
                                        placeholder='Address'
                                        value={address}
                                        onChange={(e) => setAddress(e.target.value)}
                                    />
                                    <br/>
                                    <input
                                        type='text'
                                        placeholder='Postal code'
                                        value={postalCode}
                                        onChange={(e) => setPostalCode(e.target.value)}
                                    />
                                    <br/>
                                    <input
                                        type='text'
                                        placeholder='Country'
                                        value={country}
                                        onChange={(e) => setCountry(e.target.value)}
                                    />
                                    <br/>
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

export default HomePage;