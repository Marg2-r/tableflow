import { useCallback, useEffect, useState } from "react";
import { apiFetch } from "../api";
import { API_URL } from "../config";

const dateFormatter = new Intl.DateTimeFormat(undefined, {
  year: "numeric",
  month: "short",
  day: "numeric",
});

const timeFormatter = new Intl.DateTimeFormat(undefined, {
  hour: "2-digit",
  minute: "2-digit",
});

const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: "medium",
  timeStyle: "short",
});

async function readError(response) {
  const text = await response.text();

  if (!text) {
    return `Request failed with status ${response.status}`;
  }

  try {
    const parsed = JSON.parse(text);

    if (typeof parsed === "string") {
      return parsed;
    }

    if (parsed.errors) {
      return Object.values(parsed.errors).flat().join(" ");
    }

    return parsed.title || parsed.message || text;
  } catch {
    return text;
  }
}

function normalizeStatus(status) {
  const value = String(status).toLowerCase();

  const statuses = {
    "1": "confirmed",
    "2": "cancelled",
    "3": "seated",
    "4": "no-show",
    "5": "completed",
    confirmed: "confirmed",
    cancelled: "cancelled",
    seated: "seated",
    noshow: "no-show",
    "no-show": "no-show",
    completed: "completed",
  };

  return statuses[value] ?? "unknown";
}

function getStatusLabel(status) {
  const normalizedStatus = normalizeStatus(status);

  const labels = {
    confirmed: "Confirmed",
    cancelled: "Cancelled",
    seated: "Seated",
    "no-show": "No show",
    completed: "Completed",
    unknown: "Unknown",
  };

  return labels[normalizedStatus];
}

