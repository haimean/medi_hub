import React, { useEffect, useRef, useState } from 'react';
import { Button, Form, Input, Upload, Image, Row, Col, UploadProps, message, DatePicker, Modal } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import DevicesDetailTopbar from './DevicesDetailTopbar';
import { DeleteOutlined, FileImageOutlined, UploadOutlined } from '@ant-design/icons';
import ActivityHistory from './activityHistory/ActivityHistory';
import { getDeviceById, getdoc, getdocs } from '../../../api/appApi';
import { useQuery } from '@tanstack/react-query';
import { setIsEditDevice } from '../../../stores/commonStore'; // Import setDepartments
import { useDispatch, useSelector } from 'react-redux';
import dayjs from 'dayjs';
import { getFileType } from '../../../function/commons';

const DevicesDetail = () => {
    let navigate = useNavigate();
    const { id } = useParams(); // Lấy ID từ URL
    const [form] = Form.useForm(); // Khởi tạo form
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [fileList, setFileList] = useState<any[]>([]);
    const [fileListContract, setFileListContract] = useState<any[]>([]);
    const [previewImage, setPreviewImage] = useState<string>('');
    const [previewOpen, setPreviewOpen] = useState<boolean>(false);
    const isEditDevice: boolean = useSelector((state: any) => state.isEditDevice); // Lấy trạng thái isEditDevice từ store
    const dispatch = useDispatch(); // Khởi tạo dispatch
    const { Dragger } = Upload;
    let hasFetchedDataRef = false;

    const propsInstallationContract = {
        name: 'file',
        multiple: true,
        onChange(info: any) {
            const { status } = info.file;
            if (status === 'done') {
                message.success(`${info.file.name} file uploaded successfully`);
            } else if (status === 'error') {
                message.error(`${info.file.name} file upload failed.`);
            }

            // Cập nhật danh sách file
            setFileListContract(prevFileList => {
                const updatedFileList = [...prevFileList, ...info.fileList];

                // Cập nhật giá trị của trường installationContract trong form
                form.setFieldValue('installationContract', updatedFileList.map(item => item.name));

                return updatedFileList;
            });

        },
        beforeUpload(file: any) {
            // Prevent automatic upload
            return false;
        },
        onRemove: (file: any) => {
            setFileListContract(prevFileList => prevFileList.filter(item => item.uid !== file.uid));
        },
    };

    const { data: deviceData, isLoading, isError } = useQuery({
        queryKey: [`device-detail-${id}`],
        queryFn: () => getDeviceById(id ? id : ''),
        refetchOnWindowFocus: false,
        enabled: !!id, // Only run the query if the id exists
        staleTime: 60 * 10000, // Cache for 10 minute
    });

    useEffect(() => {
        if (!isLoading && deviceData && !hasFetchedDataRef) {
            dispatch(setIsEditDevice(true));
            hasFetchedDataRef = true; // Đánh dấu là đã thực hiện

            // kiểm tra xem có avatar ko thì lấy file ảnh về
            if (deviceData?.data?.deviceAvatar && deviceData?.data?.deviceAvatar?.length > 0) {
                setFileList([]); // Cập nhật fileList với đối tượng hình ảnh

                // Call API to fetch documents
                getdoc(deviceData.data.deviceAvatar[0])
                    .then((response: any) => {
                        const imageObject = {
                            uid: '-1', // Hoặc một giá trị duy nhất khác
                            name: response?.Data, // Tên tệp
                            status: 'done', // Trạng thái
                            url: `data:image/jpeg;base64,${response?.FileDatas}`, // Đường dẫn hình ảnh
                        };

                        // Cập nhật danh sách tệp
                        setFileList([imageObject]); // Cập nhật fileList với đối tượng hình ảnh
                    })
                    .catch(error => {
                        console.error('Error fetching documents:', error);
                        message.error('Failed to fetch documents.');
                    });
            }

            if (deviceData?.data?.installationContract && deviceData?.data?.installationContract.length > 0) {
                // Call API to fetch documents
                setFileListContract([]);

                setTimeout(() => {
                    for (let index = 0; index < deviceData.data.installationContract.length; index++) {
                        const path = deviceData.data.installationContract[index];

                        getdoc(path)
                            .then((response: any) => {
                                const extension = getFileType(response?.Data);

                                const fileObject = {
                                    uid: path, // Sử dụng đường dẫn làm uid
                                    name: response?.Data || 'file', // Tên tệp từ phản hồi hoặc mặc định
                                    status: 'done', // Trạng thái
                                    url: `data:${extension};base64,${response?.FileDatas}`, // Đường dẫn tệp
                                };
                                setFileListContract(prev => [...prev, fileObject]);
                            })
                            .catch(error => {
                                console.error('Error fetching documents:', error);
                                message.error('Failed to fetch documents.');
                            });
                    }
                }, 150);
            }

            // Ánh xạ dữ liệu từ deviceData vào form
            form.setFieldsValue({
                deviceAvatar: deviceData?.data?.deviceAvatar,
                deviceName: deviceData?.data?.deviceName,
                deviceCode: deviceData?.data?.deviceCode,
                manufacturerCountry: deviceData?.data?.manufacturerCountry,
                manufacturerName: deviceData?.data?.manufacturerName,
                manufacturingYear: deviceData?.data?.manufacturingYear,
                serialNumber: deviceData?.data?.serialNumber,
                functionName: deviceData?.data?.functionName,
                installationContract: deviceData?.data?.installationContract,
                contractDuration: deviceData?.data?.contractDuration ? dayjs(deviceData?.data?.contractDuration) : null,
                machineStatus: deviceData?.data?.machineStatus,
                importSource: deviceData?.data?.importSource,
                usageDate: deviceData?.data?.usageDate ? dayjs(deviceData?.data?.usageDate) : null,
                labUsage: deviceData?.data?.labUsage,
                managerInfo: {
                    fullName: deviceData?.data?.managerInfo?.fullName,
                    phoneNumber: deviceData?.data?.managerInfo?.phoneNumber,
                },
                engineerInfo: {
                    fullName: deviceData?.data?.engineerInfo?.fullName,
                    phoneNumber: deviceData?.data?.engineerInfo?.phoneNumber,
                },
                deviceUsageInstructions: deviceData?.data?.deviceUsageInstructions,
                deviceTroubleshootingInstructions: deviceData?.data?.deviceTroubleshootingInstructions,
                maintenanceLog: deviceData?.data?.maintenanceLog,
                maintenanceReport: deviceData?.data?.maintenanceReport,
                internalMaintenanceCheck: deviceData?.data?.internalMaintenanceCheck,
                maintenanceSchedule: deviceData?.data?.maintenanceSchedule ? dayjs(deviceData?.data?.maintenanceSchedule) : null,
                notes: deviceData?.data?.notes,
            });

            if (deviceData?.data?.deviceAvatar) {
                setImageUrl(deviceData?.data?.deviceAvatar);
            }
        } else {
            if (!(id && deviceData?.data)) {
                dispatch(setIsEditDevice(false));
            }
        }
    }, [deviceData]);

    const onRemoveContract = (file: any) => {
        setFileListContract(prevFileList => {
            const updatedFileList = prevFileList.filter(item => item.uid !== file.uid);
            
            // Cập nhật giá trị của trường installationContract trong form
            form.setFieldValue('installationContract', updatedFileList.map(item => item.name)); // Hoặc item.uid nếu bạn muốn lưu uid
    
            return updatedFileList;
        });
    }

    /**
     * 
     * @param values 
     */
    const onFinish = (values: any) => {
        console.log('Received values:', values);
        // Thực hiện lưu thông tin vào backend ở đây
    };

    /**
     * 
     * @param info 
     */
    const handleUploadChange = (info: any) => {
        if (info.file.status === 'done') {
            const reader = new FileReader();
            reader.onload = (e: any) => {
                setImageUrl(e.target.result as string);
            };
        }
        setFileList(info.fileList.slice(-1)); // Chỉ giữ lại file cuối cùng
        form.setFieldValue('deviceAvatar', info.fileList?.length > 0 ? info.fileList.slice(-1)?.name : null);
    };

    /**
     * 
     * @param file 
     */
    const handlePreview = async (file: any) => {
        // Check if the file has a URL or create a data URL for local files
        const fileUrl = file.url || (file.originFileObj && URL.createObjectURL(file.originFileObj));

        // Kiểm tra loại tệp
        const fileExtension = file.name.split('.').pop().toLowerCase();
        const downloadableFileTypes = ['pdf', 'xls', 'xlsx', 'doc', 'docx'];

        // Nếu tệp là PDF, Excel hoặc Word, tạo liên kết tải về
        if (downloadableFileTypes.includes(fileExtension)) {
            // Nếu file là base64, tạo Blob và liên kết tải về
            if (file.url.startsWith('data:')) {
                const link = document.createElement('a');
                link.href = file.url; // Sử dụng URL base64
                link.download = file.name; // Đặt tên tệp khi tải về
                document.body.appendChild(link);
                link.click(); // Mô phỏng click để tải về
                document.body.removeChild(link); // Xóa liên kết sau khi tải về
            } else {
                window.open(fileUrl, '_blank'); // Mở tệp nếu không phải base64
            }
            return; // Kết thúc hàm sau khi mở tệp
        }

        switch (true) {
            case file?.url.startsWith('data:image/'):
                // Nếu file là base64 hình ảnh, sử dụng nó để xem trước
                setPreviewImage(file.url); // Cập nhật hình ảnh để xem trước
                setPreviewOpen(true); // Mở cửa sổ xem trước
                break;
            default:
                window.open(fileUrl, '_blank');
                break;
        }
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
            <DevicesDetailTopbar
                form={form}
                onFinish={onFinish}
                fileListContract={fileListContract}
                fileList={fileList}
            />
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
                            <Dragger {...propsInstallationContract} onPreview={handlePreview}>
                                <UploadOutlined style={{ fontSize: '24px' }} />
                                <p className="ant-upload-text">Tải lên</p>
                            </Dragger>
                            {fileListContract.length > 0 && (
                                <div style={{ marginTop: 16 }}>
                                    {fileListContract.map((file, index) => (
                                        <div key={`${file.uid}-${index}`}>
                                            <a
                                                onClick={() => handlePreview(file)}
                                                style={{ cursor: 'pointer', color: '#1890ff' }}
                                            >
                                                {file.name}
                                            </a>
                                            <Button 
                                                type="link" 
                                                danger 
                                                onClick={() => onRemoveContract(file)} // Gọi hàm xóa khi nhấn nút
                                            >
                                                <DeleteOutlined style={{color: 'red'}}/>
                                            </Button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </Form.Item>
                        {/* Thời hạn hợp đồng */}
                        <Form.Item
                            label='Thời hạn hợp đồng'
                            name='contractDuration'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
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
                            <DatePicker format={'DD/MM/YYYY'} />
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
                                keyForm='maintenanceLog'
                                valueForm={form?.getFieldValue('maintenanceLog')}
                                form={form}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Biên bản bảo trì'
                            name='maintenanceReport'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory
                                label='Biên bản bảo trì'
                                keyForm='maintenanceReport'
                                valueForm={form?.getFieldValue('maintenanceReport')}
                                form={form}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Nội kiểm tra bảo trì'
                            name='internalMaintenanceCheck'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory
                                label='Nội kiểm tra bảo trì'
                                keyForm='internalMaintenanceCheck'
                                valueForm={form?.getFieldValue('internalMaintenanceCheck')}
                                form={form}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Lịch bảo dưỡng'
                            name='maintenanceSchedule'
                            labelCol={{ span: 6, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
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