import React, { useState } from 'react';
import { Button, Form, Input, Upload, Image, Row, Col } from 'antd';
import { useNavigate } from 'react-router-dom';
import DevicesDetailTopbar from './DevicesDetailTopbar';

const DevicesDetail = () => {
    let navigate = useNavigate();
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [fileList, setFileList] = useState<any[]>([]);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [previewOpen, setPreviewOpen] = useState<boolean>(false);

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

    return (
        <div className="medi-devices-detail">
            <DevicesDetailTopbar />
            <div className='devices-detail__content'>
                <Form
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
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
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
                            rules={[{ required: true, message: 'Vui lòng nhập tên hãng' }]}
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
                            rules={[{ required: true, message: 'Vui lòng nhập tên chức năng' }]}
                        >
                            <Input />
                        </Form.Item>
                        {/* Hợp đồng lắp đặt */}
                        <Form.Item
                            label='Hợp đồng lắp đặt'
                            name='installationContract'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Upload
                                fileList={fileList}
                                onChange={handleUploadChange}
                                beforeUpload={(file) => {
                                    const isPDF = file.type === 'application/pdf';
                                    const isImage = file.type.startsWith('image/');
                                    if (!isPDF && !isImage) {
                                        alert('Bạn chỉ có thể tải lên file PDF hoặc ảnh!');
                                    }
                                    return isPDF || isImage;
                                }}
                                multiple // Cho phép tải lên nhiều file
                            >
                                <Button>Chọn file</Button>
                            </Upload>
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
                        {/* Thông tin người quản lý */}
                        <Row gutter={16}>
                            <Col span={12}>
                                <Form.Item
                                    label='Người quản lý'
                                    name={['managerInfo', 'fullName']}
                                    labelCol={{ span: 12 }}
                                    wrapperCol={{ span: 12 }}
                                >
                                    <Input placeholder='Nguyễn Văn A' />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name={['managerInfo', 'phoneNumber']}
                                    labelCol={{ span: 12 }}
                                    wrapperCol={{ span: 12 }}
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
                                    labelCol={{ span: 12 }}
                                    wrapperCol={{ span: 12 }}
                                >
                                    <Input placeholder='Nguyễn Văn B' />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name={['engineerInfo', 'phoneNumber']}
                                    labelCol={{ span: 12 }}
                                    wrapperCol={{ span: 12 }}
                                >
                                    <Input placeholder='Số điện thoại' />
                                </Form.Item>
                            </Col>
                        </Row>
                    </div>
                    <div className='detail__content--right'>
                        <Form.Item
                            label='Ảnh đại diện'
                            name='deviceAvatar'
                            labelCol={{ span: 6 }}
                            wrapperCol={{ span: 18 }}
                        >
                            <Upload
                                action="https://660d2bd96ddfa2943b33731c.mockapi.io/api/upload"
                                listType="picture-card"
                                fileList={fileList}
                                onPreview={handlePreview}
                                onChange={handleUploadChange}
                                beforeUpload={(file) => {
                                    const isImage = file.type.startsWith('image/');
                                    if (!isImage) {
                                        alert('Bạn chỉ có thể tải lên ảnh!');
                                    }
                                    return isImage;
                                }}
                            >
                                {fileList.length >= 1 ? null : <Button>Chọn ảnh</Button>}
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
                        <div>
                            <div className='text-xl font-bold'>Lịch sử - Tình trạng hoạt động</div>
                            <>
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
                            </>
                        </div>
                    </div>
                </Form>
            </div>
        </div>
    );
}

export default DevicesDetail;