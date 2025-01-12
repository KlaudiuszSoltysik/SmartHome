import {useEffect} from "react";
import {useNavigate} from "react-router-dom";

const HomePage = () => {
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "SmartHome - Home";

        if (!localStorage.getItem("jwtToken")) {
            navigate('/login');
        }
    }, [navigate]);

    return (
        <div className='home-page'>
            <div className='content-container'>
                <h1>Home</h1>
                <button className={'primary-button'} onClick={() => navigate('/account')}>Account</button>
            </div>
        </div>
    );
};

export default HomePage;