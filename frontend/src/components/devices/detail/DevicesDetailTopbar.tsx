import React from 'react';
import { Button, message } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import { FormInstance } from 'antd/es/form';
import { createDevices, updatedDevices, uploadDoc, uploadDocs } from '../../../api/appApi'; // Import hàm createDevices và updateDevices
import { useSelector } from 'react-redux';
import { error } from 'console';
import axiosClient from '../../../api/axiosClient';

interface DevicesDetailTopbarProps {
    form: FormInstance; // Định nghĩa kiểu cho form
    onFinish: (values: any) => void; // Định nghĩa kiểu cho hàm onFinish
    fileListContract: any;
}

const DevicesDetailTopbar: React.FC<DevicesDetailTopbarProps> = ({ form, onFinish, fileListContract }) => {
    let navigate = useNavigate();
    const department = useSelector((state: any) => state.department); // Lấy department từ store
    const isEditDevice: boolean = useSelector((state: any) => state.isEditDevice); // Lấy trạng thái isEditDevice từ store
    const { id } = useParams(); // Lấy ID từ URL

    const handleCancel = () => {
        navigate(-1); // Quay lại trang trước đó
    };

    /**
     * Hàm tạo adjustedValues
     * @param values 
     * @returns 
     * CreatedBy: PQ Huy (19.01.2025)
     */
    const createAdjustedValues = (values: any) => {
        return {
            name: values.deviceName,
            deviceAvatar: values?.deviceAvatar ? [values.deviceAvatar[0].name] : [], // Chỉ lấy tên file
            deviceCode: values?.deviceCode,
            deviceName: values?.deviceName,
            manufacturerCountry: values?.manufacturerCountry,
            manufacturerName: values?.manufacturerName,
            manufacturingYear: Number(values?.manufacturingYear), // Chuyển đổi sang số
            serialNumber: values?.serialNumber,
            functionName: values?.functionName,
            installationContract: values?.installationContract?.fileList?.map((file: any) => file?.name), // Chỉ lấy tên file
            contractDuration: values?.contractDuration,
            machineStatus: values?.machineStatus,
            importSource: values?.importSource || "", // Nếu không có giá trị, có thể để trống
            usageDate: values?.usageDate,
            labUsage: values?.labUsage,
            managerInfo: {
                fullName: values?.managerInfo?.fullName,
                dateOfBirth: new Date().toISOString(), // Thay thế bằng giá trị thực tế nếu có
                phoneNumber: values.managerInfo.phoneNumber,
                address: values.managerInfo.address || "", // Nếu không có giá trị, có thể để trống
            },
            engineerInfo: {
                fullName: values.engineerInfo.fullName,
                dateOfBirth: new Date().toISOString(), // Thay thế bằng giá trị thực tế nếu có
                phoneNumber: values.engineerInfo.phoneNumber,
                address: values.engineerInfo.address || "", // Nếu không có giá trị, có thể để trống
            },
            deviceUsageInstructions: values.deviceUsageInstructions,
            deviceTroubleshootingInstructions: values.deviceTroubleshootingInstructions,
            maintenanceLog: [], // Cần thêm thông tin nếu có
            maintenanceReport: [], // Cần thêm thông tin nếu có
            internalMaintenanceCheck: [], // Cần thêm thông tin nếu có
            maintenanceSchedule: values.maintenanceSchedule,
            notes: values.notes,
            departmentIds: [department?.id], // ID của phòng ban có thể được lấy từ một nguồn khác
        };
    };

    /**
     * 
     */
    const uploadFiles = async () => {
        if (fileListContract?.length > 0) {
            let uploadContract: any = [];

            fileListContract.forEach(async (file: any) => {

                // Kiểm tra xem originFileObj có tồn tại và có thuộc tính 'file' không

                if (file.originFileObj) {
                    const formData = new FormData();
                    formData.append('File', file.originFileObj);

                    await uploadDoc(`${department?.id}-device-contract`, formData).then((respon) => {
                        uploadContract?.push(respon?.data);
                    }).catch((error) => {
                        console.log(`Upload file false`, file.originFileObj);
                    })
                }
            });
        }
    };

    /**
     * 
     */
    const handleSave = async () => {
        try {
            const values = await form.validateFields(); // Kiểm tra tính hợp lệ của form
            let adjustedValues: any = createAdjustedValues(values); // Tạo adjustedValues

            // lưu danh sách file contract trước
            await uploadFiles();

            if (isEditDevice) {
                // Nếu đang ở chế độ sửa, gọi API updateDevices
                adjustedValues.Id = id;
                await updatedDevices([adjustedValues]).then((res: any) => {
                    if (res?.succeeded) {
                        message.success('Cập nhật thành công');
                        onFinish(values); // Gọi hàm onFinish
                    } else {
                        message.error('Cập nhật thất bại');
                    }
                });
            } else {
                // Nếu không, gọi API createDevices để thêm mới
                await createDevices([adjustedValues]).then((res: any) => {
                    if (res?.succeeded) {
                        message.success('Lưu thành công');
                        onFinish(values); // Gọi hàm onFinish
                    } else {
                        message.error('Lưu thất bại');
                    }
                });
            }
        } catch (info) {
            console.log('Validate Failed:', info);
        }
    };

    /**
     * 
     */
    const handleAdd = async () => {
        try {
            const values = await form.validateFields(); // Kiểm tra tính hợp lệ của form
            const adjustedValues = createAdjustedValues(values); // Tạo adjustedValues

            if (isEditDevice) {
                // Nếu đang ở chế độ sửa, gọi API updateDevices
                await updatedDevices([adjustedValues]).then((res: any) => {
                    if (res?.succeeded) {
                        message.success('Cập nhật thành công');
                        onFinish(values); // Gọi hàm onFinish
                        form.resetFields(); // Làm sạch form để nhập bản ghi mới
                    } else {
                        message.error('Cập nhật thất bại');
                    }
                });
            } else {
                // Nếu không, gọi API createDevices để thêm mới
                await createDevices([adjustedValues]).then((res: any) => {
                    if (res?.succeeded) {
                        message.success('Lưu thành công');
                        onFinish(values); // Gọi hàm onFinish
                        form.resetFields(); // Làm sạch form để nhập bản ghi mới
                    } else {
                        message.error('Lưu thất bại');
                    }
                });
            }
        } catch (info) {
            console.log('Validate Failed:', info);
        }
    };

    return (
        <div className="devices-detail__topbar">
            <div className='detail__topbar--left'>
                <div className='text-xl font-bold'>Lý lịch thiết bị</div>
            </div>
            <div className='detail__topbar--right'>
                <Button variant="dashed" className='btn-main-3' style={{ marginRight: '8px' }} onClick={handleCancel}>Hủy</Button>
                <Button className='btn-main-2' style={{ marginRight: '8px' }} onClick={handleAdd}>Lưu và thêm</Button>
                <Button className='btn-main' onClick={handleSave}>Lưu</Button>
            </div>
        </div>
    );
}

export default DevicesDetailTopbar;