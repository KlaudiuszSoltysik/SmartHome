﻿export const validatePassword = (password: string) => {
    if (password.length < 8) {
        return 'Password must be at least 8 characters long.';
    }
    if (!/[!@#$%^&*(),.?:{}|<>]/.test(password)) {
        return 'Password must contain at least one special character (!@#$%^&*(),.?:{}|<>).';
    }
    if (!/[A-Z]/.test(password)) {
        return 'Password must contain at least one uppercase letter.';
    }
    if (!/[a-z]/.test(password)) {
        return 'Password must contain at least one lowercase letter.';
    }
    if (!/[0-9]/.test(password)) {
        return 'Password must contain at least one number.';
    }
    return null;
};