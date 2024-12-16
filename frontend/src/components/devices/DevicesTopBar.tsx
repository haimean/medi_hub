import { Button, Input } from 'antd';
import React, { useState } from 'react';
import { ImportOutlined, PlusOutlined } from '@ant-design/icons';

const DevicesTopBar = () => {
    const { Search } = Input;

    const onSearch = (e: any) => {

    }

    return (
        <div className="devices__topbar">
            <div className='devices__topbar--left'></div>
            <div className='devices__topbar--right'>
                <Search className='right-btn-search' placeholder="Tìm kiếm theo mã, tên thiết bị, ..." onSearch={onSearch}/>
                <Button className='right-btn-add' icon={<PlusOutlined />}>Thêm</Button>
                <Button className='right-btn-excel' icon={<ImportOutlined />}>Nhập từ excel</Button>
            </div>
        </div>
    );
}

export default DevicesTopBar;