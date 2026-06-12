export const API_BASE_URL = 'http://localhost:5211/api';
 
export const API_ENDPOINTS = {
  auth: {
    login:    `${API_BASE_URL}/auth/login`,
    register: `${API_BASE_URL}/auth/register`,
    refresh:  `${API_BASE_URL}/auth/refresh`,
    logout:   `${API_BASE_URL}/auth/logout`,
  }
};
 
export const ROLE_ROUTES: Record<string, string> = {
  Patient: '/patient/dashboard',
  Doctor:  '/doctor/dashboard',
  Admin:   '/admin/dashboard',
  DoctorPasswordChange: '/doctor/profile',
};