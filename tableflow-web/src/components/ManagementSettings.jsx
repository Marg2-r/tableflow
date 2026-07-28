import { useEffect, useState } from "react";
import { API_URL, RESTAURANT_ID } from "../config";

function ManagementSettings() {
  const [settings, setSettings] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  useEffect(() => {
    loadSettings();
  }, []);

  async function loadSettings() {
    setIsLoading(true);
    setError("");

    try {
      const response = await fetch(
        `${API_URL}/restaurants/${RESTAURANT_ID}/management/settings`,
      );

      if (!response.ok) {
        const message = await response.text();

        throw new Error(
          message || `Request failed with status ${response.status}`,
        );
      }

      const data = await response.json();

      setSettings(data);
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }

  function handleChange(event) {
    const { name, value } = event.target;

    setSettings((currentSettings) => ({
      ...currentSettings,
      [name]: Number(value),
    }));

    setSuccessMessage("");
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setIsSaving(true);
    setError("");
    setSuccessMessage("");

    try {
      const response = await fetch(
        `${API_URL}/restaurants/${RESTAURANT_ID}/management/settings`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            defaultReservationDurationMinutes:
              settings.defaultReservationDurationMinutes,
            slotIntervalMinutes:
              settings.slotIntervalMinutes,
            turnoverBufferMinutes:
              settings.turnoverBufferMinutes,
            minimumAdvanceMinutes:
              settings.minimumAdvanceMinutes,
            maximumAdvanceDays:
              settings.maximumAdvanceDays,
          }),
        },
      );

      if (!response.ok) {
        const message = await response.text();

        throw new Error(
          message || `Request failed with status ${response.status}`,
        );
      }

      const updatedSettings = await response.json();

      setSettings(updatedSettings);
      setSuccessMessage("Settings saved successfully.");
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setIsSaving(false);
    }
  }

  if (isLoading) {
    return (
      <main className="page">
        <div className="management-card">
          Loading restaurant settings...
        </div>
      </main>
    );
  }

  if (!settings) {
    return (
      <main className="page">
        <div className="management-card">
          <p className="error-message">
            {error || "Settings could not be loaded."}
          </p>

          <button type="button" onClick={loadSettings}>
            Try again
          </button>
        </div>
      </main>
    );
  }

  return (
    <main className="page">
      <section className="hero management-hero">
        <p className="eyebrow">Restaurant management</p>
        <h1>Reservation settings</h1>

        <p className="hero-description">
          Configure reservation duration, available time slots and booking
          limits.
        </p>
      </section>

      <section className="management-card">
        <form className="management-form" onSubmit={handleSubmit}>
          <label>
            Reservation duration
            <span>How long guests may occupy a table.</span>

            <div className="input-with-unit">
              <input
                type="number"
                name="defaultReservationDurationMinutes"
                min="15"
                max="720"
                step="15"
                value={settings.defaultReservationDurationMinutes}
                onChange={handleChange}
                required
              />

              <strong>minutes</strong>
            </div>
          </label>

          <label>
            Time slot interval
            <span>
              How often a new reservation time appears.
            </span>

            <div className="input-with-unit">
              <input
                type="number"
                name="slotIntervalMinutes"
                min="5"
                max="60"
                step="5"
                value={settings.slotIntervalMinutes}
                onChange={handleChange}
                required
              />

              <strong>minutes</strong>
            </div>
          </label>

          <label>
            Turnover buffer
            <span>
              Extra time for cleaning and preparing the table.
            </span>

            <div className="input-with-unit">
              <input
                type="number"
                name="turnoverBufferMinutes"
                min="0"
                max="180"
                step="5"
                value={settings.turnoverBufferMinutes}
                onChange={handleChange}
                required
              />

              <strong>minutes</strong>
            </div>
          </label>

          <label>
            Minimum advance time
            <span>
              How early a guest must make a reservation.
            </span>

            <div className="input-with-unit">
              <input
                type="number"
                name="minimumAdvanceMinutes"
                min="0"
                max="10080"
                step="5"
                value={settings.minimumAdvanceMinutes}
                onChange={handleChange}
                required
              />

              <strong>minutes</strong>
            </div>
          </label>

          <label>
            Maximum advance period
            <span>
              How far into the future guests may book.
            </span>

            <div className="input-with-unit">
              <input
                type="number"
                name="maximumAdvanceDays"
                min="1"
                max="365"
                value={settings.maximumAdvanceDays}
                onChange={handleChange}
                required
              />

              <strong>days</strong>
            </div>
          </label>

          <div className="management-actions">
            <button
              className="secondary-button"
              type="button"
              onClick={loadSettings}
              disabled={isSaving}
            >
              Reset changes
            </button>

            <button type="submit" disabled={isSaving}>
              {isSaving ? "Saving..." : "Save settings"}
            </button>
          </div>
        </form>

        {error && <p className="error-message">{error}</p>}

        {successMessage && (
          <p className="success-message">
            {successMessage}
          </p>
        )}
      </section>
    </main>
  );
}

export default ManagementSettings;