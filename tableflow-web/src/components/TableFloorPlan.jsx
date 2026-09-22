import { useEffect, useRef, useState } from "react";

const MIN_CANVAS_WIDTH = 1000;
const MIN_CANVAS_HEIGHT = 600;
const TABLE_HORIZONTAL_PADDING = 60;
const TABLE_VERTICAL_PADDING = 48;

function createPositions(tables) {
  return Object.fromEntries(
    tables.map((table) => [
      table.id,
      {
        x: table.xPosition,
        y: table.yPosition,
      },
    ]),
  );
}

function clamp(value, minimum, maximum) {
  return Math.min(Math.max(value, minimum), maximum);
}

function TableFloorPlan({
  tables,
  isSaving,
  onEdit,
  onSaveLayout,
}) {
  const floorPlanRef = useRef(null);
  const dragStateRef = useRef(null);

  const [positions, setPositions] = useState(() =>
    createPositions(tables),
  );

  const [dirtyTableIds, setDirtyTableIds] = useState(
    () => new Set(),
  );

  useEffect(() => {
    setPositions(createPositions(tables));
    setDirtyTableIds(new Set());
  }, [tables]);

  const canvasWidth = Math.max(
    MIN_CANVAS_WIDTH,
    ...tables.map((table) => table.xPosition + 120),
  );

  const canvasHeight = Math.max(
    MIN_CANVAS_HEIGHT,
    ...tables.map((table) => table.yPosition + 100),
  );

  function handlePointerDown(event, tableId) {
    if (event.button !== 0 || isSaving) {
      return;
    }

    const position = positions[tableId];

    if (!position) {
      return;
    }

    dragStateRef.current = {
      tableId,
      pointerId: event.pointerId,
      startPointerX: event.clientX,
      startPointerY: event.clientY,
      startTableX: position.x,
      startTableY: position.y,
      moved: false,
    };

    event.currentTarget.setPointerCapture(event.pointerId);
  }

  function handlePointerMove(event) {
    const dragState = dragStateRef.current;
    const floorPlan = floorPlanRef.current;

    if (
      !dragState ||
      !floorPlan ||
      dragState.pointerId !== event.pointerId
    ) {
      return;
    }

    const deltaX = event.clientX - dragState.startPointerX;
    const deltaY = event.clientY - dragState.startPointerY;

    if (Math.abs(deltaX) > 2 || Math.abs(deltaY) > 2) {
      dragState.moved = true;
    }

    const nextX = clamp(
      Math.round(dragState.startTableX + deltaX),
      TABLE_HORIZONTAL_PADDING,
      floorPlan.clientWidth - TABLE_HORIZONTAL_PADDING,
    );

    const nextY = clamp(
      Math.round(dragState.startTableY + deltaY),
      TABLE_VERTICAL_PADDING,
      floorPlan.clientHeight - TABLE_VERTICAL_PADDING,
    );

    setPositions((currentPositions) => ({
      ...currentPositions,
      [dragState.tableId]: {
        x: nextX,
        y: nextY,
      },
    }));
  }

  function handlePointerEnd(event) {
    const dragState = dragStateRef.current;

    if (
      !dragState ||
      dragState.pointerId !== event.pointerId
    ) {
      return;
    }

    if (dragState.moved) {
      setDirtyTableIds((currentIds) => {
        const nextIds = new Set(currentIds);
        nextIds.add(dragState.tableId);
        return nextIds;
      });
    }

    if (event.currentTarget.hasPointerCapture(event.pointerId)) {
      event.currentTarget.releasePointerCapture(event.pointerId);
    }

    dragStateRef.current = null;
  }

  function resetLayout() {
    setPositions(createPositions(tables));
    setDirtyTableIds(new Set());
  }

  async function saveLayout() {
    const changedTables = tables
      .filter((table) => dirtyTableIds.has(table.id))
      .map((table) => ({
        ...table,
        xPosition: positions[table.id].x,
        yPosition: positions[table.id].y,
      }));

    try {
      await onSaveLayout(changedTables);
      setDirtyTableIds(new Set());
    } catch {
      // Сообщение об ошибке показывает родительский компонент.
    }
  }

  return (
    <section className="floor-plan-card">
      <div className="floor-plan-info">
        <div>
          <strong>Restaurant floor plan</strong>
          <span>
            Drag tables to change their position. Double-click to edit.
          </span>
        </div>

        <span>
          {dirtyTableIds.size === 0
            ? "Layout is saved"
            : `${dirtyTableIds.size} unsaved ${
                dirtyTableIds.size === 1 ? "change" : "changes"
              }`}
        </span>
      </div>

      <div className="floor-plan-scroll">
        <div
          ref={floorPlanRef}
          className="management-floor-plan"
          style={{
            width: `${canvasWidth}px`,
            height: `${canvasHeight}px`,
          }}
        >
          <span className="floor-plan-label">Restaurant floor</span>

          {tables.map((table) => {
            const position = positions[table.id] ?? {
              x: table.xPosition,
              y: table.yPosition,
            };

            return (
              <button
                key={table.id}
                type="button"
                className={
                  table.isActive
                    ? "management-floor-table"
                    : "management-floor-table inactive"
                }
                style={{
                  left: `${position.x}px`,
                  top: `${position.y}px`,
                }}
                title="Drag to move. Double-click to edit."
                onDoubleClick={() => onEdit(table)}
                onPointerDown={(event) =>
                  handlePointerDown(event, table.id)
                }
                onPointerMove={handlePointerMove}
                onPointerUp={handlePointerEnd}
                onPointerCancel={handlePointerEnd}
              >
                <strong>{table.name}</strong>
                <span>{table.capacity} guests</span>
                <small>{table.zone}</small>
              </button>
            );
          })}
        </div>
      </div>

      {dirtyTableIds.size > 0 && (
        <div className="floor-plan-actions">
          <button
            className="secondary-button"
            type="button"
            disabled={isSaving}
            onClick={resetLayout}
          >
            Reset positions
          </button>

          <button
            type="button"
            disabled={isSaving}
            onClick={saveLayout}
          >
            {isSaving ? "Saving..." : "Save layout"}
          </button>
        </div>
      )}
    </section>
  );
}

export default TableFloorPlan;