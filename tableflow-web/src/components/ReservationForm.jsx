import { useState } from "react";
import { API_URL, RESTAURANT_ID } from "../config";

async function readError(response) {
  const text = await response.text();

  if (!text) {
    return `Request failed with status ${response.status}`;
  }

  try {
    const parsed = JSON.parse(text);

    return typeof parsed === "string"
      ? parsed
      : parsed.title || text;
  } catch {
    return text;
  }
}

function ReservationForm({
  table,
  date,
  time,
  guests,
  onClose,
  onReserved,
}) {
  const [form, setForm] = useState({
    customerName: "",
    customerEmail: "",
    customerPhone: "",
    notes: "",
  });

  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");

  function handleChange(event) {
    const { name, value } = event.target;

    setForm((currentForm) => ({
      ...currentForm,
      [name]: value,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setIsSaving(true);
    setError("");

    try {
      const response = await fetch(
        `${API_URL}/restaurants/${RESTAURANT_ID}/reservations`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            tableId: table.id,
            customerName: form.customerName,
            customerEmail: form.customerEmail,
            customerPhone: form.customerPhone,
            reservationDate: date,
            reservationTime: `${time}:00`,
            guestCount: guests,
            notes: form.notes || null,
          }),
        },
      );

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      const reservation = await response.json();

      await onReserved(reservation);
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="modal-backdrop">
      <section
        className="reservation-modal"
        role="dialog"
        aria-modal="true"
      >
        <div className="modal-header">
          <div>
            <p className="eyebrow">Complete reservation</p>
            <h2>{table.name}</h2>

            <p>
              {date} · {time} · {guests}{" "}
              {guests === 1 ? "guest" : "guests"}
            </p>
          </div>

          <button
            type="button"
            className="close-button"
            onClick={onClose}
          >
            ×
          </button>
        </div>

        <form
          className="reservation-form"
          onSubmit={handleSubmit}
        >
          <label>
            Name
            <input
              type="text"
              name="customerName"
              maxLength="100"
              value={form.customerName}
              onChange={handleChange}
              required
            />
          </label>

          <label>
            Email
            <input
              type="email"
              name="customerEmail"
              maxLength="200"
              value={form.customerEmail}
              onChange={handleChange}
              required
            />
          </label>

          <label>
            Phone
            <input
              type="tel"
              name="customerPhone"
              maxLength="30"
              value={form.customerPhone}
              onChange={handleChange}
              required
            />
          </label>

          <label className="full-width">
            Notes
            <textarea
              name="notes"
              maxLength="500"
              rows="4"
              value={form.notes}
              onChange={handleChange}
            />
          </label>

          {error && (
            <p className="error-message full-width">
              {error}
            </p>
          )}

          <div className="modal-actions">
            <button
              type="button"
              className="secondary-button"
              onClick={onClose}
              disabled={isSaving}
            >
              Cancel
            </button>

            <button type="submit" disabled={isSaving}>
              {isSaving
                ? "Creating reservation..."
                : "Confirm reservation"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}

export default ReservationForm;