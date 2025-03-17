import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import {validateEmail} from "./utilities/validateEmail.ts";

const ForgotPasswordPage = () => {
    const navigate = useNavigate();
    const [email, setEmail] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        document.title = "SmartHome - Forgot password";

        if (localStorage.getItem("jwtToken")) {
            navigate("/");
        }
    }, [navigate]);

    const handleSubmit = async () => {
        const emailError = validateEmail(email);
        if (emailError) {
            setError(emailError);
            return;
        }

        setError("");
        setLoading(true);

        try {
            const response = await fetch("http://localhost:5050/users/forgot-password", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(email),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to reset password.");
                    throw new Error(errorMessage || "Failed to reset password.");
                });
            }

            navigate("/login", {state: {message: "Check your email inbox."}});
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
                        type="email"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                    <br/>
                    <button className={"primary-button form-button"} type="submit" disabled={loading}>
                        {loading ? "Sending email..." : "Send email"}
                    </button>
                </form>
                <p className={"error-message"} style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
            </div>
        </div>
    );
};

export default ForgotPasswordPage;