import React from "react";
import { Button, message } from "antd";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { FormInstance } from "antd/es/form";
import {
  createDevices,
  updatedDevices,
  uploadDoc,
  uploadDocs,
} from "../../../api/appApi"; // Import hàm createDevices và updateDevices
import { useSelector } from "react-redux";

interface DevicesDetailTopbarProps {
  form: FormInstance; // Định nghĩa kiểu cho form
  onFinish: (values: any) => void; // Định nghĩa kiểu cho hàm onFinish
  deviceAvatarFile: any;
  deviceUsageInstructionsFile: any;
  appraisalFile: any;
  installationContractFile: any;
}

const DevicesDetailTopbar: React.FC<DevicesDetailTopbarProps> = ({
  form,
  onFinish,
  deviceAvatarFile,
  deviceUsageInstructionsFile,
  appraisalFile,
  installationContractFile,
}) => {
  let navigate = useNavigate();
  const department = useSelector((state: any) => state.department); // Lấy department từ store
  const isEditDevice: boolean = useSelector((state: any) => state.isEditDevice); // Lấy trạng thái isEditDevice từ store
  const { id } = useParams(); // Lấy ID từ URL
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode") || "edit";

  const handleCancel = () => {
    navigate(-1); // Quay lại trang trước đó
  };

  const createAdjustedValuesForCreate = (values: any, upload: any) => {
    return {
      DeviceEntity: {
        deviceAvatar: upload?.deviceAvatar
          ? upload?.deviceAvatar
          : values?.deviceAvatar,

        deviceCode: values?.deviceCode, // string
        deviceName: values?.deviceName, // string
        manufacturerCountry: values?.manufacturerCountry, // string
        manufacturerName: Number(values?.manufacturerName || 0), // int
        manufacturingYear: Number(values?.manufacturingYear || 0), // int
        serialNumber: values?.serialNumber, // string
        machineStatus: values?.machineStatus, // string
        importSource: values?.importSource || "", // string
        functionName: values?.functionName, // string
        installationContract: values?.installationContract
          ? values?.installationContract
          : "", // string (adjust if needed)
        usageDate: values?.usageDate, // DateTime
        expirationDate: values?.expirationDate, // DateTime?
        labUsage: values?.labUsage, // string
        managerInfo: values?.managerInfo, // string
        managerPhoneNumber: values?.managerPhonenumber, // string
        engineerInfo: values?.engineerInfo, // string
        engineerPhoneNumber: values?.engineerPhonenumber, // string
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
      },

      MaintenanceRecordEntity: (values?.maintenanceLog || []).map(
        (record: any) => ({
          MaintaindDate: record.maintaindDate, // DateTime
          MaintenanceDate: record.maintenanceDate, // string
          FileLinks: record.fileLinks, // string
          DeviceID: record.deviceId, // Guid
          TypeOfMaintenance: record.typeOfMaintenance, // int
        })
      ),
    };
  };

  /**
   *
   */
  const uploadFiles = async (values: any) => {
    console.log("🚀 ~ uploadFiles ~ values:", values);
    let deviceAvatar = "";
    let deviceUsageInstructions = "";
    let appraisalFileName = "";
    let installationContract = "";
    // Ảnh đại diện deviceAvatar
    if (deviceAvatarFile.originFileObj) {
      await uploadDoc({
        file: deviceAvatarFile.originFileObj,
        urlTemp: "deviceAvatar",
      }).then((respon) => {
        console.log("🚀 ~ uploadFiles ~ respon:", respon);
        deviceAvatar = respon?.data;
      });
    }

    // Hướng dẫn sử dụng  deviceUsageInstructions
    if (deviceUsageInstructionsFile.originFileObj) {
      await uploadDoc({
        file: deviceUsageInstructionsFile.originFileObj,
        urlTemp: "deviceUsageInstructions",
      }).then((respon) => {
        console.log("🚀 ~ uploadFiles ~ respon:", respon);
        deviceUsageInstructions = respon?.data;
      });
    }
    // Hồ sơ thẩm định  appraisalFile
    if (appraisalFile.originFileObj) {
      await uploadDoc({
        file: appraisalFile.originFileObj,
        urlTemp: "appraisalFile",
      }).then((respon) => {
        console.log("🚀 ~ uploadFiles ~ respon:", respon);
        appraisalFileName = respon?.data;
      });
    }
    // Hợp đồng - Pháp lý installationContract
    if (installationContractFile.originFileObj) {
      await uploadDoc({
        file: installationContractFile.originFileObj,
        urlTemp: "installationContract",
      }).then((respon) => {
        console.log("🚀 ~ uploadFiles ~ respon:", respon);
        installationContract = respon?.data;
      });
    }

    return {
      deviceAvatar,
      installationContract,
      appraisalFile: appraisalFileName,
      deviceUsageInstructions,
    };
  };

  /**
   *
   */
  const handleSave = async () => {
    try {
      const values = await form.validateFields(); // Kiểm tra tính hợp lệ của form
      // lưu danh sách file contract trước
      let upload = await uploadFiles(values);
      console.log("🚀 ~ handleSave ~ upload:", upload);

      let adjustedValues: any = createAdjustedValuesForCreate(values, upload); // Tạo adjustedValues
      console.log("🚀 ~ handleSave ~ adjustedValues:", adjustedValues);

      //   if (isEditDevice) {
      //     // Nếu đang ở chế độ sửa, gọi API updateDevices
      //     // adjustedValues.Id = id;
      //     // await updatedDevices(adjustedValues).then((res: any) => {
      //     //   if (res?.succeeded) {
      //     //     message.success("Cập nhật thành công");
      //     //     onFinish(values); // Gọi hàm onFinish
      //     //   } else {
      //     //     message.error("Cập nhật thất bại");
      //     //   }
      //     // });
      //   } else {
      //     adjustedValues = createAdjustedValuesForCreate(values, upload); // Tạo adjustedValues cho create
      //     // Nếu không, gọi API createDevices để thêm mới
      //     await createDevices(adjustedValues).then((res: any) => {
      //       if (res?.succeeded) {
      //         message.success("Lưu thành công");
      //         onFinish(values); // Gọi hàm onFinish
      //       } else {
      //         message.error("Lưu thất bại");
      //       }
      //     });
      //   }
    } catch (info) {
      console.log("Validate Failed:", info);
    }
  };

  //
  return (
    <div className="devices-detail__topbar">
      <div className="detail__topbar--left">
        <div className="text-xl font-bold">
          {isEditDevice
            ? "Cập nhật lý lịch thiết bị"
            : "Thêm mới lý lịch thiết bị"}
        </div>
      </div>
      <div className="detail__topbar--right">
        <Button
          variant="dashed"
          className="btn-main-3"
          style={{ marginRight: "8px" }}
          onClick={handleCancel}
        >
          {mode === "edit" ? "Hủy" : "Quay lại"}
        </Button>
        {mode === "edit" && (
          <Button className="btn-main" onClick={handleSave}>
            Lưu
          </Button>
        )}
      </div>
    </div>
  );
};

export default DevicesDetailTopbar;
