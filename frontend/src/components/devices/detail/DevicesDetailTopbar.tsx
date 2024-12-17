import React, { useMemo, useState } from 'react';
import { Button } from 'antd';
import { useNavigate } from 'react-router-dom';

const DevicesDetailTopbar = () => {
    let navigate = useNavigate();

    return (
        <div className="devices-detail__topbar">
            <div className='detail__topbar--left'>
                <div className='text-xl	font-bold'>Lý lịch thiết bị</div>
            </div>
            <div className='detail__topbar--right'>
                <Button variant="dashed" className='btn-main-3' style={{ marginRight: '8px' }} onClick={() => navigate('/devices')}>Hủy bỏ</Button>
                <Button className='btn-main-2' style={{ marginRight: '8px' }} >Lưu và thêm</Button>
                <Button className='btn-main'>Lưu</Button>
            </div>
        </div>
    );
}

export default DevicesDetailTopbar;