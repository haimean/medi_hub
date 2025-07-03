/**
     * 
     * @param fileName 
     * @returns 
     */
export const getFileType = (fileName: any) => {
    const extension = fileName?.split('.').pop().toLowerCase();
    switch (extension) {
        case 'pdf':
            return 'application/pdf';
        case 'jpg':
        case 'jpeg':
            return 'image/jpeg';
        case 'png':
            return 'image/png';
        case 'gif':
            return 'image/gif';
        // Thêm các loại tệp khác nếu cần
        default:
            return 'application/octet-stream'; // Loại mặc định
    }
}

export const getManufacturerName = (type: number) => {
    const types: { [key: number]: string } = {
        1: 'Roche',
        2: 'Beckman',
        3: 'Abbott',
        4: 'Tủ Mát',
        5: 'Tủ Âm',
        6: 'Lọc RO'
    };
    return types[type] || '';
}