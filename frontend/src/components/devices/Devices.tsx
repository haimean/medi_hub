import React, { useMemo, useRef, useState } from 'react';
import DevicesTopBar from './DevicesTopBar';
import { Tooltip, Popconfirm, message } from 'antd'; // Import Popconfirm
import { DeleteOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import { AgGridReact } from 'ag-grid-react';
import { useQuery } from '@tanstack/react-query';
import { deleteDevices, getDevices } from '../../api/appApi';
import { useNavigate } from 'react-router-dom';
import MaintenanceStatus from '../common/MaintenanceStatus';

const Devices = () => {
    const [refreshDevice, setRefreshDevice] = useState(0); // Sử dụng số để trigger refetch
    const navigate = useNavigate();
    const gridRef = useRef<any>(null);

    // Use useQuery to fetch devices
    const { isError, isLoading, data } = useQuery({
        queryKey: [`all-devices`, refreshDevice],
        queryFn: () => getDevices(),
        refetchOnWindowFocus: false
    });

    /**
     * Column Devices
     * CreatedBy: PQ Huy (16.12.2024)
     */
    const columnDefs: any = useMemo(() => {
        return [
            {
                headerName: 'S/N',
                field: 'S/N',
                tooltipField: 'S/N',
                resizable: true,
                maxWidth: 100,
                minWidth: 50,
                suppressMenu: true,
                floatingFilterComponentParams: { suppressFilterButton: true },
                valueGetter: "node.rowIndex + 1",
                pinned: "left"
            },
            {
                headerName: 'MTB',
                field: 'deviceCode',
                tooltipField: 'MTB',
                resizable: true,
                minWidth: 120,
                maxWidth: 120,
                flex: 1,
                filter: 'agSetColumnFilter',
                pinned: "left"
            },
            {
                headerName: 'Tên thiết bị',
                field: 'deviceName',
                tooltipField: 'Tên thiết bị',
                resizable: true,
                minWidth: 120,
                flex: 1,
                filter: 'agSetColumnFilter',
                pinned: "left"
            },
            {
                headerName: 'Nước sản xuất',
                field: 'manufacturerCountry',
                tooltipField: 'Nước sản xuất',
                minWidth: 150,
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Tên hãng',
                field: 'manufacturerName',
                tooltipField: 'Tên hãng',
                minWidth: 150,
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Chức năng',
                field: 'functionName',
                tooltipField: 'Chức năng',
                minWidth: 150,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Số Seri',
                field: 'serialNumber',
                tooltipField: 'Số Seri',
                minWidth: 150,
                maxWidth: 150,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Người quản lý',
                field: 'managerInfo.FullName',
                tooltipField: 'Người quản lý',
                minWidth: 200,
                flex: 1,
                filter: 'agSetColumnFilter',
                cellRenderer: (params: any) => {
                    return <div>{params?.data?.managerInfo?.fullName}</div>
                }
            },
            {
                headerName: 'Kỹ sư',
                field: 'engineerInfo.FullName',
                tooltipField: 'Kỹ sư',
                minWidth: 150,
                flex: 1,
                filter: 'agSetColumnFilter',
                cellRenderer: (params: any) => {
                    return <div>{params?.data?.engineerInfo?.fullName}</div>
                }
            },
            {
                headerName: 'Trạng thái',
                field: 'status',
                tooltipField: 'Trạng thái',
                maxWidth: 150,
                minWidth: 150,
                pinned: "right",
                flex: 1,
                filter: 'agSetColumnFilter',
                cellRenderer: (params: any) => {
                    return <MaintenanceStatus maintenanceDate={params?.data?.maintenanceSchedule} />;
                }
            },
            {
                headerName: 'Hành động',
                field: '',
                resizable: false,
                minWidth: 120,
                width: 120,
                pinned: "right",
                sortable: false,
                suppressMenu: true,
                cellRenderer: (record: any) => {
                    let hasPer = true; // Placeholder for permission check
                    return (
                        <>
                            <Tooltip
                                placement='left'
                                title={hasPer ? 'Xem chi tiết' : 'Không có quyền'}
                            >
                                <EyeOutlined
                                    title="Xem chi tiết"
                                    style={{
                                        marginLeft: 12,
                                        color: !hasPer ? '#6b7280' : '#000',
                                        cursor: !hasPer ? 'not-allowed' : 'pointer'
                                    }}
                                />
                            </Tooltip>
                            <Tooltip
                                placement='left'
                                title={hasPer ? 'Sửa thiết bị' : 'Không có quyền'}
                            >
                                <EditOutlined
                                    title="Sửa thiết bị"
                                    style={{
                                        marginLeft: 12,
                                        color: !hasPer ? '#6b7280' : '#000',
                                        cursor: !hasPer ? 'not-allowed' : 'pointer'
                                    }}
                                    onClick={() => navigate(`/devices/detail/${record?.data?.id}`)}
                                />
                            </Tooltip>
                            <Popconfirm
                                title="Bạn có chắc chắn muốn xóa thiết bị này không?"
                                onConfirm={() => onDeleteDevice(record)}
                                okText="Có"
                                placement="leftTop"
                                cancelText="Không"
                            >
                                <Tooltip
                                    placement='left'
                                    title={hasPer ? 'Xóa thiết bị' : 'Không có quyền'}
                                >
                                    <DeleteOutlined
                                        title="Xóa thiết bị"
                                        style={{
                                            marginLeft: 12,
                                            color: !hasPer ? '#6b7280' : 'red',
                                            cursor: !hasPer ? 'not-allowed' : 'pointer'
                                        }}
                                    />
                                </Tooltip>
                            </Popconfirm>
                        </>
                    )
                }
            },
        ];
    }, []);

    /**
     * Func on delete device
     * @param data 
     * CreatedBy: PQ Huy (26.01.2025)
     */
    const onDeleteDevice = async (data: any) => {
        try {
            await deleteDevices([data?.data?.id]);
            setRefreshDevice(prev => prev + 1); // Trigger refetch by incrementing the state
            message.success("Xóa thiết bị thành công");
        } catch (error) {
            console.error("Error deleting device:", error);
            message.error("Có lỗi xảy ra");
        }
    }

    return (
        <div className="medi-devices">
            <DevicesTopBar
                gridRef={gridRef}
            />
            <div className='devices__content ag-theme-alpine'>
                <AgGridReact
                    ref={gridRef}
                    animateRows={true}
                    rowData={data?.data}
                    columnDefs={columnDefs}
                    suppressRowHoverHighlight={true}
                />
            </div>
        </div>
    );
}

export default Devices;