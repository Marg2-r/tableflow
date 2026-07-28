import { useEffect, useMemo, useState } from "react";
import { API_URL, RESTAURANT_ID } from "../config";

function normalizeTime(time) {
  return time.slice(0, 5);
}

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

function TableTimePicker({
  table,
  date,
  guests,
  selectedTime,
  onSelectTime,
  onBack,
  onReserve,
}) {
  const [availableTimes, setAvailableTimes] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadAvailableTimes() {
      setIsLoading(true);
      setError("");
      setAvailableTimes([]);

      try {
        const query = new URLSearchParams({
          date,
          guests: guests.toString(),
        });

        const response = await fetch(
          `${API_URL}/restaurants/${RESTAURANT_ID}/tables/${table.id}/available-times?${query}`,
        );

        if (!response.ok) {
          throw new Error(await readError(response));
        }

        const data = await response.json();

        const normalizedTimes =
          data.availableTimes.map(normalizeTime);

        setAvailableTimes(normalizedTimes);

        if (
          selectedTime &&
          !normalizedTimes.includes(selectedTime)
        ) {
          onSelectTime("");
        }
      } catch (requestError) {
        console.error(requestError);
        setError(requestError.message);
      } finally {
        setIsLoading(false);
      }
    }

    loadAvailableTimes();
  }, [table.id, date, guests]);

  const groupedTimes = useMemo(() => {
    return {
      morning: availableTimes.filter(
        (time) => Number(time.slice(0, 2)) < 12,
      ),

      afternoon: availableTimes.filter((time) => {
        const hour = Number(time.slice(0, 2));

        return hour >= 12 && hour < 17;
      }),

      evening: availableTimes.filter(
        (time) => Number(time.slice(0, 2)) >= 17,
      ),
    };
  }, [availableTimes]);

  function renderGroup(title, times) {
    if (times.length === 0) {
      return null;
    }

    return (
      <div className="time-group">
        <div className="time-group-header">
          <h3>{title}</h3>
          <span>{times.length}</span>
        </div>

        <div className="time-buttons">
          {times.map((time) => (
            <button
              key={time}
              type="button"
              className={
                selectedTime === time
                  ? "time-button selected"
                  : "time-button"
              }
              onClick={() => onSelectTime(time)}
            >
              <strong>{time}</strong>
            </button>
          ))}
        </div>
      </div>
    );
  }

  return (
    <section className="table-time-picker">
      <button
        type="button"
        className="back-button"
        onClick={onBack}
      >
        ← Back to all tables
      </button>

      <div className="selected-table-header">
        <div>
          <p className="eyebrow">Selected table</p>
          <h2>{table.name}</h2>

          <p>
            {table.zone} · Up to {table.capacity} guests
          </p>
        </div>

        <span className="selected-table-capacity">
          {guests} {guests === 1 ? "guest" : "guests"}
        </span>
      </div>

      {isLoading && (
        <p className="time-message">
          Loading available times...
        </p>
      )}

      {error && <p className="error-message">{error}</p>}

      {!isLoading && !error && availableTimes.length === 0 && (
        <p className="time-message">
          No available times for this table.
        </p>
      )}

      {!isLoading && !error && (
        <div className="time-groups">
          {renderGroup("Morning", groupedTimes.morning)}
          {renderGroup("Afternoon", groupedTimes.afternoon)}
          {renderGroup("Evening", groupedTimes.evening)}
        </div>
      )}

      <div className="reserve-table-actions">
        <div>
          <span>Selected time</span>
          <strong>{selectedTime || "Choose a time"}</strong>
        </div>

        <button
          type="button"
          disabled={!selectedTime}
          onClick={onReserve}
        >
          Reserve this table
        </button>
      </div>
    </section>
  );
}

export default TableTimePicker;