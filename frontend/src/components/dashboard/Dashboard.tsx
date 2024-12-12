import React, { useState } from 'react';
import Device1 from '../../assets/images/device1.webp';
import Device2 from '../../assets/images/device3.webp';

const Dashboard = () => {

    // Tên nhánh
    const [lstBranch, setLstBranch] = useState([
        {
            value: 'roche',
            label: 'Roche',
            avatar: 'roche-svg'
        },
        {
            value: 'beckmanCoulter',
            label: 'Beckman Coulter',
            avatar: 'beckman-svg'
        },
        {
            value: 'abbottDiagnostics',
            label: 'Abbott Diagnostics',
            avatar: 'abbott-svg'
        }
    ]);

    // Tên loại thiết bị
    const [kindDevices, setKindDevices] = useState([
        {
            value: 'tumat',
            label: 'Tủ Mát'
        },
        {
            value: 'tuam',
            label: 'Tủ Âm'
        },
        {
            value: 'locro',
            label: 'Lọc RO'
        },
        {
            value: 'khac',
            label: 'Khác'
        }
    ]);

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
                <div className='row-2--group status-due-soon'>
                    <div className='group-1--title'>Bảo Dưỡng Định Kỳ</div>
                    <div className='group-1--info'>
                        <div className='info__number'>45</div>
                        <div className='info__status-name'>Sắp đến hạn</div>
                    </div>
                </div>
                <div className='row-2--group status-overdue'>
                    <div className='group-1--title'>Thay thế Định Kỳ</div>
                    <div className='group-1--info'>
                        <div className='info__number'>45</div>
                        <div className='info__status-name'>Sắp đến hạn</div>
                    </div>
                </div>
                <div className='row-2--group status-notyet-due'>
                    <div className='group-1--title'>Bảo Dưỡng Định Kỳ</div>
                    <div className='group-1--info'>
                        <div className='info__number'>45</div>
                        <div className='info__status-name'>Sắp đến hạn</div>
                    </div>
                </div>
            </div>
            <div className='dashboard-row-3'>
                {
                    lstBranch?.map((branch: any, index: any) => {
                        return (
                            <div key={`dashboard-branch-${index}`} className={`row-3--branch`}
                                style={{ flex: `0 0 ${99 / lstBranch?.length}%` }} // Dynamic width
                            >
                                <div className={`branch-img ${branch?.avatar}`}></div>
                            </div>
                        )
                    })
                }
            </div>
            <div className='dashboard-row-4'>
                {
                    kindDevices?.map((branch: any, index: any) => {
                        return (
                            <div key={`dashboard-kind-${index}`} className={`row-3--kind`}
                                style={{ flex: `0 0 ${99 / kindDevices?.length}%` }} // Dynamic width
                            >{branch?.label}</div>
                        )
                    })
                }
            </div>
        </div>
    );
}

export default Dashboard;