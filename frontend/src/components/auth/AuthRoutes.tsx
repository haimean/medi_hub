import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import MediHubMain from '../../pages/MediHubMain';

/**
 * Component check auth and return outlet if has auth
 * @returns 
 * CreatedBy: PQ Huy (21.11.2024)
 */
const AuthRoutes = () => {
    let navigate = useNavigate();
    const [isAuth, setIsAuth] = useState<boolean>(true);

    // check has token or not
    useEffect(() => {
        if (localStorage.getItem('MEDI.Token')) {
            navigate('/dashboard')
        }
        else {
            navigate('/login')
        }
    }, []);

    return (
        <div className='w-screen h-screen'>
            {
                isAuth ? <MediHubMain /> : <div> You do not have permission ! </div>
            }
        </div>
    )
}

export default AuthRoutes;