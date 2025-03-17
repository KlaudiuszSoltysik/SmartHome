import {useEffect, useState} from "react";
import {useNavigate, useSearchParams} from "react-router-dom";
import {validatePassword} from "./utilities/validatePassword.ts";

const ResetPasswordPage = () => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const token = searchParams.get("token");
    const email = searchParams.get("email");

    useEffect(() => {
        document.title = "SmartHome - Reset password";

        if (localStorage.getItem("jwtToken")) {
            navigate("/");
        }
    }, [navigate]);

    const handleSubmit = async () => {
        const passwordError = validatePassword(password);
        if (passwordError) {
            setError(passwordError);
            return;
        }

        setError("");
        setLoading(true);

        try {
            const response = await fetch("http://localhost:5050/users/reset-password", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({email, token, password}),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to reset password.");
                    throw new Error(errorMessage || "Failed to reset password.");
                });
            }

            navigate("/login", {state: {message: "Password had been set."}});
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="forgot-password-page">
            <div className="form-container">
                <h1>Reset password</h1>
                <form onSubmit={(e) => {
                    e.preventDefault();
                    handleSubmit();
                }}
                      noValidate={true}>
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                    <br/>
                    <button className={"primary-button form-button"} type="submit" disabled={loading}>
                        {loading ? "Setting password..." : "Set password"}
                    </button>
                </form>
                <p className={"error-message"} style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
            </div>
        </div>
    );
};

export default ResetPasswordPage;