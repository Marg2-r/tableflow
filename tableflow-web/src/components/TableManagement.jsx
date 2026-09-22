import { useEffect, useState } from "react";
import { API_URL, RESTAURANT_ID } from "../config";
import TableFloorPlan from "./TableFloorPlan";

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

function TableManagement() {
  const [tables, setTables] = useState([]);
  const [formData, setFormData] = useState(null);
  const [editingTableId, setEditingTableId] = useState(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [deactivatingId, setDeactivatingId] = useState(null);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const [activeView, setActiveView] = useState("floor");
  const [isSavingLayout, setIsSavingLayout] = useState(false);

  useEffect(() => {
    loadTables();
  }, []);

  async function loadTables() {
    setIsLoading(true);
    setError("");

    try {
      const response = await fetch(
        `${API_URL}/restaurants/${RESTAURANT_ID}/tables`,
      );

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      const data = await response.json();

      setTables(data);
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setIsLoading(false);
    }
  }

  function openCreateForm() {
    const tableIndex = tables.length;

    setEditingTableId(null);
    setFormData({
      name: "",
      capacity: 2,
      zone: "Main hall",
      xPosition: 100 + (tableIndex % 4) * 180,
      yPosition: 100 + Math.floor(tableIndex / 4) * 140,
      isActive: true,
    });

    setError("");
    setSuccessMessage("");
  }

  function openEditForm(table) {
    setEditingTableId(table.id);
    setFormData({
      name: table.name,
      capacity: table.capacity,
      zone: table.zone,
      xPosition: table.xPosition,
      yPosition: table.yPosition,
      isActive: table.isActive,
    });

    setError("");
    setSuccessMessage("");
  }

  function closeForm() {
    if (isSaving) {
      return;
    }

    setFormData(null);
    setEditingTableId(null);
  }

  function handleChange(event) {
    const { name, value, type, checked } = event.target;

    setFormData((current) => ({
      ...current,
      [name]:
        type === "checkbox"
          ? checked
          : type === "number"
            ? Number(value)
            : value,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setIsSaving(true);
    setError("");
    setSuccessMessage("");

    const isEditing = editingTableId !== null;

    const url = isEditing
      ? `${API_URL}/restaurants/${RESTAURANT_ID}/tables/${editingTableId}`
      : `${API_URL}/restaurants/${RESTAURANT_ID}/tables`;

    try {
      const response = await fetch(url, {
        method: isEditing ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      const savedTable = await response.json();

      setTables((currentTables) => {
        if (isEditing) {
          return currentTables.map((table) =>
            table.id === savedTable.id ? savedTable : table,
          );
        }

        return [...currentTables, savedTable].sort(
          (first, second) => first.id - second.id,
        );
      });

      setSuccessMessage(
        isEditing
          ? `${savedTable.name} was updated.`
          : `${savedTable.name} was created.`,
      );

      setFormData(null);
      setEditingTableId(null);
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setIsSaving(false);
    }
  }


  async function handleSaveLayout(changedTables) {
    setIsSavingLayout(true);
    setError("");
    setSuccessMessage("");

    try {
        const results = await Promise.allSettled(
        changedTables.map(async (table) => {
            const response = await fetch(
            `${API_URL}/restaurants/${RESTAURANT_ID}/tables/${table.id}`,
            {
                method: "PUT",
                headers: {
                "Content-Type": "application/json",
                },
                body: JSON.stringify({
                name: table.name,
                capacity: table.capacity,
                zone: table.zone,
                xPosition: table.xPosition,
                yPosition: table.yPosition,
                isActive: table.isActive,
                }),
            },
            );

            if (!response.ok) {
            throw new Error(await readError(response));
            }

            return response.json();
        }),
        );

        const savedTables = results
        .filter((result) => result.status === "fulfilled")
        .map((result) => result.value);

        if (savedTables.length > 0) {
        const savedTablesById = new Map(
            savedTables.map((table) => [table.id, table]),
        );

        setTables((currentTables) =>
            currentTables.map(
            (table) => savedTablesById.get(table.id) ?? table,
            ),
        );
        }

        const failedResult = results.find(
        (result) => result.status === "rejected",
        );

        if (failedResult) {
        throw failedResult.reason;
        }

        setSuccessMessage(
        `${savedTables.length} table ${
            savedTables.length === 1 ? "position" : "positions"
        } saved.`,
        );
    } catch (requestError) {
        console.error(requestError);
        setError(requestError.message);
        throw requestError;
    } finally {
        setIsSavingLayout(false);
    }
    }

  async function handleDeactivate(table) {
    const confirmed = window.confirm(
      `Deactivate ${table.name}? It will no longer be available for new reservations.`,
    );

    if (!confirmed) {
      return;
    }

    setDeactivatingId(table.id);
    setError("");
    setSuccessMessage("");

    try {
      const response = await fetch(
        `${API_URL}/restaurants/${RESTAURANT_ID}/tables/${table.id}`,
        {
          method: "DELETE",
        },
      );

      if (!response.ok) {
        throw new Error(await readError(response));
      }

      setTables((currentTables) =>
        currentTables.map((currentTable) =>
          currentTable.id === table.id
            ? {
                ...currentTable,
                isActive: false,
              }
            : currentTable,
        ),
      );

      setSuccessMessage(`${table.name} was deactivated.`);
    } catch (requestError) {
      console.error(requestError);
      setError(requestError.message);
    } finally {
      setDeactivatingId(null);
    }
  }

  return (
    <main className="page">
      <section className="table-management-header">
        <div className="hero management-hero">
          <p className="eyebrow">Floor management</p>
          <h1>Restaurant tables</h1>

          <p className="hero-description">
            Manage table capacity, restaurant zones and availability.
          </p>
        </div>

        <button
          className="primary-management-button"
          type="button"
          onClick={openCreateForm}
        >
          + Add table
        </button>
      </section>

      {error && <p className="error-message">{error}</p>}

      {successMessage && (
        <p className="success-message">{successMessage}</p>
      )}
            {isLoading ? (
            <section className="table-list-card">
                <div className="table-list-message">
                Loading tables...
                </div>
            </section>
            ) : tables.length === 0 ? (
            <section className="table-list-card">
                <div className="table-list-message">
                <strong>No tables yet</strong>
                <p>Add the first table to start building the floor plan.</p>

                <button type="button" onClick={openCreateForm}>
                    Add first table
                </button>
                </div>
            </section>
            ) : (
            <>
                <div className="table-view-toolbar">
                <div
                    className="table-view-switcher"
                    aria-label="Table view"
                >
                    <button
                    type="button"
                    className={activeView === "floor" ? "active" : ""}
                    aria-pressed={activeView === "floor"}
                    onClick={() => setActiveView("floor")}
                    >
                    Floor plan
                    </button>

                    <button
                    type="button"
                    className={activeView === "list" ? "active" : ""}
                    aria-pressed={activeView === "list"}
                    onClick={() => setActiveView("list")}
                    >
                    Table list
                    </button>
                </div>

                <span>
                    {tables.filter((table) => table.isActive).length} active of{" "}
                    {tables.length} tables
                </span>
                </div>

                {activeView === "floor" ? (
                <TableFloorPlan
                    tables={tables}
                    isSaving={isSavingLayout}
                    onEdit={openEditForm}
                    onSaveLayout={handleSaveLayout}
                />
                ) : (
                <section className="table-list-card">
                    <table className="management-table">
                    <thead>
                        <tr>
                        <th>Name</th>
                        <th>Capacity</th>
                        <th>Zone</th>
                        <th>Position</th>
                        <th>Status</th>
                        <th aria-label="Actions" />
                        </tr>
                    </thead>

                    <tbody>
                        {tables.map((table) => (
                        <tr key={table.id}>
                            <td>
                            <strong>{table.name}</strong>
                            <small>Table #{table.id}</small>
                            </td>

                            <td>{table.capacity} guests</td>
                            <td>{table.zone}</td>

                            <td>
                            {table.xPosition}, {table.yPosition}
                            </td>

                            <td>
                            <span
                                className={
                                table.isActive
                                    ? "table-status active"
                                    : "table-status inactive"
                                }
                            >
                                {table.isActive ? "Active" : "Inactive"}
                            </span>
                            </td>

                            <td>
                            <div className="table-row-actions">
                                <button
                                type="button"
                                onClick={() => openEditForm(table)}
                                >
                                Edit
                                </button>

                                {table.isActive && (
                                <button
                                    className="danger-text-button"
                                    type="button"
                                    disabled={deactivatingId === table.id}
                                    onClick={() => handleDeactivate(table)}
                                >
                                    {deactivatingId === table.id
                                    ? "Deactivating..."
                                    : "Deactivate"}
                                </button>
                                )}
                            </div>
                            </td>
                        </tr>
                        ))}
                    </tbody>
                    </table>
                </section>
                )}
            </>
            )}
            
      {formData && (
        <div className="modal-backdrop" onMouseDown={closeForm}>
          <section
            className="reservation-modal table-editor-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="table-editor-title"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <div className="modal-header">
              <div>
                <p className="eyebrow">
                  {editingTableId === null
                    ? "New table"
                    : "Edit table"}
                </p>

                <h2 id="table-editor-title">
                  {editingTableId === null
                    ? "Add a restaurant table"
                    : formData.name}
                </h2>
              </div>

              <button
                className="close-button"
                type="button"
                aria-label="Close"
                disabled={isSaving}
                onClick={closeForm}
              >
                ×
              </button>
            </div>

            <form
              className="table-editor-form"
              onSubmit={handleSubmit}
            >
              <label className="full-width">
                Table name
                <input
                  type="text"
                  name="name"
                  maxLength="100"
                  value={formData.name}
                  onChange={handleChange}
                  required
                />
              </label>

              <label>
                Capacity
                <input
                  type="number"
                  name="capacity"
                  min="1"
                  max="50"
                  value={formData.capacity}
                  onChange={handleChange}
                  required
                />
              </label>

              <label>
                Zone
                <input
                  type="text"
                  name="zone"
                  maxLength="100"
                  value={formData.zone}
                  onChange={handleChange}
                  required
                />
              </label>

              <label>
                X position
                <input
                  type="number"
                  name="xPosition"
                  min="0"
                  max="5000"
                  value={formData.xPosition}
                  onChange={handleChange}
                  required
                />
              </label>

              <label>
                Y position
                <input
                  type="number"
                  name="yPosition"
                  min="0"
                  max="5000"
                  value={formData.yPosition}
                  onChange={handleChange}
                  required
                />
              </label>

              <label className="table-active-field full-width">
                <input
                  type="checkbox"
                  name="isActive"
                  checked={formData.isActive}
                  onChange={handleChange}
                />

                <span>
                  <strong>Active table</strong>
                  <small>
                    Active tables are available for new reservations.
                  </small>
                </span>
              </label>

              <div className="modal-actions">
                <button
                  className="secondary-button"
                  type="button"
                  disabled={isSaving}
                  onClick={closeForm}
                >
                  Cancel
                </button>

                <button type="submit" disabled={isSaving}>
                  {isSaving
                    ? "Saving..."
                    : editingTableId === null
                      ? "Create table"
                      : "Save changes"}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </main>
  );
}

export default TableManagement;