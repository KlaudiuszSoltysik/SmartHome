import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";

const AccountPage = () => {
    const navigate = useNavigate();
    const [userInfo, setUserInfo] = useState({name: "", email: ""});
    const [loading, setLoading] = useState(true);
    const [showPopup, setShowPopup] = useState(false);
    const [name, setName] = useState("");
    const [error, setError] = useState("");
    const [timer, setTimer] = useState<number | null>(null);

    useEffect(() => {
        document.title = "SmartHome - Account";

        if (!localStorage.getItem("jwtToken")) {
            navigate("/login");
            return;
        }

        fetchUserData();
    }, [navigate]);

    const handleMouseDown = () => {
        setError("Hold the button for 3 seconds to delete.");
        const newTimer = setTimeout(() => {
            deleteUser();
        }, 3000);
        setTimer(newTimer);
    };

    const handleMouseUpOrLeave = () => {
        setError("");
        if (timer) {
            clearTimeout(timer);
            setTimer(null);
        }
    };

    const editUser = async () => {
        if (!name) {
            setError("Name is required.");
            return;
        }

        setError("");
        setLoading(true);

        try {
            const response = await fetch("http://localhost:5050/users", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
                body: JSON.stringify({name}),
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to change name.");
                    throw new Error(errorMessage || "Failed to change name.");
                });
            }

            setShowPopup(false);
            await fetchUserData();
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

    const deleteUser = async () => {
        setLoading(true);

        try {
            const response = await fetch("http://localhost:5050/users", {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                },
            });

            if (!response.ok) {
                return response.text().then(errorMessage => {
                    setError(errorMessage || "Failed to delete user.");
                    throw new Error(errorMessage || "Failed to delete user.");
                });
            }

            localStorage.removeItem("jwtToken");
            navigate("/login");
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

    const fetchUserData = async () => {
        try {
            const response = await fetch("http://localhost:5050/users", {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("jwtToken")}`,
                },
            });

            if (!response.ok) {
                throw new Error("Failed to fetch user data");
            }

            const data = await response.json();
            setUserInfo(data);
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
        <div className="account-page">
            <div className="content-container">
                <h1>Account</h1>
                {loading && (
                    <div className="shimmer-wrapper">
                        <div className="shimmer-block"></div>
                        <div className="shimmer-block"></div>
                    </div>
                )}
                {!loading && (
                    <>
                        <p><strong>Name:</strong> {userInfo.name}</p>
                        <p><strong>Email:</strong> {userInfo.email}</p>
                    </>
                )}
                <div className={"row"}>
                    <button className={"secondary-button"} onClick={() => setShowPopup(!showPopup)}>Edit name</button>
                    <button className={"tertiary-button"}
                            onMouseDown={handleMouseDown}
                            onMouseUp={handleMouseUpOrLeave}
                            onMouseLeave={handleMouseUpOrLeave}>
                        Delete account
                    </button>
                </div>
                <p className={"error-message"}
                   style={{visibility: error && !showPopup ? "visible" : "hidden"}}>{error || "error"}</p>
                {showPopup && (
                    <>
                        <div className="popup">
                            <div className="form-container">
                                <h1>Edit name</h1>
                                <form onSubmit={(e) => {
                                    e.preventDefault();
                                    editUser();
                                }}
                                      noValidate={true}>
                                    <input
                                        type="text"
                                        placeholder="Name"
                                        value={name}
                                        onChange={(e) => setName(e.target.value)}
                                    />
                                    <br/>
                                    <button className={"primary-button form-button"} type="submit" disabled={loading}>
                                        {loading ? "In progress..." : "Change"}
                                    </button>
                                </form>
                                <p className={"error-message"}
                                   style={{visibility: error ? "visible" : "hidden"}}>{error || ""}</p>
                            </div>
                        </div>
                        <div className="backdrop" onClick={() => {
                            setShowPopup(!showPopup);
                            setError("")
                        }}></div>
                    </>
                )}
            </div>
        </div>
    );
};

export default AccountPage;