import {useEffect, useState} from "react";
import {useLocation, useNavigate} from "react-router-dom";
import {validateEmail} from "./utilities/validateEmail.ts";
import {validatePassword} from "./utilities/validatePassword.ts";

const LoginPage = () => {
    const navigate = useNavigate();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const location = useLocation();
    const message = location.state?.message;

    useEffect(() => {
        document.title = "SmartHome - Login";

        if (localStorage.getItem("jwtToken")) {
            navigate("/");
        }
    }, [navigate]);

    const handleSubmit = async () => {
        if (location.state?.message) {
            location.state.message = null;
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

        setError("");
        setLoading(true);

        try {
            const response = await fetch("http://localhost:5050/users/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({email, password}),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to log in.");
                    throw new Error(errorMessage || "Failed to log in.");
                });
            }

            const data = await response.json();
            const jwtToken = data.token;

            if (jwtToken) {
                localStorage.setItem("jwtToken", jwtToken);
            } else {
                setError("Token not found in the response.");
            }

            navigate("/");
        } catch (error: unknown) {
            if (error instanceof Error) {
                setError(error.message);
            } else {
                setError("An unknown error occurred.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-page">
            <div className="form-container">
                <h1>Login</h1>
                <form onSubmit={(e) => {
                    e.preventDefault();
                    handleSubmit();
                }}
                      noValidate={true}>
                    <input
                        type="email"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                    <br/>
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                    <br/>
                    <button className={"primary-button form-button"} type="submit" disabled={loading}>
                        {loading ? "Logging in..." : "Login"}
                    </button>
                </form>
                <div style={{display: "flex"}}>
                    <p className={"error-message"}
                       style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
                    <p className={"error-message"}
                       style={{color: "#3B82F6", visibility: message ? "visible" : "hidden"}}>{message || ""}</p>
                </div>
                <button className={"secondary-button form-button"} onClick={() => navigate("/register")}>Register new
                    account
                </button>
                <button className={"tertiary-button form-button"} onClick={() => navigate("/forgot-password")}>Reset
                    password
                </button>
            </div>
        </div>
    );
};

export default LoginPage;