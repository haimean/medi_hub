import React, { useEffect, useRef, useState } from 'react';
import { Button, Form, Input, Upload, Image, Row, Col, UploadProps, message, DatePicker, Modal, Select, Divider, Space, InputRef } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import DevicesDetailTopbar from './DevicesDetailTopbar';
import { DeleteOutlined, FileImageOutlined, PlusOutlined, UploadOutlined } from '@ant-design/icons';
import ActivityHistory from './activityHistory/ActivityHistory';
import { getDeviceById, getdoc, getdocs, getManufacturerBranch } from '../../../api/appApi';
import { useQuery } from '@tanstack/react-query';
import { setIsEditDevice } from '../../../stores/commonStore'; // Import setDepartments
import { useDispatch, useSelector } from 'react-redux';
import dayjs from 'dayjs';
import { getFileType } from '../../../function/commons';
import { v4 as uuidv4 } from 'uuid';

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

    const [nameFunc, setNameFunc] = useState('');
    const [lstFunc, setLstFunc] = useState<any>([]);

    const [nameManuFact, setNameManuFact] = useState('');
    const [lstManufacters, setLstManufacters] = useState([
        {
            value: 1,
            label: 'Roche',
            // avatar: 'roche-svg'
        },
        {
            value: 2,
            label: 'Beckman',
            // avatar: 'beckman-svg'
        },
        {
            value: 3,
            label: 'Abbott',
            // avatar: 'abbott-svg'
        },
        {
            value: 4,
            label: 'Tủ Mát'
        },
        {
            value: 5,
            label: 'Tủ Âm'
        },
        {
            value: 6,
            label: 'Lọc RO'
        },
        {
            value: 7,
            label: 'Hãng khác'
        }
    ]);

    const [deviceStatus, setDeviceStatus] = useState([
        {
            value: 0,
            label: 'Đang sử dụng'
        },
        {
            value: 1,
            label: 'Đang sửa chữa'
        },
        {
            value: 2,
            label: 'Chờ thẩm định'
        },
        {
            value: 3,
            label: 'Không sử dụng'
        }
    ]);

    const [machineStatus, setMachineStatus] = useState([
        {
            value: 0,
            label: 'Mới'
        },
        {
            value:1,
            label: 'Cũ'
        }
    ]);

    const propsInstallationContract = {
        name: 'file',
        multiple: false,
        maxCount: 1,
        onChange(info: any) {
            const { status } = info.file;
            if (status === 'done') {
                message.success(`${info.file.name} file uploaded successfully`);
            } else if (status === 'error') {
                message.error(`${info.file.name} file upload failed.`);
            }

            // Cập nhật danh sách file
            setFileListContract(prevFileList => {
                const updatedFileList = [...info.fileList];

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

    const { data: dataManufacturerBranch, isLoading: isLoadingManufacturerBranch, isError: isErrorManufacturerBranch } = useQuery({
        queryKey: [`get-manufacturer-branch`],
        queryFn: () => getManufacturerBranch(),
        refetchOnWindowFocus: false,
        staleTime: 60 * 10000, // Cache for 10 minute
    });

    useEffect(() => {
        if (!isLoadingManufacturerBranch && dataManufacturerBranch?.data) {
            setLstFunc(dataManufacturerBranch?.data?.function);
        }
    }, [dataManufacturerBranch])

    useEffect(() => {
        if (!isLoading && deviceData && !hasFetchedDataRef) {
            dispatch(setIsEditDevice(true));
            hasFetchedDataRef = true; // Đánh dấu là đã thực hiện

            // kiểm tra xem có avatar ko thì lấy file ảnh về
            // if (deviceData?.data?.deviceAvatar && deviceData?.data?.deviceAvatar?.length > 0) {
            //     setFileList([]); // Cập nhật fileList với đối tượng hình ảnh

            //     // Call API to fetch documents
            //     getdoc(deviceData.data.deviceAvatar[0])
            //         .then((response: any) => {
            //             const imageObject = {
            //                 uid: '-1', // Hoặc một giá trị duy nhất khác
            //                 name: response?.Data, // Tên tệp
            //                 status: 'done', // Trạng thái
            //                 url: `data:image/jpeg;base64,${response?.FileDatas}`, // Đường dẫn hình ảnh
            //             };

            //             // Cập nhật danh sách tệp
            //             setFileList([imageObject]); // Cập nhật fileList với đối tượng hình ảnh
            //         })
            //         .catch(error => {
            //             console.error('Error fetching documents:', error);
            //             message.error('Failed to fetch documents.');
            //         });
            // }

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
                                // console.error('Error fetching documents:', error);
                                // message.error('Failed to fetch documents.');
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
                expirationDate: deviceData?.data?.expirationDate ? dayjs(deviceData?.data?.expirationDate) : null,
                machineStatus: deviceData?.data?.machineStatus,
                deviceStatus : deviceData?.data?.deviceStatus,
                importSource: deviceData?.data?.importSource,
                usageDate: deviceData?.data?.usageDate ? dayjs(deviceData?.data?.usageDate) : null,
                labUsage: deviceData?.data?.labUsage,
                managerInfo: deviceData?.data?.managerInfo,
                status: deviceData?.data?.status,
                managerPhonenumber: deviceData?.data?.managerPhoneNumber,
                engineerInfo: deviceData?.data?.engineerInfo,
                engineerPhonenumber: deviceData?.data?.engineerPhoneNumber,
                deviceUsageInstructions: deviceData?.data?.deviceUsageInstructions,
                deviceTroubleshootingInstructions: deviceData?.data?.deviceTroubleshootingInstructions,
                maintenanceDate: deviceData?.data?.maintenanceDate ? dayjs(deviceData?.data?.maintenanceDate) : null,
                maintenanceNextDate: deviceData?.data?.maintenanceNextDate ? dayjs(deviceData?.data?.maintenanceNextDate) : null,
                calibrationDate: deviceData?.data?.calibrationDate ? dayjs(deviceData?.data?.calibrationDate) : null,
                calibrationNextDate: deviceData?.data?.calibrationNextDate ? dayjs(deviceData?.data?.calibrationNextDate) : null,
                replaceDate: deviceData?.data?.replaceDate ? dayjs(deviceData?.data?.replaceDate) : null,
                replaceNextDate: deviceData?.data?.replaceNextDate ? dayjs(deviceData?.data?.replaceNextDate) : null,
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

        const fileExtension = file.name.split('.').pop().toLowerCase();
        const downloadableFileTypes = ['pdf', 'xls', 'xlsx', 'doc', 'docx', 'jpg','png'];

        if (downloadableFileTypes.includes(fileExtension)) {
            if (file.url && file.url.startsWith('data:')) {
                const link = document.createElement('a');
                link.href = file.url;
                link.download = file.name;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                window.open(fileUrl, '_blank');
            }
            return;
        }

        switch (true) {
            case file?.url?.startsWith('data:image/') || file?.hasOwnProperty('originFileObj'):
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

    const inputRef = useRef<InputRef>(null);

    /**
     * 
     * @param event 
     * @param setFunc 
     */
    const onNameChange = (event: React.ChangeEvent<HTMLInputElement>, setFunc: any) => {
        setFunc(event.target.value);
    };

    /**
     * 
     * @param e 
     * @param setFunc 
     * @param valueFunc 
     * @param setName 
     * @param name 
     */
    const addItem = (e: React.MouseEvent<HTMLButtonElement | HTMLAnchorElement>, setFunc: any, valueFunc: any, setName: any, name: any) => {
        e.preventDefault();
        const newUuid = uuidv4(); // Tạo UUID mới
        setFunc([...valueFunc, name || `New item ${newUuid}`]);
        setName('');
        setTimeout(() => {
            inputRef.current?.focus();
        }, 0);
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
                            <Select
                                placeholder="Nhập tên hãng"
                                dropdownRender={(menu) => (
                                    <>
                                        {menu}
                                        <Divider style={{ margin: '8px 0' }} />
                                    </>
                                )}
                                options={lstManufacters?.map((item: any) => ({ label: item.label, value: item.value }))}
                            />
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
                            label='Tình trạng máy'
                            name='machineStatus'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Select
                                placeholder="Nhập tình trạng máy"
                                dropdownRender={(menu) => (
                                    <>
                                        {menu}
                                        <Divider style={{ margin: '8px 0' }} />
                                    </>
                                )}
                                options={machineStatus.map((item: any) => ({ label: item.label, value: item.value }))}
                            />
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
                            label='Tên chức năng'
                            name='functionName'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                            rules={[{ required: true, message: 'Vui lòng nhập tên chức năng' }]}
                        >
                            <Select
                                placeholder="Nhập tên chức năng"
                                dropdownRender={(menu) => (
                                    <>
                                        {menu}
                                        <Divider style={{ margin: '8px 0' }} />
                                        <Space style={{ padding: '0 8px 4px' }}>
                                            <Input
                                                placeholder="Nhập tên chức năng"
                                                ref={inputRef}
                                                value={nameFunc}
                                                onChange={(e) => onNameChange(e, setNameFunc)}
                                                onKeyDown={(e) => e.stopPropagation()}
                                            />
                                            <Button type="text" icon={<PlusOutlined />} onClick={(e: any) => addItem(e, setLstFunc, lstFunc, setNameFunc, nameFunc)}>
                                                Thêm mới
                                            </Button>
                                        </Space>
                                    </>
                                )}
                                options={lstFunc.map((item: any) => ({ label: item, value: item }))}
                            />
                        </Form.Item>
                        {/* Hợp đồng lắp đặt */}
                        <Form.Item
                            label='Hợp đồng - Pháp lý'
                            name='installationContract'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            {/* vì đây nhớ không nhầm là pdf nên đang chỉ accept mỗi pdf nhá : k phải thì xóa đi, phải thì các chỗ khác làm tương tự accept='xxxx' */}
                            <Dragger {...propsInstallationContract} onPreview={handlePreview} accept='application/pdf'>
                                <UploadOutlined style={{ fontSize: '24px' }} />
                            </Dragger>
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
                        {/* Thời hạn hợp đồng */}
                        <Form.Item
                            label='Ngày hết hạn sử dụng'
                            name='expirationDate'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
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
                                    name='managerInfo'
                                    labelCol={{ span: 12, prefixCls: 'left-item' }}
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Nguyễn Văn A' />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name='managerPhonenumber'
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
                                    name='engineerInfo'
                                    labelCol={{ span: 12, prefixCls: 'left-item' }}
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Nguyễn Văn B' />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name='engineerPhonenumber'
                                    wrapperCol={{ span: 15 }}
                                >
                                    <Input placeholder='Số điện thoại' />
                                </Form.Item>
                            </Col>
                        </Row>
                        <Form.Item
                            label='Ghi chú'
                            name='notes'
                            labelCol={{ span: 6, prefixCls: 'left-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Input.TextArea />
                        </Form.Item>
                    </div>
                    <div className='detail__content--right'>
                        <Form.Item
                            label='Ảnh đại diện'
                            name='deviceAvatar'
                            labelCol={{ span: 12, prefixCls: 'right-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Upload
                                listType="picture-card"
                                fileList={fileList}
                                onPreview={handlePreview}
                                onChange={handleUploadChange}
                                className='right-avatar'
                                accept='image/*'
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
                        {/* Hồ sơ thẩm định */}
                        <Form.Item
                            // Trường hợp muốn click vào label mà không thực hiện action gì thì viết như dưới nhé
                            label={<span onClick={e => e.preventDefault()}>Hồ sơ thẩm định</span>}
                            name='installationContract'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                            wrapperCol={{ span: 12 }}
                        >
                            <Dragger {...propsInstallationContract} onPreview={handlePreview}>
                                <UploadOutlined style={{ fontSize: '24px' }} />
                            </Dragger>
                        </Form.Item>
                        <Form.Item
                            label='Tình trạng hoạt động'
                            name='deviceStatus'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <Select
                                placeholder="Nhập tình trạng máy"
                                dropdownRender={(menu) => (
                                    <>
                                        {menu}
                                        <Divider style={{ margin: '8px 0' }} />
                                    </>
                                )}
                                options={deviceStatus.map((item: any) => ({ label: item.label, value: item.value }))}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Bảo trì, bảo dưỡng, sửa chữa'
                            name='maintenanceLog'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory
                                label='Bảo trì, bảo dưỡng, sửa chữa'
                                keyForm='maintenanceLog'
                                valueForm={form?.getFieldValue('maintenanceLog')}
                                form={form}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Nội kiểm sau bảo dưỡng'
                            name='maintenanceReport'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory
                                label='Nội kiểm sau bảo dưỡng'
                                keyForm='maintenanceReport'
                                valueForm={form?.getFieldValue('maintenanceReport')}
                                form={form}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Nhật kí sử dụng hàng ngày'
                            name='internalMaintenanceCheck'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <ActivityHistory
                                label='Nhật kí sử dụng hàng ngày'
                                keyForm='internalMaintenanceCheck'
                                valueForm={form?.getFieldValue('internalMaintenanceCheck')}
                                form={form}
                            />
                        </Form.Item>
                        <Form.Item
                            label='Ngày bảo trì, bảo dưỡng'
                            name='maintenanceDate'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
                        </Form.Item>
                        
                        <Form.Item
                            label='Ngày bảo trì, bảo dưỡng kế tiếp'
                            name='maintenanceNextDate'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
                        </Form.Item>
                        <Form.Item
                            label='Ngày hiệu chuẩn'
                            name='calibrationDate'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
                        </Form.Item>
                        <Form.Item
                            label='Ngày hiệu chuẩn kế tiếp'
                            name='calibrationNextDate'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
                        </Form.Item>
                        <Form.Item
                            label='Ngày thay thế'
                            name='replaceDate'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
                        </Form.Item>
                        <Form.Item
                            label='Ngày thay thế kế tiếp'
                            name='replaceNextDate'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <DatePicker format={'DD/MM/YYYY'} />
                        </Form.Item>
                        {/* HDSD Thiết bị */}
                        <Form.Item
                            label='HDSD Thiết bị'
                            name='deviceUsageInstructions'
                            labelCol={{ span: 8, prefixCls: 'right-item' }}
                        >
                            <Input.TextArea placeholder="Hướng dẫn sử dụng thiết bị" />
                        </Form.Item>
                    </div>
                </Form>
            </div>
        </div>
    );
}

export default DevicesDetail;