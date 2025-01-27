// src/stores/commonStore.ts
import { createStore, Reducer } from 'redux';

// Action type constants
const UPDATE_VARIABLE = 'UPDATE_VARIABLE';
const SET_USERNAME = 'SET_USERNAME';
const SET_AUTH = 'SET_AUTH';
const SET_DEPARTMENTS = 'SET_DEPARTMENTS'; // New action type for departments
const SET_USER_INFO = 'SET_USER_INFO'; // New action type for user information
const SET_SELECTED_DEPARTMENT = 'SET_SELECTED_DEPARTMENT'; // New action type for selected department
const SET_SELECTED_BRAND = 'SET_SELECTED_BRAND'; // New action type for selected brand
const SET_SELECTED_DEVICE_TYPE = 'SET_SELECTED_DEVICE_TYPE'; // New action type for selected device type
const SET_IS_EDIT_DEVICE = 'SET_IS_EDIT_DEVICE'; // New action type for edit device state

// Định nghĩa kiểu cho trạng thái
interface UserInfo {
    id: string; // User ID
    username: string | null; // Username
    email: string | null; // Email
    departmentIds: string[]; // List of department IDs
    role: string; // User role
}

interface State {
    globalVariable: string;
    username: string | null; // Thêm trường username
    isAuth: boolean; // Thêm trường isAuth
    departments: any[]; // Thêm trường departments
    userInfo: UserInfo | null; // Thêm trường userInfo
    department: any | null; // Thêm trường department
    selectedBrand: string | null; // Thêm trường selectedBrand
    selectedDeviceType: string | null; // Thêm trường selectedDeviceType
    isEditDevice: boolean; // Thêm trường isEditDevice
}

// Định nghĩa trạng thái ban đầu
const initialState: State = {
    globalVariable: 'Giá trị mặc định',
    username: null, // Mặc định là null
    isAuth: false, // Mặc định là false
    departments: [], // Mặc định là mảng rỗng
    userInfo: null, // Mặc định là null
    department: null, // Mặc định là null
    selectedBrand: null, // Mặc định là null
    selectedDeviceType: null, // Mặc định là null
    isEditDevice: false, // Mặc định là false
};

// Định nghĩa kiểu cho các action
interface Action {
    type: string;
    payload?: any;
}

// Định nghĩa reducer
const reducer: Reducer<State, Action> = (state = initialState, action) => {
    switch (action.type) {
        case UPDATE_VARIABLE:
            return { ...state, globalVariable: action.payload };
        case SET_USERNAME: // Thêm action để cập nhật username
            return { ...state, username: action.payload };
        case SET_AUTH: // Thêm action để cập nhật isAuth
            return { ...state, isAuth: action.payload };
        case SET_DEPARTMENTS: // Thêm action để cập nhật departments
            return { ...state, departments: action.payload };
        case SET_USER_INFO: // Thêm action để cập nhật userInfo
            return { ...state, userInfo: action.payload };
        case SET_SELECTED_DEPARTMENT: // Thêm action để cập nhật department
            return { ...state, department: action.payload };
        case SET_SELECTED_BRAND: // Thêm action để cập nhật selectedBrand
            return { ...state, selectedBrand: action.payload };
        case SET_SELECTED_DEVICE_TYPE: // Thêm action để cập nhật selectedDeviceType
            return { ...state, selectedDeviceType: action.payload };
        case SET_IS_EDIT_DEVICE: // Thêm action để cập nhật trạng thái isEditDevice
            return { ...state, isEditDevice: action.payload };
        default:
            return state;
    }
};

// Tạo store
const commonStore = createStore(reducer);

// Định nghĩa action để cập nhật username
export const setUsername = (username: string) => ({
    type: SET_USERNAME,
    payload: username,
});

// Định nghĩa action để cập nhật isAuth
export const setAuth = (isAuth: boolean) => ({
    type: SET_AUTH,
    payload: isAuth,
});

// Định nghĩa action để cập nhật departments
export const setDepartments = (departments: any[]) => ({
    type: SET_DEPARTMENTS,
    payload: departments,
});

// Định nghĩa action để cập nhật userInfo
export const setUserInfo = (userInfo: UserInfo) => ({
    type: SET_USER_INFO,
    payload: userInfo,
});

// Định nghĩa action để cập nhật department
export const setDepartment = (department: any | null) => ({
    type: SET_SELECTED_DEPARTMENT,
    payload: department,
});

// Định nghĩa action để cập nhật selectedBrand
export const setSelectedBrand = (brand: string | null) => ({
    type: SET_SELECTED_BRAND,
    payload: brand,
});

// Định nghĩa action để cập nhật selectedDeviceType
export const setSelectedDeviceType = (deviceType: string | null) => ({
    type: SET_SELECTED_DEVICE_TYPE,
    payload: deviceType,
});

// Định nghĩa action để cập nhật trạng thái isEditDevice
export const setIsEditDevice = (isEdit: boolean) => ({
    type: SET_IS_EDIT_DEVICE,
    payload: isEdit,
});

// Optional: Action to update global variable
export const updateVariable = (value: string) => ({
    type: UPDATE_VARIABLE,
    payload: value,
});

export default commonStore;