import axios from 'axios';
import queryString from 'query-string';

// Sử dụng biến môi trường để lấy URL API
const apiUrl = process.env.REACT_APP_API_URL;

/**
 * 
 * @returns 
 */
const getLocalToken = () => {
  let token = window.localStorage.getItem('MEDI.Token');
  return token;
}


const axiosClient = axios.create({
  baseURL: apiUrl,
  headers: { 'content-type': 'application/json' },
  paramsSerializer: (params) => queryString.stringify(params),
});

axiosClient.interceptors.request.use(async (config: any) => {
  let token = getLocalToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

axiosClient.interceptors.response.use(
  (response: any) => {
    if (response && response.data) {
      return response.data;
    }
    return response;
  },
  (error) => {
    console.log(
      'error => ',
      error?.response?.config?.url,
      error?.response?.status
    );

    if (
      error?.response?.config?.url === 'User/get-my-projects' &&
      error?.response?.status >= 400
    ) {
      throw error;
    }

    if (
      error?.response?.status === 403 &&
      window.location.pathname !== '/403-page'
    ) {
      // window.location.replace('/403-page')
    }
    if (
      error?.response?.status === 404 &&
      window.location.pathname !== '/404-page'
    ) {
      // window.location.replace('/404-page')
    }
    throw error;
  }
);

export default axiosClient;
