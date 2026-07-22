import { PagedResponse } from "./paged-response.model";


export interface HospitalDashboardResponse {
  summary: HospitalSummary;
  rooms: PagedResponse<HospitalRoom>;
}

export interface HospitalSummary {
  totalRooms: number;
  totalBeds: number;
  occupiedBeds: number;
  availableBeds: number;
  admittedPatients: number;
  dischargedPatientsToday: number;
}

export interface HospitalRoom {
  id: string;
  roomNumber: string;
  wardName: string;
  roomTypeName: string;
  floor: number;
  totalBeds: number;
  occupiedBeds: number;
  availableBeds: number;
  occupancyStatus: OccupancyStatus;
  ward_Id: string;
  room_Type_Id: string;
  total_Count: number;
}

export type OccupancyStatus = 'Empty' | 'Partially Occupied' | 'Full';