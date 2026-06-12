export type UserRole = 'Patient' | 'Doctor' | 'Admin';
export type Gender = 'Male' | 'Female' | 'Other';
export interface LoginRequest {
  email: string;
  password: string;
}
 
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  dateOfBirth?: string;
  gender?: Gender;
  bloodGroup?: string;
  contactNumber?: string;
  address?: string;
}
 
export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}
 
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  mustChangePassword: boolean;
  user: UserProfile;
}