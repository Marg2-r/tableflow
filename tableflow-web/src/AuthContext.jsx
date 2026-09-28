import {
  useEffect,
  useState,
} from "react";
import { AuthContext } from "./auth-context";
import { API_URL } from "./config";
import { apiFetch, UNAUTHORIZED_EVENT } from "./api";

async function readError(response) {
  const text = await response.text();

  if (!text) {
    return `Request failed with status ${response.status}`;
  }

  try {
    const data = JSON.parse(text);

    return typeof data === "string"
      ? data
      : data.title || data.message || text;
  } catch {
    return text;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function loadCurrentUser() {
      try {
        const response = await apiFetch(`${API_URL}/auth/me`);

        if (!cancelled && response.ok) {
          setUser(await response.json());
        }
      } catch (error) {
        console.error("Could not restore manager session.", error);
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    loadCurrentUser();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    function handleUnauthorized() {
      setUser(null);
    }

    window.addEventListener(
      UNAUTHORIZED_EVENT,
      handleUnauthorized,
    );

    return () => {
      window.removeEventListener(
        UNAUTHORIZED_EVENT,
        handleUnauthorized,
      );
    };
  }, []);

  async function login(email, password, rememberMe) {
    const response = await apiFetch(`${API_URL}/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        email,
        password,
        rememberMe,
      }),
    });

    if (!response.ok) {
      throw new Error(await readError(response));
    }

    const currentUser = await response.json();

    setUser(currentUser);

    return currentUser;
  }

  async function logout() {
    try {
      await apiFetch(`${API_URL}/auth/logout`, {
        method: "POST",
      });
    } finally {
      setUser(null);
    }
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
