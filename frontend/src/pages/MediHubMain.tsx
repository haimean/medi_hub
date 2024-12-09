// src/pages/MediHubMain.tsx
import React, { useEffect, useState } from 'react';
import { Outlet, useNavigate } from "react-router-dom";
import { Popconfirm, Button, message, Select } from 'antd';
import { apiLogout } from "../api/appApi";
import { useSelector, useDispatch } from 'react-redux'; // Import useDispatch
import { setAuth, setDepartment } from '../stores/commonStore';

// Định nghĩa kiểu cho RootState
interface RootState {
    username: string | null; // Định nghĩa kiểu cho username
    department: any | null; // Định nghĩa kiểu cho username
    departments: [] | null; // Định nghĩa kiểu cho departments
    userInfo: any | null; // Định nghĩa kiểu cho departments
}

/**
 * Page default init page (sidebar, topbar)
 * CreatedBy: PQ Huy (21.11.2024)
 */
const MediHubMain = () => {
    const username = useSelector((state: RootState) => state.username); // Lấy username từ store
    const userInfo = useSelector((state: RootState) => state.userInfo); // Lấy username từ store
    const departments: any = useSelector((state: RootState) => state.departments); // Lấy departments từ store
    const department: any = useSelector((state: RootState) => state.department); // Lấy department từ store

    const [departmentOption, setDepartmentOption] = useState<any>([]);
    const [isEnableDepartment, setIsEnableDepartment] = useState<boolean>(false);

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

    useEffect(() => {
        if (userInfo && departments?.length > 0) {
            if (userInfo.role.includes('Admin')) {
                setDepartmentOption(departments?.map((item: any) => {
                    return {
                        value: item?.id,
                        label: item?.name
                    }
                }))
            }

            dispatch(setDepartment({
                id: departments[0]?.id,
                name: departments[0]?.name,
            }))
        }
    }, [userInfo, departments])

    return (
        <div className="medihub-main w-full h-full">
            <div className="medi-topbar">
                <div className='medi-topbar__left'>
                    <div className="medi-topbar__logo"></div>
                    <div className='topbar__logo--content'>
                        <div className='content--text'>
                            Bệnh Viện Bạch Mai
                        </div>
                        <div className='content--department'>
                            {
                                !isEnableDepartment ? (
                                    <div className='content--text cursor-pointer' onClick={() => setIsEnableDepartment(true)}>{department?.name}</div>
                                ) : <Select
                                    value={department?.id ? department?.id : departments[0]?.id}
                                    style={{ width: '100%', maxHeight: '1.5rem' }}
                                    options={departmentOption}
                                    open={true} // Automatically open the dropdown
                                    onChange={(e) => {
                                        dispatch(setDepartment({
                                            id: e,
                                            name: departments?.find((x: any) => x.id == e)?.name,
                                        }));
                                        setIsEnableDepartment(false);
                                    }}
                                />
                            }
                        </div>
                    </div>
                </div>
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