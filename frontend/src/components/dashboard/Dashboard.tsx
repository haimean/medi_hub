import React, { useState } from 'react';
import Device1 from '../../assets/images/device1.webp';
import Device2 from '../../assets/images/device2.webp';

const Dashboard = () => {
    return (
        <div className="medi-dashboard">
            <div className='dashboard-row-1'>
                <img className='row-1--device-1' src={Device1} alt="device-1" />
                <div className='row-1--app-title'>
                    MEDICAL EQUIPMENT LAB
                </div>
                <img className='row-1--device-2' src={Device2} alt="device-1" />
            </div>
            <div className='dashboard-row-2'>
                <div className='row-2--group-1'>

                </div>
                <div className='row-2--group-2'>

                </div>
                <div className='row-2--group-3'>

                </div>
            </div>
            <div className='dashboard-row-3'>
                <div className='row-3--branch-1'>

                </div>
                <div className='row-3--branch-2'>

                </div>
                <div className='row-3--branch-3'>

                </div>
            </div>
            <div className='dashboard-row-4'>
                <div className='row-4--kind-1'>

                </div>
                <div className='row-4--kind-2'>

                </div>
                <div className='row-4--kind-3'>

                </div>
                <div className='row-4--kind-4'>

                </div>
            </div>
        </div>
    );
}

export default Dashboard;