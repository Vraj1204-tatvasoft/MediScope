
export enum BedStatus {
  Available = 0,
  Occupied = 1,
  UnderMaintenance = 2,
  Inactive = 3
}

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}

export interface WardSummary { id: string; name: string; description?: string; }
export interface CreateWardPayload { name: string; description?: string; }
export interface UpdateWardPayload { name: string; description?: string; }

export interface RoomType { id: string; name: string; }
export interface CreateRoomTypePayload { name: string; }
export interface UpdateRoomTypePayload { name: string; }

export interface RoomSummary {
  id: string;
  wardId: string;        
  roomTypeId: string;
  roomNumber: string;
  wardName: string;
  roomTypeName: string;
  bedCount: number;
}
export interface CreateRoomPayload { roomNumber: string; wardId: string; roomTypeId: string; numberOfBeds: number; }
export interface UpdateRoomPayload { roomNumber: string; wardId: string; roomTypeId: string; }

export interface BedSummary {
  id: string;
  bedNumber: string;
  status: BedStatus;
  roomNumber: string;
  wardName: string;
}
export interface UpdateBedPayload { bedNumber: string; status: BedStatus; }