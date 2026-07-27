import { inject, Injectable } from '@angular/core';
import { BaseHttpService } from './base-http.service';
import { HttpParams } from '@angular/common/http';
import { 
  BedSummary, CreateRoomPayload, CreateRoomTypePayload, CreateWardPayload, 
  PaginationParams, RoomSummary, RoomType, UpdateBedPayload, 
  UpdateRoomPayload, UpdateRoomTypePayload, UpdateWardPayload, WardSummary 
} from '../models/manage-room.model';
import { PagedResponse } from '../models/paged-response.model';


@Injectable({
  providedIn: 'root'
})
export class ManageRoomService {
  private readonly baseHttp = inject(BaseHttpService);

  private buildParams(params: PaginationParams): HttpParams {
    let httpParams = new HttpParams();
    
    Object.keys(params).forEach(key => {
      const value = (params as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.append(key, value.toString());
      }
    });
    
    return httpParams;
  }

  // --- WARDS ---
  getWards(params: PaginationParams) {
    return this.baseHttp.get<PagedResponse<WardSummary>>(`wards`, { params: this.buildParams(params) });
  }
  createWard(payload: CreateWardPayload) {
    return this.baseHttp.post<any>(`wards`, payload, { showSuccess: true });
  }
  updateWard(id: string, payload: UpdateWardPayload) {
    return this.baseHttp.put<any>(`wards/${id}`, payload, { showSuccess: true });
  }
  deleteWard(id: string) {
    return this.baseHttp.delete<any>(`wards/${id}`, { showSuccess: true });
  }

  // --- ROOM TYPES ---
  getRoomTypes(params: PaginationParams) {
    return this.baseHttp.get<PagedResponse<RoomType>>(`room-types`, { params: this.buildParams(params) });
  }
  createRoomType(payload: CreateRoomTypePayload) {
    return this.baseHttp.post<any>(`room-types`, payload, { showSuccess: true });
  }
  updateRoomType(id: string, payload: UpdateRoomTypePayload) {
    return this.baseHttp.put<any>(`room-types/${id}`, payload, { showSuccess: true });
  }
  deleteRoomType(id: string) {
    return this.baseHttp.delete<any>(`room-types/${id}`, { showSuccess: true });
  }

  // --- ROOMS ---
  getRooms(params: PaginationParams) {
    return this.baseHttp.get<PagedResponse<RoomSummary>>(`rooms`, { params: this.buildParams(params) });
  }
  createRoom(payload: CreateRoomPayload) {
    return this.baseHttp.post<any>(`rooms`, payload, { showSuccess: true });
  }
  updateRoom(id: string, payload: UpdateRoomPayload) {
    return this.baseHttp.put<any>(`rooms/${id}`, payload, { showSuccess: true });
  }
  deleteRoom(id: string) {
    return this.baseHttp.delete<any>(`rooms/${id}`, { showSuccess: true });
  }

  // --- BEDS ---
  getBeds(params: PaginationParams) {
    return this.baseHttp.get<PagedResponse<BedSummary>>(`beds`, { params: this.buildParams(params) });
  }
  updateBed(id: string, payload: UpdateBedPayload) {
    return this.baseHttp.put<any>(`beds/${id}`, payload, { showSuccess: true });
  }
  deleteBed(id: string) {
    return this.baseHttp.delete<any>(`beds/${id}`, { showSuccess: true });
  }
}