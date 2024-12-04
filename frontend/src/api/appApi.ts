import axiosClient from "./axiosClient";

//#region API Check Auth

/**
 * 
 * @param request 
 * @returns 
 */
async function apiCheckPermission(token: any) {
   return await axiosClient.get(`${process.env.REACT_APP_API_URL}/check-token?token=${token}`);
}

/**
 * 
 * @param request 
 * @returns 
 */
async function apiLogin(userName: string, passWord: string) {
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
async function apiLogout(userName: string) {
   return await axiosClient.post(`${process.env.REACT_APP_API_URL}/logout?userName=${userName}`);
}

//#endregion

export {
   apiCheckPermission,
   apiLogin,
   apiLogout
};
