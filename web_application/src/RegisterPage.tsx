import {useEffect, useState} from 'react';
import {useNavigate} from "react-router-dom";
import {validateEmail} from './utilities/validateEmail.ts';
import {validatePassword} from './utilities/validatePassword.ts';

const LoginPage = () => {
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "SmartHome - Register";

        if (localStorage.getItem("jwtToken")) {
            navigate('/');
        }
    }, [navigate]);

    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async () => {
        if (!name) {
            setError('Name is required.');
            return;
        }

        const emailError = validateEmail(email);
        if (emailError) {
            setError(emailError);
            return;
        }

        const passwordError = validatePassword(password);
        if (passwordError) {
            setError(passwordError);
            return;
        }

        setError('');
        setLoading(true);

        try {
            const credentials = {name, email, password};

            const response = await fetch('http://localhost:5050/users/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(credentials),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || 'Failed to register.');
                    throw new Error(errorMessage || 'Failed to register.');
                });
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
    };

    return (
        <div className='register-page'>
            <div className='form-container'>
                <h1>Register</h1>
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
                        type='email'
                        placeholder='Email'
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                    <br/>
                    <input
                        type='password'
                        placeholder='Password'
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                    <br/>
                    <button className={'primary-button form-button'} type='submit' disabled={loading}>
                        {loading ? 'Registering...' : 'Register'}
                    </button>
                </form>
                <p className={'error-message'}
                   style={{visibility: error ? 'visible' : 'hidden'}}>{error ? error : 'error'}</p>
            </div>
        </div>
    );
};

export default LoginPage;