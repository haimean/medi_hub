import React, { useState } from 'react';
import { Button, Form, Input, Checkbox, Upload, Image } from 'antd';
import { useNavigate } from 'react-router-dom';
import DevicesDetailTopbar from './DevicesDetailTopbar';

const DevicesDetail = () => {
    let navigate = useNavigate();
    const [imageUrl, setImageUrl] = useState<string | null>(null);

    const onFinish = (values: any) => {
        console.log('Received values:', values);
        // Thực hiện lưu thông tin vào backend ở đây
    };

    const handleUploadChange = (info: any) => {
        if (info.file.status === 'done') {
            // Lấy URL của ảnh đã chọn
            const reader = new FileReader();
            reader.onload = (e: any) => {
                setImageUrl(e.target.result as string);
            };
            reader.readAsDataURL(info.file.originFileObj);
        }
    };

    return (
        <div className="medi-devices-detail">
            <DevicesDetailTopbar />
            <div className='devices-detail__content'>
                <Form
                    name="basic"
                    className='detail__content--form'
                    initialValues={{ remember: true }}
                    onFinish={onFinish}
                    onFinishFailed={() => {}}
                    autoComplete="off"
                >
                    <div className='detail__content--left'>
                        <Form.Item
                            label='Tên thiết bị'
                            name='deviceName'
                            labelCol={{ span: 6 }} // Căn chỉnh nhãn
                            wrapperCol={{ span: 18 }} // Căn chỉnh ô input
                            rules={[{ required: true, message: 'Vui lòng nhập tên thiết bị' }]}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Mã thiết bị'
                            name='deviceCode'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                            rules={[{ required: true, message: 'Vui lòng nhập mã thiết bị' }]}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Nước sản xuất'
                            name='manufacturerCountry'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Tên hãng'
                            name='manufacturerName'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                            rules={[{ required: true, message: 'Vui lòng nhập tên hãng' }]} // Bắt buộc nhập
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Năm sản xuất'
                            name='manufacturingYear'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                            rules={[{ required: true, message: 'Vui lòng nhập năm sản xuất' }]}
                        >
                            <Input type="number" />
                        </Form.Item>
                        <Form.Item
                            label='Số seri'
                            name='serialNumber'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Tên chức năng'
                            name='functionName'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                            rules={[{ required: true, message: 'Vui lòng nhập tên chức năng' }]} // Bắt buộc nhập
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Tình trạng máy'
                            name='machineStatus'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Nguồn nhập'
                            name='importSource'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Ngày sử dụng'
                            name='usageDate'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                            rules={[{ required: true, message: 'Vui lòng nhập ngày sử dụng' }]}
                        >
                            <Input type="date" />
                        </Form.Item>
                        <Form.Item
                            label='Lab sử dụng'
                            name='labUsage'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input />
                        </Form.Item>
                    </div>
                    <div className='detail__content--right'>
                        <Form.Item
                            label='Ảnh đại diện'
                            name='deviceAvatar'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Upload
                                showUploadList={false}
                                onChange={handleUploadChange}
                            >
                                <Button>Chọn ảnh</Button>
                            </Upload>
                            {imageUrl && (
                                <div style={{ marginTop: 16 }}>
                                    <Image
                                        width={200}
                                        src={imageUrl}
                                        alt="Ảnh đại diện"
                                        style={{ border: '1px solid #d9d9d9', borderRadius: '4px' }}
                                    />
                                </div>
                            )}
                        </Form.Item>
                        <Form.Item
                            label='Nhật ký bảo dưỡng'
                            name='maintenanceLog'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input.TextArea placeholder="Nhập nhật ký bảo dưỡng" />
                        </Form.Item>
                        <Form.Item
                            label='Biên bản bảo trì'
                            name='maintenanceReport'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input.TextArea placeholder="Nhập biên bản bảo trì" />
                        </Form.Item>
                        <Form.Item
                            label='Nội kiểm tra bảo trì'
                            name='internalMaintenanceCheck'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input.TextArea placeholder="Nhập nội kiểm tra bảo trì" />
                        </Form.Item>
                        <Form.Item
                            label='Lịch bảo dưỡng'
                            name='maintenanceSchedule'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input placeholder="Nhập lịch bảo dưỡng" />
                        </Form.Item>
                        <Form.Item
                            label='Ghi chú'
                            name='notes'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Input.TextArea />
                        </Form.Item>
                    </div>
                </Form>
            </div>
        </div>
    );
}

export default DevicesDetail;