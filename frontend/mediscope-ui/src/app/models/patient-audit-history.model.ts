import { PagedResponse } from "./paged-response.model";


export interface PatientAuditHistory {
  id: string;
  patientId: string;
  changedByUserId: string;
  changedByUserName: string;
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
  changedAt: string;
}

export interface PatientAuditHistoryResponse
  extends PagedResponse<PatientAuditHistory> {}