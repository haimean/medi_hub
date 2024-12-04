// src/pages/MediHubMain.tsx
import React from 'react';
import { Outlet, useNavigate } from "react-router-dom";
import { Popconfirm, Button, message } from 'antd';
import { apiLogout } from "../api/appApi";
import { useSelector, useDispatch } from 'react-redux'; // Import useDispatch
import { setAuth } from '../stores/commonStore';

// Định nghĩa kiểu cho RootState
interface RootState {
    username: string | null; // Định nghĩa kiểu cho username
}

/**
 * Page default init page (sidebar, topbar)
 * CreatedBy: PQ Huy (21.11.2024)
 */
const MediHubMain = () => {
    const username = useSelector((state: RootState) => state.username); // Lấy username từ store
    const dispatch = useDispatch(); // Khởi tạo dispatch
    let navigate = useNavigate();

    const handleLogout = async () => {
        if (username) {
            await apiLogout(username); // Gọi API logout
            message.success('Đăng xuất thành công!');
            // Xóa token và cập nhật trạng thái isAuth thành false
            localStorage.removeItem('MEDI.Token');
            dispatch(setAuth(false)); // Cập nhật isAuth trong store
            // Thực hiện chuyển hướng đến trang login
            navigate('/login');
        }
    };

    return (
        <div className="medihub-main w-full h-full">
            <div className="medi-topbar">
                <div className="medi-topbar__logo"></div>
                <div className="medi-topbar__right">
                    <div className="medi-icon topbar__right--setting"></div>
                    <div className="topbar__right--notification"></div>
                    <div className="topbar__right--user">
                        <Popconfirm
                            placement="bottomLeft"
                            title="Bạn có chắc chắn muốn đăng xuất?"
                            onConfirm={handleLogout}
                            okText="Có"
                            cancelText="Không"
                        >
                            <Button
                                className="logout-button"
                                style={{ background: 'none', border: 'none', padding: 0 }}
                            >
                                <div className="user-icon" style={{ backgroundImage: 'url(path/to/your/icon.png)', width: '24px', height: '24px' }}></div>
                            </Button>
                        </Popconfirm>
                    </div>
                </div>
            </div>
            <div className="medi-content flex">
                <div className="medi-content--overlay"></div>
                <div className='medi-content__detail'>
                    <Outlet />
                </div>
            </div>
        </div>
    );
}

export default MediHubMain;