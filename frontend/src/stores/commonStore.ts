// src/stores/commonStore.ts
import { createStore, Reducer } from 'redux';

// Action type constants
const UPDATE_VARIABLE = 'UPDATE_VARIABLE';
const SET_USERNAME = 'SET_USERNAME';
const SET_AUTH = 'SET_AUTH';
const SET_DEPARTMENTS = 'SET_DEPARTMENTS'; // New action type for departments
const SET_USER_INFO = 'SET_USER_INFO'; // New action type for user information
const SET_SELECTED_DEPARTMENT = 'SET_SELECTED_DEPARTMENT'; // New action type for selected department

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
}

// Định nghĩa trạng thái ban đầu
const initialState: State = {
    globalVariable: 'Giá trị mặc định',
    username: null, // Mặc định là null
    isAuth: false, // Mặc định là false
    departments: [], // Mặc định là mảng rỗng
    userInfo: null, // Mặc định là null
    department: null, // Mặc định là null
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
export const setSelectedDepartment = (department: any | null) => ({
    type: SET_SELECTED_DEPARTMENT,
    payload: department,
});

// Optional: Action to update global variable
export const updateVariable = (value: string) => ({
    type: UPDATE_VARIABLE,
    payload: value,
});

export default commonStore;