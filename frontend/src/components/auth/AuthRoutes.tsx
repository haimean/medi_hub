// src/components/AuthRoutes.tsx
import React, { useEffect } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import MediHubMain from '../../pages/MediHubMain';
import { apiCheckPermission } from '../../api/appApi';
import { useDispatch, useSelector } from 'react-redux'; // Import useDispatch và useSelector
import { setAuth, setUsername } from '../../stores/commonStore';

/**
 * Component check auth and return outlet if has auth
 * @returns 
 * CreatedBy: PQ Huy (21.11.2024)
 */
const AuthRoutes = () => {
    let navigate = useNavigate();
    const dispatch = useDispatch(); // Khởi tạo dispatch
    const isAuth = useSelector((state: any) => state.isAuth); // Lấy isAuth từ store

    // check has token or not
    useEffect(() => {
        const token = localStorage.getItem('MEDI.Token');
        if (!token) {
            // Nếu không có token, chuyển hướng đến trang login
            navigate('/login');
        } else {
            // Nếu có token, gọi API để kiểm tra tính hợp lệ của token
            apiCheckPermission(token)
                .then((response: any) => {
                    const path = window.location.pathname; // Lấy đường dẫn hiện tại
                    const firstSegment = path.split('/')[1]; // Tách chuỗi và lấy phần đầu tiên sau dấu /

                    if (response.data?.isValid) {
                        dispatch(setAuth(true)); // Cập nhật isAuth trong store
                        dispatch(setUsername(response.data.user?.username)); // Lưu tên người dùng vào store
                        if (firstSegment !== 'dashboard') {
                            navigate('/dashboard');
                        }
                    } else {
                        dispatch(setAuth(false)); // Cập nhật isAuth trong store
                        if (firstSegment !== 'login') {
                            navigate('/login'); // Có lỗi khi kiểm tra token, chuyển hướng đến login
                        }
                    }
                })
                .catch((error: any) => {
                    dispatch(setAuth(false)); // Cập nhật isAuth trong store
                    navigate('/login'); // Có lỗi khi kiểm tra token, chuyển hướng đến login
                });
        }
    }, [navigate, dispatch]);

    return (
        <div className='w-screen h-screen'>
            {
                isAuth ? <MediHubMain /> : <Outlet />
            }
        </div>
    );
}

export default AuthRoutes;