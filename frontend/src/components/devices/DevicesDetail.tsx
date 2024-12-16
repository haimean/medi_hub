import React, { useMemo, useState } from 'react';
import DevicesTopBar from './DevicesTopBar';
import { Button, Tooltip } from 'antd';
import { DeleteOutlined, EditOutlined, EyeOutlined, RollbackOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';

const DevicesDetail = () => {
    let navigate = useNavigate();
    
    return (
        <div className="medi-devices-detail">
            <div className="devices-detail__topbar">
                <div className='detail__topbar--left'>
                    <div className='text-xl	font-bold'>Lý lịch thiết bị</div>
                </div>
                <div className='detail__topbar--right'>
                    <Button variant="dashed" style={{marginRight: '8px'}} onClick={() => navigate('/devices')}>Hủy bỏ</Button>
                    <Button className='btn-main-2' style={{marginRight: '8px'}} >Lưu và thêm</Button>
                    <Button className='btn-main'>Lưu</Button>
                </div>
            </div>
            <div className='devices-detail__content'>
               
            </div>
        </div>
    );
}

export default DevicesDetail;