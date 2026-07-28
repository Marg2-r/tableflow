import { useCallback, useEffect, useState } from "react";
import "./App.css";

import GeneralTimePicker from "./components/GeneralTimePicker";
import ManagementSettings from "./components/ManagementSettings";
import ReservationForm from "./components/ReservationForm";
import TableTimePicker from "./components/TableTimePicker";
import { API_URL, RESTAURANT_ID } from "./config";

function formatLocalDate(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

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

    if (typeof parsed === "string") {
      return parsed;
    }

    return parsed.title || parsed.message || text;
  } catch {
    return text;
  }
}

function App() {
  const today = formatLocalDate(new Date());

  const [activePage, setActivePage] = useState("booking");

  const [date, setDate] = useState(today);
  const [guests, setGuests] = useState(2);

  const [generalTimes, setGeneralTimes] = useState([]);
  const [searchTime, setSearchTime] = useState("");

  const [tables, setTables] = useState([]);
  const [selectedTable, setSelectedTable] = useState(null);
  const [selectedTableTime, setSelectedTableTime] = useState("");

  const [isOverviewLoading, setIsOverviewLoading] = useState(false);
  const [isTablesLoading, setIsTablesLoading] = useState(false);

  const [isReservationFormOpen, setIsReservationFormOpen] =
    useState(false);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const loadAvailableTables = useCallback(
    async (time) => {
      if (!time) {
        setTables([]);
        return;
      }

      setIsTablesLoading(true);
      setError("");

      try {
        const query = new URLSearchParams({
          date,
          time: `${time}:00`,
          guests: guests.toString(),
        });

        const response = await fetch(
          `${API_URL}/restaurants/${RESTAURANT_ID}/tables/available?${query}`,
        );

        if (!response.ok) {
          throw new Error(await readError(response));
        }

        const data = await response.json();

        setTables(data);
      } catch (requestError) {
        console.error(requestError);
        setTables([]);
        setError(requestError.message);
      } finally {
        setIsTablesLoading(false);
      }
    },
    [date, guests],
  );

  const loadGeneralAvailability = useCallback(async () => {
    setIsOverviewLoading(true);
    setError("");
    setSuccessMessage("");

    setSelectedTable(null);
    setSelectedTableTime("");

    try {
      const query = new URLSearchParams({
        date,
        guests: guests.toString(),
      });

      const response = await fetch(
        `${API_URL}/restaurants/${RESTAURANT_ID}/tables/available-times?${query}`,
      );

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      const data = await response.json();

      const normalizedTimes = data.availableTimes.map((option) => ({
        time: normalizeTime(option.time),
        availableTableCount: option.availableTableCount,
      }));

      setGeneralTimes(normalizedTimes);

      if (normalizedTimes.length === 0) {
        setSearchTime("");
        setTables([]);
        return;
      }

      // Первый элемент backend уже возвращает как ближайший
      // доступный временной слот.
      const nearestTime = normalizedTimes[0].time;

      setSearchTime(nearestTime);
      await loadAvailableTables(nearestTime);
    } catch (requestError) {
      console.error(requestError);

      setGeneralTimes([]);
      setSearchTime("");
      setTables([]);
      setError(requestError.message);
    } finally {
      setIsOverviewLoading(false);
    }
  }, [date, guests, loadAvailableTables]);

  useEffect(() => {
    if (activePage === "booking") {
      loadGeneralAvailability();
    }
  }, [activePage, loadGeneralAvailability]);

  async function handleGeneralTimeSelect(time) {
    setSearchTime(time);
    setSelectedTable(null);
    setSelectedTableTime("");
    setSuccessMessage("");

    await loadAvailableTables(time);
  }

  function handleTableSelect(table) {
    setSelectedTable(table);

    // Стол уже был найден свободным на searchTime,
    // поэтому это время можно выделить сразу.
    setSelectedTableTime(searchTime);
    setSuccessMessage("");
  }

  function handleBackToAllTables() {
    setSelectedTable(null);
    setSelectedTableTime("");
    setIsReservationFormOpen(false);
  }

  async function handleReservationCreated(reservation) {
    setIsReservationFormOpen(false);
    setSelectedTable(null);
    setSelectedTableTime("");

    setSuccessMessage(
      `Reservation #${reservation.id} was created successfully.`,
    );

    // Перезагружаем времена и карту, потому что доступность изменилась.
    await loadGeneralAvailability();
  }

  return (
    <>
      <nav className="app-navigation">
        <div className="navigation-content">
          <strong className="app-logo">TableFlow</strong>

          <div className="navigation-buttons">
            <button
              type="button"
              className={activePage === "booking" ? "active" : ""}
              onClick={() => setActivePage("booking")}
            >
              Booking
            </button>

            <button
              type="button"
              className={activePage === "management" ? "active" : ""}
              onClick={() => setActivePage("management")}
            >
              Management
            </button>
          </div>
        </div>
      </nav>

      {activePage === "management" ? (
        <ManagementSettings />
      ) : (
        <main className="page">
          <section className="hero">
            <p className="eyebrow">Restaurant reservation platform</p>
            <h1>Book your table</h1>

            <p className="hero-description">
              Choose a date, party size and one of the available times.
            </p>
          </section>

          <section className="search-card">
            <div className="booking-controls">
              <label>
                Date

                <input
                  type="date"
                  min={today}
                  value={date}
                  onChange={(event) => setDate(event.target.value)}
                />
              </label>

              <label>
                Guests

                <input
                  type="number"
                  min="1"
                  max="50"
                  value={guests}
                  onChange={(event) =>
                    setGuests(Number(event.target.value))
                  }
                />
              </label>
            </div>

            {selectedTable ? (
              <TableTimePicker
                table={selectedTable}
                date={date}
                guests={guests}
                selectedTime={selectedTableTime}
                onSelectTime={setSelectedTableTime}
                onBack={handleBackToAllTables}
                onReserve={() => setIsReservationFormOpen(true)}
              />
            ) : (
              <GeneralTimePicker
                options={generalTimes}
                selectedTime={searchTime}
                isLoading={isOverviewLoading}
                onSelectTime={handleGeneralTimeSelect}
              />
            )}

            {error && <p className="error-message">{error}</p>}

            {successMessage && (
              <p className="success-message">{successMessage}</p>
            )}
          </section>

          <section className="results">
            <div className="results-header">
              <div>
                <p className="eyebrow">Availability</p>

                <h2>
                  {searchTime
                    ? `Available tables at ${searchTime}`
                    : "Available tables"}
                </h2>
              </div>

              <span className="result-count">
                {tables.length}{" "}
                {tables.length === 1 ? "table" : "tables"}
              </span>
            </div>

            {isTablesLoading ? (
              <div className="empty-state">
                Loading available tables...
              </div>
            ) : tables.length === 0 ? (
              <div className="empty-state">
                No tables are available for the selected time.
              </div>
            ) : (
              <div className="restaurant-map">
                {tables.map((table) => (
                  <button
                    key={table.id}
                    type="button"
                    className={
                      selectedTable?.id === table.id
                        ? "table-item selected"
                        : "table-item"
                    }
                    style={{
                      left: `${table.xPosition}px`,
                      top: `${table.yPosition}px`,
                    }}
                    onClick={() => handleTableSelect(table)}
                  >
                    <strong>{table.name}</strong>
                    <span>{table.capacity} guests</span>
                    <small>{table.zone}</small>
                  </button>
                ))}
              </div>
            )}
          </section>

          {isReservationFormOpen &&
            selectedTable &&
            selectedTableTime && (
              <ReservationForm
                table={selectedTable}
                date={date}
                time={selectedTableTime}
                guests={guests}
                onClose={() => setIsReservationFormOpen(false)}
                onReserved={handleReservationCreated}
              />
            )}
        </main>
      )}
    </>
  );
}

export default App;