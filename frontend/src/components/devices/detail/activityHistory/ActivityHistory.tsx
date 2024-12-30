import React, { useState } from 'react';
import { CalendarOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons'; // Thêm PlusOutlined
import { Modal, Button, DatePicker, List, Upload, Image, message } from 'antd';
import dayjs from 'dayjs';

/**
 * CreatedBy: PQ Huy (25.12.2024)
 */
const ActivityHistory = ({ label, value }: any) => {
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [isImageModalVisible, setIsImageModalVisible] = useState(false);
    const [selectedDateRange, setSelectedDateRange] = useState<any>(null);
    const [activities, setActivities] = useState<any>([]);
    const [currentImages, setCurrentImages] = useState<string[]>([]);
    const [fileList, setFileList] = useState<any[]>([]); // Danh sách file đã tải lên
    const [previewImage, setPreviewImage] = useState<string>('');
    const [previewOpen, setPreviewOpen] = useState<boolean>(false);
    const [monthYear, setMonthYear] = useState<string>(''); // State to store month/year input

    /**
     * Hiển thị modal cho hoạt động
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const showModal = () => {
        setIsModalVisible(true);
    };

    /**
     * Xử lý khi nhấn nút OK trong modal
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const handleOk = () => {
        setIsModalVisible(false);
    };

    /**
     * Xử lý khi nhấn nút Hủy trong modal
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const handleCancel = () => {
        setIsModalVisible(false);
    };

    /**
     * Xử lý thay đổi ngày trong DatePicker
     * @param dates - khoảng thời gian đã chọn
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const onDateChange = (dates: any) => {
        setSelectedDateRange(dates);
    };

    /**
     * Xóa hoạt động theo ID
     * @param id - ID của hoạt động cần xóa
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const handleDelete = (id: number) => {
        setActivities(activities.filter((activity: any) => activity.id !== id));
    };

    /**
     * Hiển thị modal cho hình ảnh
     * @param images - danh sách hình ảnh
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const showImageModalEdit = (images: string[]) => {
        setCurrentImages(images);
        setFileList(images.map(image => ({ uid: image, name: image, status: 'done', url: image }))); // Chuyển đổi hình ảnh thành định dạng fileList
        setIsImageModalVisible(true);
    };

    /**
     * Hiển thị modal cho hình ảnh thêm mới
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const showImageModalAdd = () => {
        setCurrentImages([]);
        setFileList([]); // Chuyển đổi hình ảnh thành định dạng fileList
        setIsImageModalVisible(true);
    };

    /**
     * Xử lý khi nhấn nút Hủy trong modal hình ảnh
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const handleImageModalCancel = () => {
        setIsImageModalVisible(false);
        setFileList([]); // Xóa danh sách file khi đóng modal
    };

    /**
     * Xử lý thay đổi danh sách file
     * @param newFileList - danh sách file mới
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const handleChange = ({ fileList: newFileList }: any) => {
        setFileList(newFileList);
    };

    /**
     * Kiểm tra loại file trước khi tải lên
     * @param file - file cần kiểm tra
     * @returns {boolean} - true nếu là ảnh, false nếu không
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const beforeUploadImage = (file: any) => {
        const isImage = file.type.startsWith('image/');
        if (!isImage) {
            message.error('Bạn chỉ có thể tải lên ảnh!');
        }
        return false; // Allow only image files
    };

    /**
     * Xử lý xem trước hình ảnh
     * @param file - file hình ảnh
     * CreatedBy: PQ Huy (25.12.2024)
     */
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

    /**
     * Xử lý khi nhấn nút OK trong modal hình ảnh
     * CreatedBy: PQ Huy (25.12.2024)
     */
    const handleImageModalOk = () => {
        if (!monthYear) {
            message.error('Vui lòng nhập tháng/năm trước khi lưu!');
            return;
        }

        // Create folder and save img automatically

        // save to setActivities


        setIsImageModalVisible(false);
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
                        Hủy
                    </Button>,
                    <Button className='btn-main' key="submit" onClick={showImageModalAdd} icon={<PlusOutlined />}>
                        Thêm mới
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
                    renderItem={(item: any) => (
                        <List.Item
                            actions={[
                                <span 
                                    onClick={() => showImageModalEdit(item.images)} 
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
                        Lưu 
                    </Button>
                ]}
            >
                <div className='pb-2.5'>
                    <span className='pr-2'>Bảo dưỡng:</span>
                    <DatePicker 
                        picker="month" 
                        placeholder='Chọn tháng/năm'
                        onChange={(date) => setMonthYear(dayjs(date).format('MM-YYYY'))} // Set month/year
                    />
                </div>
                <Upload
                    listType="picture-card"
                    fileList={fileList}
                    onPreview={handlePreview}
                    onChange={handleChange}
                    beforeUpload={beforeUploadImage}
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