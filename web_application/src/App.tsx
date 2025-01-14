import React from 'react';
import {BrowserRouter as Router, Navigate, Route, Routes} from 'react-router-dom';

import HomePage from './HomePage';
import LoginPage from './LoginPage';
import RegisterPage from './RegisterPage';
import AccountPage from './AccountPage';
import ResetPasswordPage from './ResetPasswordPage';

const App: React.FC = () => {
    return (
        <Router>
            <Routes>
                <Route path='/' element=<HomePage/>/>
                <Route path='/login' element=<LoginPage/>/>
                <Route path='/register' element=<RegisterPage/>/>
                <Route path='/reset-password' element=<ResetPasswordPage/>/>
                <Route path='/account' element=<AccountPage/>/>
                <Route path='*' element={<Navigate to='/'/>}/>
            </Routes>
        </Router>
    );
};

export default App;
