import { useState } from "react";
import ManagementSettings from "./ManagementSettings";
import ReservationManagement from "./ReservationManagement";
import TableManagement from "./TableManagement";

const sections = [
  {
    id: "settings",
    label: "Settings",
  },
  {
    id: "tables",
    label: "Tables",
  },
  {
    id: "reservations",
    label: "Reservations",
  },
];

function ManagementPage({ restaurantId }) {
  const [activeSection, setActiveSection] = useState("settings");

  function renderActiveSection() {
    if (activeSection === "tables") {
      return <TableManagement restaurantId={restaurantId} />;
    }

    if (activeSection === "reservations") {
      return (
        <ReservationManagement restaurantId={restaurantId} />
      );
    }

    return <ManagementSettings restaurantId={restaurantId} />;
  }

  return (
    <>
      <div className="management-tabs-shell">
        <nav
          className="management-tabs"
          aria-label="Management sections"
        >
          {sections.map((section) => (
            <button
              key={section.id}
              type="button"
              className={
                activeSection === section.id
                  ? "management-tab active"
                  : "management-tab"
              }
              aria-current={
                activeSection === section.id
                  ? "page"
                  : undefined
              }
              onClick={() => setActiveSection(section.id)}
            >
              {section.label}
            </button>
          ))}
        </nav>
      </div>

      {renderActiveSection()}
    </>
  );
}

export default ManagementPage;
