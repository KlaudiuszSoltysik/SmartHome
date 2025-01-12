import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";

const AccountPage = () => {
    const navigate = useNavigate();
    const [userInfo, setUserInfo] = useState({name: '', email: ''});
    const [loading, setLoading] = useState(true);
    const [showPopup, setShowPopup] = useState(false);
    const [name, setName] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async () => {
        if (!name) {
            setError('Name is required.');
            return;
        }

        setError('');
        setLoading(true);

        // try {
        //     const response = await fetch('http://localhost:5050/users/register', {
        //         method: 'POST',
        //         headers: {
        //             'Content-Type': 'application/json',
        //         },
        //         body: JSON.stringify(name),
        //     });
        //
        //     if (!response.ok) {
        //         return response.text().then(errorMessage => {
        //             setError(errorMessage || 'Failed to register.');
        //             throw new Error(errorMessage || 'Failed to register.');
        //         });
        //     }
        // } catch (error:unknown) {
        //     if (error instanceof Error) {
        //         setError(error.message);
        //     } else {
        //         setError('An unknown error occurred');
        //     }
        // } finally {
        //     setLoading(false);
        // }
    };

    useEffect(() => {
        document.title = "SmartHome - Account";

        if (!localStorage.getItem("jwtToken")) {
            navigate('/login');
            return;
        }

        const fetchUserData = async () => {
            try {
                const token = localStorage.getItem("jwtToken");
                const response = await fetch('http://localhost:5050/users', {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                });

                if (!response.ok) {
                    throw new Error('Failed to fetch user data');
                }

                const data = await response.json();
                setUserInfo(data);
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

        fetchUserData();
    }, [navigate]);

    if (loading) {
        return (
            <div className='account-page'>
                <div className='content-container'>
                    <h1>Account</h1>
                    <div className="shimmer-wrapper">
                        <div className="shimmer-block"></div>
                        <div className="shimmer-block"></div>
                    </div>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className='account-page'>
                <div className='content-container'>
                    <p className={'error-message'}>{error}</p>
                </div>
            </div>
        );
    }

    return (
        <div className='account-page'>
            <div className='content-container'>
                <h1>Account</h1>
                <p><strong>Name:</strong> {userInfo.name}</p>
                <p><strong>Email:</strong> {userInfo.email}</p>
                <div className={'row'}>
                    <button className={'secondary-button'} onClick={() => setShowPopup(!showPopup)}>Edit name</button>
                    <button className={'tertiary-button'}>Delete account</button>
                </div>
                {showPopup && (
                    <>
                        <div className="popup">
                            <div className='form-container'>
                                <h1>Edit name</h1>
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
                                    <button className={'primary-button form-button'} type='submit' disabled={loading}>
                                        {loading ? 'Logging in...' : 'Login'}
                                    </button>
                                </form>
                                <p className={'error-message'}
                                   style={{visibility: error ? 'visible' : 'hidden'}}>{error ? error : 'error'}</p>
                            </div>
                        </div>
                        <div className="backdrop" onClick={() => setShowPopup(!showPopup)}></div>
                    </>
                )}
            </div>
        </div>
    );
};

export default AccountPage;