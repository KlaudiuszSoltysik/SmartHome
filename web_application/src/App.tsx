import React from 'react';
import {BrowserRouter as Router, Navigate, Route, Routes} from 'react-router-dom';

import HomePage from './HomePage';
import LoginPage from './LoginPage';
import RegisterPage from './RegisterPage';
import AccountPage from './AccountPage';
import ForgotPasswordPage from './ForgotPasswordPage.tsx';
import ResetPasswordPage from './ResetPasswordPage';
import Stream from "./Stream.tsx";

const App: React.FC = () => {
    return (
        <Router>
            <Routes>
                <Route path='/' element=<HomePage/>/>
                <Route path='/login' element=<LoginPage/>/>
                <Route path='/register' element=<RegisterPage/>/>
                <Route path='/forgot-password' element=<ForgotPasswordPage/>/>
                <Route path='/reset-password' element=<ResetPasswordPage/>/>
                <Route path='/account' element=<AccountPage/>/>
                <Route path='/stream' element=<Stream/>/>
                <Route path='*' element={<Navigate to='/'/>}/>
            </Routes>
        </Router>
    );
};

export default App;
