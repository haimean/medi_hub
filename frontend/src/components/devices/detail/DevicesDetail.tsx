import React, { useState } from 'react';
import { Button, Form, Input, Upload, Image, Row, Col, UploadProps, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import DevicesDetailTopbar from './DevicesDetailTopbar';
import { FileImageOutlined, UploadOutlined } from '@ant-design/icons';
import ActivityHistory from './activityHistory/ActivityHistory';

const DevicesDetail = () => {
    let navigate = useNavigate();    
    const [form] = Form.useForm(); // Khởi tạo form
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [fileList, setFileList] = useState<any[]>([]);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [previewOpen, setPreviewOpen] = useState<boolean>(false);

    const { Dragger } = Upload;

    const propsInstallationContract: UploadProps = {
        name: 'file',
        multiple: true,
        onChange(info) {
            const { status } = info.file;
        },
        beforeUpload(file, fileList) {
            return false;
        },
    };

    const onFinish = (values: any) => {
        console.log('Received values:', values);
        // Thực hiện lưu thông tin vào backend ở đây
    };

    const handleUploadChange = (info: any) => {
        if (info.file.status === 'done') {
            const reader = new FileReader();
            reader.onload = (e: any) => {
                setImageUrl(e.target.result as string);
            };
        }
        setFileList(info.fileList.slice(-1)); // Chỉ giữ lại file cuối cùng
    };

    const handlePreview = async (file: any) => {
        if (!file.url && !file.preview) {
            file.preview = await new Promise((resolve) => {
                const reader = new FileReader();
                reader.readAsDataURL(file.originFileObj);
                reader.onload = () => resolve(reader.result);
            });
        }
        setPreviewImage(file.url || file.preview);
        setPreviewOpen(true);
    };

    // Function to validate image file type
    const beforeUploadImage = (file: any) => {
        const isImage = file.type.startsWith('image/');
        if (!isImage) {
            message.error('Bạn chỉ có thể tải lên ảnh!');
        }
        return false; // Allow only image files
    };

    return (
        <div className="medi-devices-detail">
            <DevicesDetailTopbar form={form} onFinish={onFinish} />
            <div className='devices-detail__content'>
                <Form
                    form={form} // Gán form vào component
                    name="basic"
                    className='detail__content--form'
                    initialValues={{ remember: true }}
                    onFinish={onFinish}
                    onFinishFailed={() => { }}
                    autoComplete="off"
                >
                    <div className='detail__content--left'>
                        <Form.Item
                            label='Tên thiết bị'
                            name='deviceName'
                            rules={[{ required: true, message: 'Vui lòng nhập tên thiết bị' }]}
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Mã thiết bị'
                            name='deviceCode'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                            rules={[{ required: true, message: 'Vui lòng nhập mã thiết bị' }]}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Nước sản xuất'
                            name='manufacturerCountry'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Tên hãng'
                            name='manufacturerName'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                            rules={[{ required: true, message: 'Vui lòng nhập tên hãng' }]}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Năm sản xuất'
                            name='manufacturingYear'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                            rules={[{ required: true, message: 'Vui lòng nhập năm sản xuất' }]}
                        >
                            <Input type="number" />
                        </Form.Item>
                        <Form.Item
                            label='Số seri'
                            name='serialNumber'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Tên chức năng'
                            name='functionName'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                            rules={[{ required: true, message: 'Vui lòng nhập tên chức năng' }]}
                        >
                            <Input />
                        </Form.Item>
                        {/* Hợp đồng lắp đặt */}
                        <Form.Item
                            label='Hợp đồng lắp đặt'
                            name='installationContract'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Dragger {...propsInstallationContract}>
                                <UploadOutlined style={{ fontSize: '24px' }} />
                            </Dragger>
                        </Form.Item>
                        {/* Thời hạn hợp đồng */}
                        <Form.Item
                            label='Thời hạn hợp đồng'
                            name='contractDuration'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input type="date" />
                        </Form.Item>
                        <Form.Item
                            label='Tình trạng máy'
                            name='machineStatus'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Nguồn nhập'
                            name='importSource'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input />
                        </Form.Item>
                        <Form.Item
                            label='Ngày sử dụng'
                            name='usageDate'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                            rules={[{ required: true, message: 'Vui lòng nhập ngày sử dụng' }]}
                        >
                            <Input type="date" />
                        </Form.Item>
                        <Form.Item
                            label='Lab sử dụng'
                            name='labUsage'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input />
                        </Form.Item>
                        {/* Thông tin người quản lý */}
                        <Row gutter={16}>
                            <Col span={12}>
                                <Form.Item
                                    label='Người quản lý'
                                    name={['managerInfo', 'fullName']}
                                    labelCol={{ span: 12, prefixCls: 'left-item' }}
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Nguyễn Văn A' />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name={['managerInfo', 'phoneNumber']}
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Số điện thoại' />
                                </Form.Item>
                            </Col>
                        </Row>
                        {/* Thông tin kỹ sư */}
                        <Row gutter={16}>
                            <Col span={12}>
                                <Form.Item
                                    label='Kỹ sư'
                                    name={['engineerInfo', 'fullName']}
                                    labelCol={{ span: 12, prefixCls: 'left-item' }}
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Nguyễn Văn B' />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name={['engineerInfo', 'phoneNumber']}
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Số điện thoại' />
                                </Form.Item>
                            </Col>
                        </Row>
                    </div>
                    <div className='detail__content--right'>
                        <div className='text-xl font-bold' style={{ paddingBottom: '8px' }}>Ảnh đại diện</div>
                        <Form.Item
                            label=''
                            name='deviceAvatar'
                        >
                            <Upload
                                listType="picture-card"
                                fileList={fileList}
                                onPreview={handlePreview}
                                onChange={handleUploadChange}
                                className='right-avatar'
                                beforeUpload={beforeUploadImage} // Validate image file type
                            >
                                {fileList.length >= 1 ? null : <FileImageOutlined style={{ fontSize: '2rem' }} />}
                            </Upload>
                            {previewImage && (
                                <Image
                                    preview={{
                                        visible: previewOpen,
                                        onVisibleChange: (visible) => setPreviewOpen(visible),
                                        afterOpenChange: (visible) => !visible && setPreviewImage(''),
                                    }}
                                    src={previewImage}
                                    style={{ marginTop: 16, width: 200 }}
                                />
                            )}
                        </Form.Item>
                        <div className='text-xl font-bold' style={{ paddingBottom: '8px' }}>Lịch sử - Tình trạng hoạt động</div>
                        <Form.Item
                            label='Nhật ký bảo dưỡng'
                            name='maintenanceLog'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory 
                                label='Nhật ký bảo dưỡng'
                                key='maintenanceLog'
                                value={form?.getFieldValue('maintenanceLog')}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Biên bản bảo trì'
                            name='maintenanceReport'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory 
                                label='Biên bản bảo trì'
                                key='maintenanceReport'
                                value={form?.getFieldValue('maintenanceReport')}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Nội kiểm tra bảo trì'
                            name='internalMaintenanceCheck'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory 
                                label='Nội kiểm tra bảo trì'
                                key='internalMaintenanceCheck'
                                value={form?.getFieldValue('internalMaintenanceCheck')}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Lịch bảo dưỡng'
                            name='maintenanceSchedule'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <Input type="date" />
                        </Form.Item>
                        <Form.Item
                            label='Ghi chú'
                            name='notes'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <Input.TextArea />
                        </Form.Item>
                        {/* HDSD Thiết bị */}
                        <Form.Item
                            label='HDSD Thiết bị'
                            name='deviceUsageInstructions'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <Input.TextArea placeholder="Hướng dẫn sử dụng thiết bị" />
                        </Form.Item>
                        {/* HD sử lý sự cố thiết bị */}
                        <Form.Item
                            label='HD sử lý sự cố thiết bị'
                            name='deviceTroubleshootingInstructions'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <Input.TextArea placeholder="Hướng dẫn xử lý sự cố thiết bị" />
                        </Form.Item>
                    </div>
                </Form>
            </div>
        </div>
    );
}

export default DevicesDetail;