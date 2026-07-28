import { useMemo } from "react";

function GeneralTimePicker({
  options,
  selectedTime,
  isLoading,
  onSelectTime,
}) {
  const groupedTimes = useMemo(() => {
    return {
      morning: options.filter(
        (option) => Number(option.time.slice(0, 2)) < 12,
      ),

      afternoon: options.filter((option) => {
        const hour = Number(option.time.slice(0, 2));

        return hour >= 12 && hour < 17;
      }),

      evening: options.filter(
        (option) => Number(option.time.slice(0, 2)) >= 17,
      ),
    };
  }, [options]);

  function renderGroup(title, groupOptions) {
    if (groupOptions.length === 0) {
      return null;
    }

    return (
      <div className="time-group">
        <div className="time-group-header">
          <h3>{title}</h3>
          <span>{groupOptions.length}</span>
        </div>

        <div className="time-buttons">
          {groupOptions.map((option) => (
            <button
              key={option.time}
              type="button"
              className={
                selectedTime === option.time
                  ? "time-button selected"
                  : "time-button"
              }
              onClick={() => onSelectTime(option.time)}
            >
              <strong>{option.time}</strong>

              <small>
                {option.availableTableCount}{" "}
                {option.availableTableCount === 1
                  ? "table"
                  : "tables"}
              </small>
            </button>
          ))}
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="general-time-picker">
        <p className="time-message">
          Loading available times...
        </p>
      </div>
    );
  }

  if (options.length === 0) {
    return (
      <div className="general-time-picker">
        <p className="time-message">
          No reservation times are available for this date.
        </p>
      </div>
    );
  }

  return (
    <section className="general-time-picker">
      <div className="time-picker-title">
        <div>
          <p className="eyebrow">Available times</p>
          <h2>Select a time</h2>
        </div>

        <span>Shows all available tables</span>
      </div>

      <div className="time-groups">
        {renderGroup("Morning", groupedTimes.morning)}
        {renderGroup("Afternoon", groupedTimes.afternoon)}
        {renderGroup("Evening", groupedTimes.evening)}
      </div>
    </section>
  );
}

export default GeneralTimePicker;