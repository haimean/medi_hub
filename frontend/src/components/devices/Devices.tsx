import React, { useMemo, useState } from 'react';
import DevicesTopBar from './DevicesTopBar';
import { Tooltip } from 'antd';
import { DeleteOutlined, EditOutlined } from '@ant-design/icons';
import { AgGridReact } from 'ag-grid-react';

const Devices = () => {

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
                minWidth: 100,
                maxWidth: 150,
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
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Số Seri',
                field: 'serialNumber',
                tooltipField: 'Số Seri',
                minWidth: 150,
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Người quản lý',
                field: 'managerInfo.FullName',
                tooltipField: 'Người quản lý',
                minWidth: 200,
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Kỹ sư',
                field: 'engineerInfo.FullName',
                tooltipField: 'Kỹ sư',
                minWidth: 150,
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Trạng thái',
                field: 'status',
                tooltipField: 'Trạng thái',
                minWidth: 150,
                resizable: false,
                flex: 1,
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Hành động',
                field: '',
                resizable: false,
                minWidth: 150,
                width: 150,
                flex: 1,
                pinned: "right",
                cellRenderer: (record: any) => {
                    let hasPer = true;
                    return (
                        <Tooltip
                            placement='left'
                            title={hasPer ? 'Edit Advisory' : 'Do not have permission'}
                        >
                            <EditOutlined
                                title="Edit Advisory"
                                style={{
                                    marginLeft: 12,
                                    color: !hasPer ? '#6b7280' : '#000',
                                    cursor: !hasPer ? 'not-allowed' : 'pointer'
                                }}
                            />
                        </Tooltip>
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
                    rowData={[]}
                    columnDefs={columnDefs}
                    suppressRowHoverHighlight={true}
                ></AgGridReact>
            </div>
        </div>
    );
}

export default Devices;