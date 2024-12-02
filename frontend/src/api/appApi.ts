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


//#endregion

export {
   apiCheckPermission
};
