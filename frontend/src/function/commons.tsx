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