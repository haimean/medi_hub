// src/stores/commonStore.ts
import { createStore, Reducer } from 'redux';

// Định nghĩa kiểu cho trạng thái
interface State {
    globalVariable: string;
    username: string | null; // Thêm trường username
    isAuth: boolean; // Thêm trường isAuth
}

// Định nghĩa trạng thái ban đầu
const initialState: State = {
    globalVariable: 'Giá trị mặc định',
    username: null, // Mặc định là null
    isAuth: false, // Mặc định là false
};

// Định nghĩa kiểu cho các action
interface Action {
    type: string;
    payload?: any;
}

// Định nghĩa reducer
const reducer: Reducer<State, Action> = (state = initialState, action) => {
    switch (action.type) {
        case 'UPDATE_VARIABLE':
            return { ...state, globalVariable: action.payload };
        case 'SET_USERNAME': // Thêm action để cập nhật username
            return { ...state, username: action.payload };
        case 'SET_AUTH': // Thêm action để cập nhật isAuth
            return { ...state, isAuth: action.payload };
        default:
            return state;
    }
};

// Tạo store
const commonStore = createStore(reducer);

// Định nghĩa action để cập nhật username
export const setUsername = (username: string) => ({
    type: 'SET_USERNAME',
    payload: username,
});

// Định nghĩa action để cập nhật isAuth
export const setAuth = (isAuth: boolean) => ({
    type: 'SET_AUTH',
    payload: isAuth,
});

export default commonStore;