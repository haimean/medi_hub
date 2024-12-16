// src/components/AuthRoutes.tsx
import React, { useEffect } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import MediHubMain from '../../pages/MediHubMain';
import { useDispatch, useSelector } from 'react-redux'; // Import useDispatch và useSelector
import { setAuth, setUsername, setDepartments, setUserInfo } from '../../stores/commonStore'; // Import setDepartments
import { apiCheckPermission, getDepartments } from '../../api/appApi'; // Import the new API function
import { useQuery } from '@tanstack/react-query';
import { message } from 'antd';

/**
 * Component check auth and return outlet if has auth
 * @returns 
 * CreatedBy: PQ Huy (21.11.2024)
 */
const AuthRoutes = () => {
    let navigate = useNavigate();
    const dispatch = useDispatch(); // Khởi tạo dispatch
    const isAuth = useSelector((state: any) => state.isAuth); // Lấy isAuth từ store

    // Get token from local storage
    const token = localStorage.getItem('MEDI.Token');

    // Use useQuery to check token validity
    const { isError, isLoading, data } = useQuery({
        queryKey: [`apiCheckPermission ${token}`, token],
        queryFn: () => apiCheckPermission(token),
        refetchOnWindowFocus: false,
        enabled: !!token, // Only run the query if the token exists
    });

    // Use useQuery to fetch departments
    const { data: departmentsData, isLoading: isLoadingDepartments } = useQuery({
        queryKey: ["get-all-departments"],
        queryFn: getDepartments,
        staleTime: 60 * 10000, // Cache for 10 minute
    });

    useEffect(() => {
        dispatch(setDepartments(departmentsData?.data)); // Dispatch action to store departments
    }, [departmentsData])

    useEffect(() => {
        if (isLoading) return; // Wait for the query to load
        if (isError || !data?.data?.isValid) {
            dispatch(setAuth(false)); // Cập nhật isAuth trong store
            navigate('/login'); // Có lỗi khi kiểm tra token, chuyển hướng đến login
        } else {
            dispatch(setAuth(true)); // Cập nhật isAuth trong store
            dispatch(setUsername(data?.data.user?.username)); // Lưu tên người dùng vào store
            dispatch(setUserInfo(data?.data.user)); // Lưu người dùng vào store
            if (!['dashboard', 'devices'].includes(window.location.pathname.split('/')[1])) {
                navigate('/dashboard');
            }
            if(!data?.data?.user?.departmentIds || data?.data?.user?.departmentIds?.length <= 0) {
                message.warning("Bạn chưa được phân quyền vào phòng ban nào. Vui lòng liên hệ quản trị viên !")
            }
        }
    }, [isLoading, isError, data, dispatch, navigate]);

    return (
        <div className='w-screen h-screen'>
            {
                isAuth ? <MediHubMain /> : <Outlet />
            }
        </div>
    );
}

export default AuthRoutes;