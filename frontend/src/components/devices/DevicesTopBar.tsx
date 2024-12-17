import { Button, Input } from 'antd';
import React, { useState } from 'react';
import { ImportOutlined, PlusOutlined, RollbackOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';

const DevicesTopBar = () => {
    let navigate = useNavigate();
    const { Search } = Input;

    const onSearch = (e: any) => {

    }

    return (
        <div className="devices__topbar">
            <div className='devices__topbar--left'>
                <Button icon={<RollbackOutlined />} onClick={() => navigate('/dashboard')}>Quay về</Button>
            </div>
            <div className='devices__topbar--right'>
                <Search className='right-btn-search' placeholder="Tìm kiếm theo mã, tên thiết bị, ..." onSearch={onSearch}/>
                <Button className='btn-main' style={{ marginRight: '8px' }} onClick={() => navigate('detail')} icon={<PlusOutlined />}>Thêm</Button>
                <Button className='btn-main-2' icon={<ImportOutlined />}>Nhập từ excel</Button>
            </div>
        </div>
    );
}

export default DevicesTopBar;