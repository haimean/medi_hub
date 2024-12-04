import React, { useState } from 'react';
import { Button, Input, message } from 'antd';
import { useNavigate } from 'react-router-dom'; // Import useNavigate để chuyển hướng
import { apiLogin } from '../api/appApi';

/**
 * Login page: check token, allow user entry user name, password
 * CreatedBy: PQ Huy (21.11.2024)
 */
const LoginPage = () => {
    const [username, setUsername] = useState(''); // State để lưu username
    const [password, setPassword] = useState(''); // State để lưu password
    const [usernameError, setUsernameError] = useState(false); // State để theo dõi lỗi username
    const [passwordError, setPasswordError] = useState(false); // State để theo dõi lỗi password
    const navigate = useNavigate(); // Khởi tạo useNavigate

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

        try {
            const response: any = await apiLogin(username, password);

            // Kiểm tra mã trạng thái và xử lý phản hồi từ API
            if (response?.succeeded && response?.message != "Unauthorized") {
                const token = response.data; // Giả sử token được trả về trong response
                localStorage.setItem('MEDI.Token', token); // Lưu token vào localStorage
                message.success('Đăng nhập thành công!');
                navigate('/dashboard'); // Chuyển hướng đến dashboard
            } else {
                message.error('Đăng nhập thất bại! Vui lòng kiểm tra lại thông tin.');
            }
        } catch (error) {
            message.error('Có lỗi xảy ra! Vui lòng thử lại.');
            console.error('Login error:', error);
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
                                setUsername(e.target.value);
                                setUsernameError(false); // Tắt lỗi khi người dùng nhập
                            }} // Cập nhật state khi người dùng nhập
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
                        />
                    </div>
                    <div className="modal--forgot-pass">
                        {/* Có thể thêm liên kết quên mật khẩu ở đây */}
                    </div>
                    <div className="modal--submit w-full">
                        <Button onClick={funcLogin} className='w-full modal--submit__btn' type="primary">Đăng Nhập</Button>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default LoginPage;