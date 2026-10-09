import { useState } from "react";
import { useAuth } from "../useAuth";

function ManagerLogin({ onCancel }) {
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();

    setIsSubmitting(true);
    setError("");

    try {
      await login(email, password, rememberMe);
    } catch (loginError) {
      setError(loginError.message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="page">
      <section className="hero management-hero">
        <p className="eyebrow">Restaurant management</p>
        <h1>Manager login</h1>

        <p className="hero-description">
          Sign in to manage tables, reservations and settings.
        </p>
      </section>

      <section className="management-card manager-login-card">
        <form
          className="management-form manager-login-form"
          onSubmit={handleSubmit}
        >
          <label>
            <span>Email</span>

            <input
              type="email"
              value={email}
              autoComplete="username"
              autoFocus
              required
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>

          <label>
            <span>Password</span>

            <input
              type="password"
              value={password}
              autoComplete="current-password"
              required
              onChange={(event) =>
                setPassword(event.target.value)
              }
            />
          </label>

          <label className="manager-remember">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(event) =>
                setRememberMe(event.target.checked)
              }
            />

            <span>Remember me</span>
          </label>

          {error && <p className="error-message">{error}</p>}

          <div className="management-actions">
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Signing in..." : "Sign in"}
            </button>

            <button
              type="button"
              className="secondary-button"
              disabled={isSubmitting}
              onClick={onCancel}
            >
              Back to booking
            </button>
          </div>
        </form>
      </section>
    </main>
  );
}

export default ManagerLogin;
