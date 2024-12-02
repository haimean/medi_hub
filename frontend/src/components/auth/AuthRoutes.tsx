import React, { useEffect, useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import MediHubMain from '../../pages/MediHubMain';
import { apiCheckPermission } from '../../api/appApi';

/**
 * Component check auth and return outlet if has auth
 * @returns 
 * CreatedBy: PQ Huy (21.11.2024)
 */
const AuthRoutes = () => {
    let navigate = useNavigate();
    const [isAuth, setIsAuth] = useState<boolean>(false); // Mặc định là false

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

                    if (response.data) {
                        setIsAuth(true); // Token hợp lệ
                        if(firstSegment != 'dashboard') {
                            navigate('/dashboard');
                        }
                    } else {
                        setIsAuth(false); // Token ko hợp lệ
                        if(firstSegment != 'login') {
                            navigate('/login'); // Có lỗi khi kiểm tra token, chuyển hướng đến login
                        }
                    }
                })
                .catch((error: any) => {
                    setIsAuth(false); // Token hợp lệ
                    navigate('/login'); // Có lỗi khi kiểm tra token, chuyển hướng đến login
                });
        }
    }, [navigate]);

    return (
        <div className='w-screen h-screen'>
            {
                isAuth ? <MediHubMain /> : <Outlet />
            }
        </div>
    );
}

export default AuthRoutes;