// MaintenanceStatus.tsx
import React from 'react';
import dayjs from 'dayjs';

interface MaintenanceStatusProps {
    maintenanceDate: string; // Expecting a date string in ISO format
}

const MaintenanceStatus: React.FC<MaintenanceStatusProps> = ({ maintenanceDate }) => {
    const today = dayjs(); // Get today's date
    const maintenanceMoment = dayjs(maintenanceDate); // Convert the maintenance date to a moment object
    const daysUntilMaintenance = maintenanceMoment.diff(today, 'days'); // Calculate the difference in days

    let statusMessage = '';
    let statusColor = '';

    if (daysUntilMaintenance < 0) {
        // Overdue
        statusMessage = 'Đã đến hạn';
        statusColor = 'red'; // Red color for overdue
    } else if (daysUntilMaintenance <= 15) {
        // Due soon
        statusMessage = 'Sắp đến hạn';
        statusColor = 'orange'; // Orange color for due soon
    } else {
        // Not due yet
        statusMessage = 'Chưa đến hạn';
        statusColor = 'green'; // Green color for not due yet
    }

    return (
        <div className='font-bold' style={{ color: statusColor }}>
            {statusMessage}
        </div>
    );
};

export default MaintenanceStatus;