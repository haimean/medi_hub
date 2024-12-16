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
                maxWidth: 50,
                minWidth: 50,
                suppressMenu: true,
                floatingFilterComponentParams: { suppressFilterButton: true },
                valueGetter: "node.rowIndex + 1",
                pinned: "left"
            },
            {
                headerName: 'Project Name',
                field: 'projectName',
                tooltipField: 'Project Name',
                resizable: true,
                maxWidth: 120,
                minWidth: 120,
                flex: 1,
                filter: 'agSetColumnFilter',
                pinned: "left"
            },
            {
                headerName: 'Status',
                field: 'status',
                tooltipField: 'Status',
                minWidth: 150,
                maxWidth: 150,
                resizable: false,
                flex: 1,
                pinned: "right",
                filter: 'agSetColumnFilter'
            },
            {
                headerName: 'Actions',
                field: '',
                resizable: false,
                minWidth: 100,
                width: 100,
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