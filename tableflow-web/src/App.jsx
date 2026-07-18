import { useMemo, useState } from "react";
import "./App.css";

const API_URL = "http://localhost:8080";
const OPENING_TIME_MINUTES = 12 * 60;
const CLOSING_TIME_MINUTES = 23 * 60;
const TIME_STEP_MINUTES = 15;

function formatLocalDate(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function formatTime(minutes) {
  const hours = String(Math.floor(minutes / 60)).padStart(2, "0");
  const remainingMinutes = String(minutes % 60).padStart(2, "0");

  return `${hours}:${remainingMinutes}`;
}

function timeToMinutes(time) {
  const [hours, minutes] = time.split(":").map(Number);

  return hours * 60 + minutes;
}

const ALL_TIME_SLOTS = [];

for (
  let minutes = OPENING_TIME_MINUTES;
  minutes < CLOSING_TIME_MINUTES;
  minutes += TIME_STEP_MINUTES
) {
  ALL_TIME_SLOTS.push(formatTime(minutes));
}


function App() {
  const today = formatLocalDate(new Date());

  const [date, setDate] = useState(today);
  const [time, setTime] = useState("");
  const [guests, setGuests] = useState(2);

  const [tables, setTables] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [hasSearched, setHasSearched] = useState(false);

  const availableTimeSlots = useMemo(() => {
  if (date !== today) {
    return ALL_TIME_SLOTS;
  }

  const now = new Date();
  const currentTimeMinutes = now.getHours() * 60 + now.getMinutes();

  return ALL_TIME_SLOTS.filter(
    (slot) => timeToMinutes(slot) > currentTimeMinutes,
  );
}, [date, today]);

  async function handleSearch(event) {
    event.preventDefault();

    setError("");
    setTables([]);
    setHasSearched(false);

    if (!date || !time || guests < 1) {
      setError("Please select a date, time and number of guests.");
      return;
    }

    if (date < today) {
      setError("You cannot select a past date.");
      return;
    }

    if (!time) {
      setError("Please select an available time.");
      return;
    }
    setIsLoading(true);

    try {
      const reservationTime = `${time}:00`;

      const query = new URLSearchParams({
        date,
        time: reservationTime,
        guests: guests.toString(),
      });

      const response = await fetch(
        `${API_URL}/tables/available?${query.toString()}`,
      );

      if (!response.ok) {
        throw new Error(`Request failed with status ${response.status}`);
      }

      const data = await response.json();

      setTables(data);
      setHasSearched(true);
    } catch (requestError) {
      console.error(requestError);
      setError(
        "Could not load available tables. Make sure the backend is running.",
      );
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="page">
      <section className="hero">
        <p className="eyebrow">Restaurant reservation platform</p>
        <h1>Book your table</h1>
        <p className="hero-description">
          Choose a date, time and party size to see available tables.
        </p>
      </section>

      <section className="search-card">
        <form className="search-form" onSubmit={handleSearch}>
          <label>
            Date
            <input
              type="date"
              min={today}
              value={date}
              onChange={(event) => {
                setDate(event.target.value);
                setTime("");
              }}
              required
            />
          </label>

          <select
            value={time}
            onChange={(event) => setTime(event.target.value)}
            disabled={availableTimeSlots.length === 0}
            required
          >
            <option value="">
              {availableTimeSlots.length === 0
                ? "No more times today"
                : "Select time"}
            </option>

            {availableTimeSlots.map((slot) => (
              <option key={slot} value={slot}>
                {slot}
              </option>
            ))}
          </select>

          <label>
            Guests
            <input
              type="number"
              min="1"
              max="20"
              value={guests}
              onChange={(event) => setGuests(Number(event.target.value))}
            />
          </label>

          <button type="submit" disabled={isLoading}>
            {isLoading ? "Searching..." : "Find available tables"}
          </button>
        </form>

        {error && <p className="error-message">{error}</p>}
      </section>

      {hasSearched && (
        <section className="results">
          <div className="results-header">
            <div>
              <p className="eyebrow">Availability</p>
              <h2>Available tables</h2>
            </div>

            <span className="result-count">
              {tables.length} {tables.length === 1 ? "table" : "tables"}
            </span>
          </div>

          {tables.length === 0 ? (
            <div className="empty-state">
              No tables are available for the selected time.
            </div>
          ) : (
            <div className="restaurant-map">
              {tables.map((table) => (
                <button
                  className="table-item"
                  key={table.id}
                  style={{
                    left: `${table.xPosition}px`,
                    top: `${table.yPosition}px`,
                  }}
                  type="button"
                >
                  <strong>{table.name}</strong>
                  <span>{table.capacity} guests</span>
                  <small>{table.zone}</small>
                </button>
              ))}
            </div>
          )}
        </section>
      )}
    </main>
  );
}

export default App;