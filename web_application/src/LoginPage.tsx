import {useEffect, useState} from 'react';
import {useNavigate} from "react-router-dom";
import {validateEmail} from './utilities/validateEmail.ts';
import {validatePassword} from './utilities/validatePassword.ts';

const LoginPage = () => {
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "SmartHome - Login";

        if (localStorage.getItem("jwtToken")) {
            navigate('/');
        }
    }, [navigate]);

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async () => {
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
            const credentials = {email, password};

            const response = await fetch('http://localhost:5050/users/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(credentials),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || 'Failed to log in.');
                    throw new Error(errorMessage || 'Failed to log in.');
                });
            }

            const data = await response.json();
            const jwtToken = data.token;

            if (jwtToken) {
                localStorage.setItem('jwtToken', jwtToken);
            } else {
                setError('Token not found in the response');
            }
            console.log(jwtToken);
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

    return (
        <div className='login-page'>
            <div className='form-container'>
                <h1>Login</h1>
                <form onSubmit={(e) => {
                    e.preventDefault();
                    handleSubmit();
                }}
                      noValidate={true}>
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
                        {loading ? 'Logging in...' : 'Login'}
                    </button>
                </form>
                <p className={'error-message'}
                   style={{visibility: error ? 'visible' : 'hidden'}}>{error ? error : 'error'}</p>
            </div>
        </div>
    );
};

export default LoginPage;