function getLocalDateKey(value) {
  const date = new Date(value);

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function ReservationStatus({ status }) {
  const normalizedStatus = normalizeStatus(status);

  return (
    <span className={`reservation-status ${normalizedStatus}`}>
      {getStatusLabel(status)}
    </span>
  );
}

function ReservationManagement({ restaurantId }) {
  const [reservations, setReservations] = useState([]);
  const [selectedReservation, setSelectedReservation] =
    useState(null);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [dateFilter, setDateFilter] = useState("");

  const [isLoading, setIsLoading] = useState(true);
  const [cancellingId, setCancellingId] = useState(null);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const loadReservations = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const response = await apiFetch(
        `${API_URL}/restaurants/${restaurantId}/reservations`,
      );

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      const data = await response.json();

      setReservations(data);
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }, [restaurantId]);

  useEffect(() => {
    loadReservations();
  }, [loadReservations]);

  async function handleCancel(reservation) {
    const confirmed = window.confirm(
      `Cancel reservation #${reservation.id} for ${reservation.customerName}?`,
    );

    if (!confirmed) {
      return;
    }

    setCancellingId(reservation.id);
    setError("");
    setSuccessMessage("");

    try {
      const response = await apiFetch(
        `${API_URL}/restaurants/${restaurantId}/reservations/${reservation.id}/cancel`,
        {
          method: "PATCH",
        },
      );

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      const updatedReservation = await response.json();

      setReservations((currentReservations) =>
        currentReservations.map((currentReservation) =>
          currentReservation.id === updatedReservation.id
            ? updatedReservation
            : currentReservation,
        ),
      );

      setSelectedReservation((currentReservation) =>
        currentReservation?.id === updatedReservation.id
          ? updatedReservation
          : currentReservation,
      );

      setSuccessMessage(
        `Reservation #${updatedReservation.id} was cancelled.`,
      );
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setCancellingId(null);
    }
  }

  const normalizedSearch = search.trim().toLowerCase();

  const filteredReservations = reservations.filter(
    (reservation) => {
      const reservationStatus = normalizeStatus(
        reservation.status,
      );

      const matchesStatus =
        statusFilter === "all" ||
        reservationStatus === statusFilter;

      const matchesDate =
        !dateFilter ||
        getLocalDateKey(reservation.startsAtUtc) === dateFilter;

      const searchableText = [
        reservation.id,
        reservation.customerName,
        reservation.customerEmail,
        reservation.customerPhone,
        reservation.tableName,
      ]
        .join(" ")
        .toLowerCase();

      const matchesSearch =
        !normalizedSearch ||
        searchableText.includes(normalizedSearch);

      return matchesStatus && matchesDate && matchesSearch;
    },
  );

  return (
    <main className="page">
      <section className="reservation-management-header">
        <div className="hero management-hero">
          <p className="eyebrow">Reservation management</p>
          <h1>Reservations</h1>

          <p className="hero-description">
            View guest details, reservation times and booking
            statuses.
          </p>
        </div>

        <button
          className="secondary-management-button"
          type="button"
          disabled={isLoading}
          onClick={loadReservations}
        >
          {isLoading ? "Refreshing..." : "Refresh"}
        </button>
      </section>

      {error && <p className="error-message">{error}</p>}

      {successMessage && (
        <p className="success-message">{successMessage}</p>
      )}

      <section className="reservation-filters">
        <label>
          Search
          <input
            type="search"
            placeholder="Guest, email, phone or table"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </label>

        <label>
          Date
          <input
            type="date"
            value={dateFilter}
            onChange={(event) =>
              setDateFilter(event.target.value)
            }
          />
        </label>

        <label>
          Status
          <select
            value={statusFilter}
            onChange={(event) =>
              setStatusFilter(event.target.value)
            }
          >
            <option value="all">All statuses</option>
            <option value="confirmed">Confirmed</option>
            <option value="cancelled">Cancelled</option>
            <option value="seated">Seated</option>
            <option value="no-show">No show</option>
            <option value="completed">Completed</option>
          </select>
        </label>

        <button
          className="clear-filter-button"
          type="button"
          onClick={() => {
            setSearch("");
            setDateFilter("");
            setStatusFilter("all");
          }}
        >
          Clear filters
        </button>
      </section>

      <div className="reservation-results-summary">
        <strong>
          {filteredReservations.length}{" "}
          {filteredReservations.length === 1
            ? "reservation"
            : "reservations"}
        </strong>

        <span>{reservations.length} total</span>
      </div>

      <section className="reservation-list-card">
        {isLoading ? (
          <div className="reservation-list-message">
            Loading reservations...
          </div>
        ) : filteredReservations.length === 0 ? (
          <div className="reservation-list-message">
            <strong>No reservations found</strong>
            <p>Try changing or clearing the filters.</p>
          </div>
        ) : (
          <table className="reservation-management-table">
            <thead>
              <tr>
                <th>Date and time</th>
                <th>Guest</th>
                <th>Table</th>
                <th>Party</th>
                <th>Status</th>
                <th aria-label="Actions" />
              </tr>
            </thead>

            <tbody>
              {filteredReservations.map((reservation) => {
                const startsAt = new Date(
                  reservation.startsAtUtc,
                );

                const canCancel =
                  normalizeStatus(reservation.status) ===
                  "confirmed";

                return (
                  <tr key={reservation.id}>
                    <td>
                      <strong>
                        {dateFormatter.format(startsAt)}
                      </strong>

                      <small>
                        {timeFormatter.format(startsAt)} · #
                        {reservation.id}
                      </small>
                    </td>

                    <td>
                      <strong>{reservation.customerName}</strong>
                      <small>{reservation.customerEmail}</small>
                      <small>{reservation.customerPhone}</small>
                    </td>

                    <td>{reservation.tableName}</td>
                    <td>{reservation.guestCount} guests</td>

                    <td>
                      <ReservationStatus
                        status={reservation.status}
                      />
                    </td>

                    <td>
                      <div className="reservation-row-actions">
                        <button
                          type="button"
                          onClick={() =>
                            setSelectedReservation(reservation)
                          }
                        >
                          Details
                        </button>

                        {canCancel && (
                          <button
                            className="danger-text-button"
                            type="button"
                            disabled={
                              cancellingId === reservation.id
                            }
                            onClick={() =>
                              handleCancel(reservation)
                            }
                          >
                            {cancellingId === reservation.id
                              ? "Cancelling..."
                              : "Cancel"}
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </section>

      {selectedReservation && (
        <div
          className="modal-backdrop"
          onMouseDown={() => setSelectedReservation(null)}
        >
          <section
            className="reservation-modal reservation-details-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="reservation-details-title"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <div className="modal-header">
              <div>
                <p className="eyebrow">
                  Reservation #{selectedReservation.id}
                </p>

                <h2 id="reservation-details-title">
                  {selectedReservation.customerName}
                </h2>
              </div>

              <button
                className="close-button"
                type="button"
                aria-label="Close"
                onClick={() => setSelectedReservation(null)}
              >
                ×
              </button>
            </div>

            <div className="reservation-details-grid">
              <div>
                <span>Status</span>
                <ReservationStatus
                  status={selectedReservation.status}
                />
              </div>

              <div>
                <span>Date and time</span>
                <strong>
                  {dateTimeFormatter.format(
                    new Date(selectedReservation.startsAtUtc),
                  )}
                </strong>
              </div>

              <div>
                <span>Table</span>
                <strong>{selectedReservation.tableName}</strong>
              </div>

              <div>
                <span>Party size</span>
                <strong>
                  {selectedReservation.guestCount} guests
                </strong>
              </div>

              <div>
                <span>Duration</span>
                <strong>
                  {selectedReservation.durationMinutes} minutes
                </strong>
              </div>

              <div>
                <span>Available again</span>
                <strong>
                  {timeFormatter.format(
                    new Date(
                      selectedReservation.tableAvailableAtUtc,
                    ),
                  )}
                </strong>
              </div>

              <div>
                <span>Email</span>
                <a
                  href={`mailto:${selectedReservation.customerEmail}`}
                >
                  {selectedReservation.customerEmail}
                </a>
              </div>

              <div>
                <span>Phone</span>
                <a
                  href={`tel:${selectedReservation.customerPhone}`}
                >
                  {selectedReservation.customerPhone}
                </a>
              </div>

              <div className="full-width">
                <span>Notes</span>
                <strong>
                  {selectedReservation.notes || "No notes"}
                </strong>
              </div>
            </div>

            <div className="reservation-details-actions">
              <button
                className="secondary-button"
                type="button"
                onClick={() => setSelectedReservation(null)}
              >
                Close
              </button>

              {normalizeStatus(selectedReservation.status) ===
                "confirmed" && (
                <button
                  className="danger-button"
                  type="button"
                  disabled={
                    cancellingId === selectedReservation.id
                  }
                  onClick={() =>
                    handleCancel(selectedReservation)
                  }
                >
                  {cancellingId === selectedReservation.id
                    ? "Cancelling..."
                    : "Cancel reservation"}
                </button>
              )}
            </div>
          </section>
        </div>
      )}
    </main>
  );
}

export default ReservationManagement;
