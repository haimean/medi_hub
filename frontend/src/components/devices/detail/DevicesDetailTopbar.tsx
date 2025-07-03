import React from 'react';
import { Button, message } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import { FormInstance } from 'antd/es/form';
import { createDevices, updatedDevices, uploadDoc, uploadDocs } from '../../../api/appApi'; // Import hàm createDevices và updateDevices
import { useSelector } from 'react-redux';

interface DevicesDetailTopbarProps {
    form: FormInstance; // Định nghĩa kiểu cho form
    onFinish: (values: any) => void; // Định nghĩa kiểu cho hàm onFinish
    fileListContract: any;
    fileList: any
}

const DevicesDetailTopbar: React.FC<DevicesDetailTopbarProps> = ({ form, onFinish, fileListContract, fileList }) => {
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
    const createAdjustedValues = (values: any, upload: any) => {

        return {
            name: values.deviceName,
            deviceAvatar: upload?.deviceAvatar?.length > 0 ? upload?.deviceAvatar : (values?.deviceAvatar ? values?.deviceAvatar : []), // Chỉ lấy tên file
            deviceCode: values?.deviceCode,
            deviceName: values?.deviceName,
            manufacturerCountry: values?.manufacturerCountry,
            manufacturerName: values?.manufacturerName,
            manufacturingYear: Number(values?.manufacturingYear), // Chuyển đổi sang số
            serialNumber: values?.serialNumber,
            functionName: values?.functionName,
            installationContract: values?.installationContract ? values?.installationContract : [],
            contractDuration: values?.contractDuration,
            machineStatus: values?.machineStatus,
            importSource: values?.importSource || "", // Nếu không có giá trị, có thể để trống
            usageDate: values?.usageDate,
            labUsage: values?.labUsage,
            managerInfo: values?.managerInfo,
            managerPhoneNumber: values?.managerPhoneNumber,
            engineerInfo: values?.engineerInfo,
            engineerPhoneNumber: values?.engineerPhoneNumber,
            deviceUsageInstructions: values.deviceUsageInstructions,
            deviceTroubleshootingInstructions: values.deviceTroubleshootingInstructions,
            maintenanceLog: values?.maintenanceLog, // Cần thêm thông tin nếu có
            maintenanceReport: values?.maintenanceReport, // Cần thêm thông tin nếu có
            internalMaintenanceCheck: values?.internalMaintenanceCheck, // Cần thêm thông tin nếu có
            maintenanceSchedule: values.maintenanceSchedule,
            notes: values.notes,
            departmentIds: [department?.id], // ID của phòng ban có thể được lấy từ một nguồn khác
        };
    };

    const createAdjustedValuesForCreate = (values: any, upload: any) => {

        return {
            DeviceEntity: [
                {
                    deviceAvatar: upload?.deviceAvatar?.length > 0 ? upload?.deviceAvatar : (values?.deviceAvatar ? values?.deviceAvatar : []), // List<string>
                    deviceCode: values?.deviceCode, // string
                    deviceName: values?.deviceName, // string
                    manufacturerCountry: values?.manufacturerCountry, // string
                    manufacturerName: Number(values?.manufacturerName || 0), // int
                    manufacturingYear: Number(values?.manufacturingYear || 0), // int
                    serialNumber: values?.serialNumber, // string
                    machineStatus: values?.machineStatus, // string
                    importSource: values?.importSource || "", // string
                    functionName: values?.functionName, // string
                    installationContract: values?.installationContract ? values?.installationContract : '', // string (adjust if needed)
                    usageDate: values?.usageDate, // DateTime
                    expirationDate: values?.expirationDate, // DateTime?
                    labUsage: values?.labUsage, // string
                    managerInfo: values?.managerInfo, // string
                    managerPhoneNumber: values?.managerPhoneNumber, // string
                    engineerInfo: values?.engineerInfo, // string
                    engineerPhoneNumber: values?.engineerPhoneNumber, // string
                    deviceUsageInstructions: values?.deviceUsageInstructions, // string
                    appraisalFile: values?.appraisalFile, // string
                    status: values?.status, // int
                    maintenanceDate: values?.maintenanceDate, // DateTime?
                    maintenanceNextDate: values?.maintenanceNextDate, // DateTime?
                    maintenanceSchedule: Number(values?.maintenanceSchedule || 0), // int
                    calibrationDate: values?.calibrationDate, // DateTime?
                    calibrationNextDate: values?.calibrationNextDate, // DateTime?
                    replaceDate: values?.replaceDate, // DateTime?
                    replaceNextDate: values?.replaceNextDate, // DateTime?
                    notes: values?.notes, // string
                }
            ],
            MaintenanceRecordEntity: (values?.maintenanceLog || []).map((record: any) => ({
                MaintaindDate: record.maintaindDate, // DateTime
                MaintenanceDate: record.maintenanceDate, // string
                FileLinks: record.fileLinks, // string
                DeviceID: record.deviceId, // Guid
                TypeOfMaintenance: record.typeOfMaintenance // int
            }))
        };
    };

    /**
     * 
     */
    const uploadFiles = async () => {
        let uploadContract: any = [];
        let uploadAvatar: any = [];

        if (fileListContract?.length > 0) {

            for (let index = 0; index < fileListContract.length; index++) {
                const file = fileListContract[index];

                if (file.originFileObj) {
                    const formData = new FormData();
                    formData.append('File', file.originFileObj);

                    await uploadDoc(`${department?.id}-device-contract`, formData).then((respon) => {
                        uploadContract?.push(respon?.data);
                    })
                }

            }
        }

        if (fileList?.length > 0) {
            for (let index = 0; index < fileList.length; index++) {
                const file = fileList[index];

                if (file.originFileObj) {
                    const formData = new FormData();
                    formData.append('File', file.originFileObj);

                    await uploadDoc(`${department?.id}-device-contract`, formData).then((respon) => {
                        uploadAvatar?.push(respon?.data);
                    })
                }
            }
        }

        return {
            'installationContract': uploadContract,
            'deviceAvatar': uploadAvatar
        }
    };

    /**
     * 
     */
    const handleSave = async () => {
        try {
            const values = await form.validateFields(); // Kiểm tra tính hợp lệ của form
            // lưu danh sách file contract trước
            let upload = await uploadFiles();

            let adjustedValues: any = createAdjustedValues(values, upload); // Tạo adjustedValues

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
                adjustedValues = createAdjustedValuesForCreate(values, upload); // Tạo adjustedValues cho create
                // Nếu không, gọi API createDevices để thêm mới
                await createDevices(adjustedValues).then((res: any) => {
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

            // lưu danh sách file contract trước
            let upload = await uploadFiles();

            let adjustedValues : any = createAdjustedValues(values, upload); // Tạo adjustedValues

            if (isEditDevice) {
                // Nếu đang ở chế độ sửa, gọi API updateDevices
                await updatedDevices(adjustedValues).then((res: any) => {
                    if (res?.succeeded) {
                        message.success('Cập nhật thành công');
                        onFinish(values); // Gọi hàm onFinish
                        form.resetFields(); // Làm sạch form để nhập bản ghi mới
                    } else {
                        message.error('Cập nhật thất bại');
                    }
                });
            } else {
                adjustedValues = createAdjustedValuesForCreate(values, upload); // Tạo adjustedValues cho create
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