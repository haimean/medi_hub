import React, { useState } from 'react';
import Device1 from '../../assets/images/device1.webp';
import Device2 from '../../assets/images/device3.webp';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { setSelectedBrand, setSelectedDeviceType } from '../../stores/commonStore'; // Import setDepartments

const Dashboard = () => {
    const navigate = useNavigate();
    const dispatch = useDispatch(); // Khởi tạo dispatch

    // Tên loại thiết bị
    const [kindDevices, setKindDevices] = useState([
        {
            value: 1,
            label: 'Roche',
            // avatar: 'roche-svg'
        },
        {
            value: 2,
            label: 'Beckman',
            // avatar: 'beckman-svg'
        },
        {
            value: 3,
            label: 'Abbott',
            // avatar: 'abbott-svg'
        },
        {
            value: 4,
            label: 'Tủ Mát'
        },
        {
            value: 5,
            label: 'Tủ Âm'
        },
        {
            value: 6,
            label: 'Lọc RO'
        }
    ]);

    const [statusDevices, setStatusDevices] = useState([
        {
            value: 'periodicMaintenancev',
            label: 'Bảo Dưỡng Định Kỳ'
        },
        {
            value: 'regularCalibration',
            label: 'Hiệu Chuẩn Định Kỳ'
        },
        {
            value: 'periodicReplacement',
            label: 'Thay Thế Định Kỳ'
        },
        {
            value: 'usingDevices',
            label: 'Thiết Bị Đang Sử Dụng'
        },
        {
            value: 'notUsingDevices',
            label: 'Thiết Bị Không Sử Dụng'
        },
        {
            value: 7,
            label: 'Thiết bị khác'
        }
    ]);

    return (
        <div className="medi-dashboard">
            <div className="dashboard-columns">
                <div className="dashboard-column status-devices">
                    <h3 className="dashboard-title">Trạng Thái Thiết Bị</h3>
                    {statusDevices.map((status, idx) => (
                        <div
                            key={status.value}
                            className="dashboard-card status-card"
                            onClick={() => {
                                // Bạn có thể thêm logic khi click vào status nếu cần
                            }}
                        >
                            {status.label}
                        </div>
                    ))}
                </div>

                <div className="dashboard-column kind-devices">
                    <h3 className="dashboard-title">Loại Thiết Bị</h3>
                    {kindDevices.map((kind, idx) => (
                        <div
                            key={kind.value}
                            className="dashboard-card kind-card"
                            onClick={() => {
                                dispatch(setSelectedDeviceType(kind.value));
                                navigate('/devices');
                            }}
                        >
                            {kind.label}
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export default Dashboard;