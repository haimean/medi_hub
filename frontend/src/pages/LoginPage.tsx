// src/pages/LoginPage.tsx
import React, { useState, useEffect } from 'react'; // Import useEffect
import { Button, Input, message } from 'antd';
import { useNavigate } from 'react-router-dom'; // Import useNavigate để chuyển hướng
import { apiLogin } from '../api/appApi';
import { useDispatch } from 'react-redux'; // Import useDispatch
import { setUsername } from '../stores/commonStore';

/**
 * Login page: check token, allow user entry user name, password
 * CreatedBy: PQ Huy (21.11.2024)
 */
const LoginPage = () => {
    const [username, setUsernameState] = useState(''); // State để lưu username
    const [password, setPassword] = useState(''); // State để lưu password
    const [usernameError, setUsernameError] = useState(false); // State để theo dõi lỗi username
    const [passwordError, setPasswordError] = useState(false); // State để theo dõi lỗi password
    const [loading, setLoading] = useState(false); // State để theo dõi trạng thái loading
    const navigate = useNavigate(); // Khởi tạo useNavigate
    const dispatch = useDispatch(); // Khởi tạo useDispatch

    // Load saved username and password from localStorage
    useEffect(() => {
        const savedUsername = localStorage.getItem('savedUsername');
        const savedPassword = localStorage.getItem('savedPassword');
        if (savedUsername) {
            setUsernameState(savedUsername);
        }
        if (savedPassword) {
            setPassword(savedPassword);
        }
    }, []);

    // Hàm gọi API để đăng nhập
    const funcLogin = async () => {
        // Kiểm tra xem username và password có được nhập hay không
        const hasError = !username || !password;

        if (hasError) {
            if (!username) setUsernameError(true); // Bật lỗi cho username
            if (!password) setPasswordError(true); // Bật lỗi cho password
            message.warning('Vui lòng nhập Số điện thoại/Email và Mật khẩu!'); // Cảnh báo nếu không nhập
            return; // Dừng hàm nếu không có thông tin
        }

        setLoading(true); // Bắt đầu trạng thái loading

        try {
            const response: any = await apiLogin(username, password);

            // Kiểm tra mã trạng thái và xử lý phản hồi từ API
            if (response?.succeeded && response?.message !== "Unauthorized") {
                const token = response.data; // Giả sử token được trả về trong response
                localStorage.setItem('MEDI.Token', token); // Lưu token vào localStorage
                localStorage.setItem('savedUsername', username); // Lưu username vào localStorage
                localStorage.setItem('savedPassword', password); // Lưu password vào localStorage
                dispatch(setUsername(username)); // Lưu tên người dùng vào store
                message.success('Đăng nhập thành công!');
                navigate('/dashboard'); // Chuyển hướng đến dashboard
            } else {
                message.error('Đăng nhập thất bại! Vui lòng kiểm tra lại thông tin.');
            }
        } catch (error) {
            message.error('Có lỗi xảy ra! Vui lòng thử lại.');
            console.error('Login error:', error);
        } finally {
            setLoading(false); // Kết thúc trạng thái loading
        }
    };

    // Hàm xử lý sự kiện nhấn phím
    const handleKeyPress = (event: React.KeyboardEvent<HTMLInputElement>) => {
        if (event.key === 'Enter') {
            funcLogin(); // Gọi hàm đăng nhập khi nhấn Enter
        }
    };

    return (
        <div className="medi-login w-full h-full">
            <div className="medi-login__content">
                <div className="content__lang">
                    <div className="lang--icon"></div>
                    <div className="lang--text">Tiếng Việt</div>
                </div>
                <div className="content__modal">
                    <div className="modal--medi-log"></div>
                    <div className="modal--app-name">
                        MEDICAL LAB
                    </div>
                    <div className="modal--user">
                        <Input
                            className={`h-full ${usernameError ? 'input-error' : ''}`} // Thêm class nếu có lỗi
                            placeholder="Số điện thoại/ Email"
                            value={username}
                            onChange={(e) => {
                                setUsernameState(e.target.value);
                                setUsernameError(false); // Tắt lỗi khi người dùng nhập
                            }} // Cập nhật state khi người dùng nhập
                            onKeyPress={handleKeyPress} // Thêm sự kiện onKeyPress
                        />
                    </div>
                    <div className="modal--pass">
                        <Input.Password
                            className={`h-full ${passwordError ? 'input-error' : ''}`} // Thêm class nếu có lỗi
                            placeholder="Mật khẩu"
                            value={password}
                            onChange={(e) => {
                                setPassword(e.target.value);
                                setPasswordError(false); // Tắt lỗi khi người dùng nhập
                            }} // Cập nhật state khi người dùng nhập
                            onKeyPress={handleKeyPress} // Thêm sự kiện onKeyPress
                        />
                    </div>
                    <div className="modal--forgot-pass">
                        {/* Có thể thêm liên kết quên mật khẩu ở đây */}
                    </div>
                    <div className="modal--submit w-full">
                        <Button 
                            onClick={funcLogin} 
                            className='w-full modal--submit__btn' 
                            type="primary" 
                            loading={loading} // Thêm thuộc tính loading
                        >
                            Đăng Nhập
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default LoginPage;