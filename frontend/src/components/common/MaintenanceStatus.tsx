// MaintenanceStatus.tsx
import React from 'react';
import dayjs from 'dayjs';

interface MaintenanceStatusProps {
    maintenanceDate: number; // Expecting a date string in ISO format
}

const MaintenanceStatus: React.FC<MaintenanceStatusProps> = ({ maintenanceDate }) => {
    const today = dayjs(); // Get today's date
    const maintenanceMoment = dayjs(maintenanceDate); // Convert the maintenance date to a moment object
    const daysUntilMaintenance = maintenanceMoment.diff(today, 'days'); // Calculate the difference in days

    let statusMessage = '';
    let statusColor = '';

    if (maintenanceDate == 0) {
        // Overdue
        statusMessage = 'Đang sử dụng';
        statusColor = 'red'; // Red color for overdue
    } else if (maintenanceDate == 1) {
        // Due soon
        statusMessage = 'Đang sửa chữa';
        statusColor = 'orange'; // Orange color for due soon
    } else if (maintenanceDate == 2){
        // Not due yet
        statusMessage = 'Chờ thẩm định';
        statusColor = 'green'; 
    }else if (maintenanceDate == 3) {
        statusMessage = 'Không sử dụng';
        statusColor = 'green'; 
    }

    return (
        <div className='font-bold' style={{ color: statusColor }}>
            {statusMessage}
        </div>
    );
};

export default MaintenanceStatus;