import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import MediHubMain from '../../pages/MediHubMain';
import axios from 'axios';

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
            axios.get('/api/check-token', { params: { token } })
                .then((response: any) => {
                    if (response.data.valid) {
                        setIsAuth(true); // Token hợp lệ
                    } else {
                        navigate('/login'); // Token không hợp lệ, chuyển hướng đến login
                    }
                })
                .catch((error: any) => {
                    console.error("Error checking token:", error);
                    navigate('/login'); // Có lỗi khi kiểm tra token, chuyển hướng đến login
                });
        }
    }, [navigate]);

    return (
        <div className='w-screen h-screen'>
            {
                isAuth ? <MediHubMain /> : <div> You do not have permission ! </div>
            }
        </div>
    );
}

export default AuthRoutes;