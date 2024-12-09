import axiosClient from "./axiosClient";

//#region API Check Auth

/**
 * 
 * @param request 
 * @returns 
 */
export async function apiCheckPermission(token: any) {
   return await axiosClient.get(`${process.env.REACT_APP_API_URL}/check-token?token=${token}`);
}

/**
 * 
 * @param request 
 * @returns 
 */
export async function apiLogin(userName: string, passWord: string) {
   return await axiosClient.post(`${process.env.REACT_APP_API_URL}/login`, {
      username: userName,
      password: passWord
    });
}

/**
 * 
 * @param request 
 * @returns 
 */
export async function apiLogout(userName: string) {
   return await axiosClient.post(`${process.env.REACT_APP_API_URL}/logout?userName=${userName}`);
}

//#endregion

//#region API Department

export async function createDepartments(params: any) {
   return await axiosClient.post(`${process.env.REACT_APP_API_URL}/v1/departments`, params);
}

export async function getDepartments() {
   return await axiosClient.get(`${process.env.REACT_APP_API_URL}/v1/departments`);
}

export async function updatedDepartments(params: any) {
   return await axiosClient.put(`${process.env.REACT_APP_API_URL}/v1/departments`, params);
}

export async function deleteDepartments(params: any) {
   return await axiosClient.delete(`${process.env.REACT_APP_API_URL}/v1/departments`, params);
}
//#endregion
