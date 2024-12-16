import React, { useMemo, useState } from 'react';
import { Button, Form, Tooltip } from 'antd';
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
                    <Button variant="dashed" className='btn-main-3' style={{ marginRight: '8px' }} onClick={() => navigate('/devices')}>Hủy bỏ</Button>
                    <Button className='btn-main-2' style={{ marginRight: '8px' }} >Lưu và thêm</Button>
                    <Button className='btn-main'>Lưu</Button>
                </div>
            </div>
            <div className='devices-detail__content'>
                <Form
                    name="basic"
                    style={{ width: '100%', height: '100%' }}
                    initialValues={{ remember: true }}
                    onFinish={() => {}}
                    onFinishFailed={() => {}}
                    autoComplete="off"
                >
                    <div className='detail__content--left'>

                    </div>
                    <div className='detail__content--right'>

                    </div>
                </Form>
            </div>
        </div>
    );
}

export default DevicesDetail;