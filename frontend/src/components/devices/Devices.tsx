import React, { useMemo, useState } from 'react';
import DevicesTopBar from './DevicesTopBar';
import { Tooltip } from 'antd';
import { DeleteOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import { AgGridReact } from 'ag-grid-react';
import { useQuery } from '@tanstack/react-query';
import { getDevices } from '../../api/appApi';

const Devices = () => {
    
    const [refreshDevice, setRefreshDevice] = useState([]);

    // Use useQuery to check token validity
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
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Hành động',
                field: '',
                resizable: false,
                minWidth: 120,
                width: 120,
                pinned: "right",
                sortable: false, // Tắt chức năng sắp xếp cho cột này
                suppressMenu: true, // Ẩn menu của cột
                cellRenderer: (record: any) => {
                    let hasPer = true;
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
                                />
                            </Tooltip>
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
                        </>
                    )
                }
            },
        ];
    }, []);



    return (
        <div className="medi-devices">
            <DevicesTopBar />
            <div className='devices__content ag-theme-alpine'>
                <AgGridReact
                    animateRows={true}
                    rowData={data?.data}
                    columnDefs={columnDefs}
                    suppressRowHoverHighlight={true}
                ></AgGridReact>
            </div>
        </div>
    );
}

export default Devices;