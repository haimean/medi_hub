import React, { useState } from 'react';
import { CalendarOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons'; // Thêm PlusOutlined
import { Modal, Button, DatePicker, List, Upload, Image } from 'antd';
import dayjs from 'dayjs';

const ActivityHistory = ({ label, value }: any) => {
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [isImageModalVisible, setIsImageModalVisible] = useState(false);
    const [selectedDateRange, setSelectedDateRange] = useState<any>(null);
    const [activities, setActivities] = useState([
        { id: 1, monthYear: '12/2024', images: ['image1.jpg', 'image2.jpg'] },
        { id: 2, monthYear: '11/2024', images: ['image3.jpg'] },
    ]);
    const [currentImages, setCurrentImages] = useState<string[]>([]);
    const [fileList, setFileList] = useState<any[]>([]); // Danh sách file đã tải lên
    const [previewImage, setPreviewImage] = useState<string>('');
    const [previewOpen, setPreviewOpen] = useState<boolean>(false);

    const showModal = () => {
        setIsModalVisible(true);
    };

    const handleOk = () => {
        setIsModalVisible(false);
    };

    const handleCancel = () => {
        setIsModalVisible(false);
    };

    const onDateChange = (dates: any) => {
        setSelectedDateRange(dates);
    };

    const handleDelete = (id: number) => {
        setActivities(activities.filter(activity => activity.id !== id));
    };

    const showImageModal = (images: string[]) => {
        setCurrentImages(images);
        setFileList(images.map(image => ({ uid: image, name: image, status: 'done', url: image }))); // Chuyển đổi hình ảnh thành định dạng fileList
        setIsImageModalVisible(true);
    };

    const handleImageModalOk = () => {
        setIsImageModalVisible(false);
    };

    const handleImageModalCancel = () => {
        setIsImageModalVisible(false);
        setFileList([]); // Xóa danh sách file khi đóng modal
    };

    const handleChange = ({ fileList: newFileList }: any) => {
        setFileList(newFileList);
    };

    const handlePreview = async (file: any) => {
        if (!file.url && !file.preview) {
            file.preview = await new Promise(resolve => {
                const reader = new FileReader();
                reader.readAsDataURL(file.originFileObj);
                reader.onload = () => resolve(reader.result);
            });
        }
        setPreviewImage(file.url || file.preview);
        setPreviewOpen(true);
    };

    return (
        <div className='activity-history'>
            <CalendarOutlined className='history__calendar-icon' onClick={showModal} />
            <Modal
                title={label}
                visible={isModalVisible}
                onOk={handleOk}
                onCancel={handleCancel}
                footer={[
                    <Button key="back" onClick={handleCancel}>
                        Hủy {/* Thay đổi chữ Cancel thành Hủy */}
                    </Button>,
                    <Button className='btn-main' key="submit" onClick={handleOk} icon={<PlusOutlined />}>
                        Thêm mới {/* Thay đổi chữ OK thành Thêm mới và thêm icon + */}
                    </Button>,
                ]}
            >
                <div style={{ marginBottom: '16px' }}>
                    <span style={{ fontWeight: 'bold', marginRight: '8px' }}>Bộ Lọc:</span>
                    <DatePicker.RangePicker
                        onChange={onDateChange}
                        format="YYYY-MM"
                        picker="month"
                        placeholder={['Bắt đầu', 'Kết thúc']}
                    />
                </div>
                <List
                    dataSource={activities}
                    renderItem={item => (
                        <List.Item
                            actions={[
                                <span 
                                    onClick={() => showImageModal(item.images)} 
                                    style={{ color: '#0073E6', cursor: 'pointer', marginRight: '8px' }}
                                >
                                    Xem ảnh ({item.images.length})
                                </span>,
                                <DeleteOutlined 
                                    onClick={() => handleDelete(item.id)} 
                                    style={{ color: 'red' }} 
                                />
                            ]}
                            style={{ borderBottom: '1px solid #f0f0f0' }}
                        >
                            <List.Item.Meta
                                title={item.monthYear}
                            />
                        </List.Item>
                    )}
                />
            </Modal>

            {/* Modal hiển thị ảnh */}
            <Modal
                visible={isImageModalVisible}
                onOk={handleImageModalOk}
                onCancel={handleImageModalCancel}
                footer={[
                    <Button key="back" onClick={handleImageModalCancel}>
                        Hủy {/* Thay đổi chữ Cancel thành Hủy */}
                    </Button>,
                    <Button className='btn-main' key="submit" onClick={handleImageModalOk}>
                        Lưu {/* Thay đổi chữ OK thành Thêm mới và thêm icon + */}
                    </Button>,
                ]}
            >
                <Upload
                    action="https://660d2bd96ddfa2943b33731c.mockapi.io/api/upload"
                    listType="picture-card"
                    fileList={fileList}
                    onPreview={handlePreview}
                    onChange={handleChange}
                >
                    {fileList.length >= 8 ? null : <div>+ Upload</div>}
                </Upload>
                {previewImage && (
                    <Image
                        wrapperStyle={{ display: 'none' }}
                        preview={{
                            visible: previewOpen,
                            onVisibleChange: (visible) => setPreviewOpen(visible),
                            afterOpenChange: (visible) => !visible && setPreviewImage(''),
                        }}
                        src={previewImage}
                    />
                )}
            </Modal>
        </div>
    );
};

export default ActivityHistory;