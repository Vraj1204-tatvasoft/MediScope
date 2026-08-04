import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { PagedResponse } from '../models/paged-response.model';
import { QuestionnaireListFilter, QuestionnaireListItem, QuestionnaireDetail, ActiveQuestionnaire, CreateQuestionnaireRequest, UpdateQuestionnaireRequest, QuestionItem, CreateQuestionRequest, UpdateQuestionRequest, ReorderQuestionsRequest, QuestionnaireRender, SubmitQuestionnaireRequest, SubmissionHistoryItem, SubmissionDetail, AssignQuestionnaireRequest, PatientAssignmentFilterDto, PatientAssignmentResponseDto, SubmissionVersion } from '../models/questionnaire.model';
import { BaseHttpService } from './base-http.service';

@Injectable({ providedIn: 'root' })
export class QuestionnaireService {
  private readonly http = inject(BaseHttpService);
  private readonly base = `questionnaires`; 
  private baseUrl = 'questionnaire-assignments';

  getQuestionnaires(filter: QuestionnaireListFilter): Observable<ApiResponse<PagedResponse<QuestionnaireListItem>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber)
      .set('pageSize', filter.pageSize);

    if (filter.search) params = params.set('search', filter.search);
    if (filter.status) params = params.set('status', filter.status);

    return this.http.get<PagedResponse<QuestionnaireListItem>>(this.base, { params });
  }

  getQuestionnaireById(id: string): Observable<ApiResponse<QuestionnaireDetail>> {
    return this.http.get<QuestionnaireDetail>(`${this.base}/${id}`);
  }

  getActiveQuestionnaires(): Observable<ApiResponse<ActiveQuestionnaire[]>> {
    return this.http.get<ActiveQuestionnaire[]>(`${this.base}/active`);
  }

  createQuestionnaire(request: CreateQuestionnaireRequest): Observable<ApiResponse<{ id: string }>> {
    return this.http.post<{ id: string }>(this.base, request ,{showSuccess: true, showError: true});
  }

  updateQuestionnaire(id: string, request: UpdateQuestionnaireRequest): Observable<ApiResponse<null>> {
    return this.http.put<null>(`${this.base}/${id}`, request, {showSuccess: true, showError: true});
  }

  deleteQuestionnaire(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`${this.base}/${id}`);
  }

  toggleStatus(id: string): Observable<ApiResponse<null>> {
    return this.http.patch<null>(`${this.base}/${id}/status`, {}, {showSuccess: true, showError: true});
  }

  getQuestions(questionnaireId: string): Observable<ApiResponse<QuestionItem[]>> {
    return this.http.get<QuestionItem[]>(`${this.base}/${questionnaireId}/questions`);
  }

  addQuestion(questionnaireId: string, request: CreateQuestionRequest): Observable<ApiResponse<{ id: string }>> {
    return this.http.post<{ id: string }>(`${this.base}/${questionnaireId}/questions`, request, {showSuccess: true, showError: true});
  }

  updateQuestion(questionId: string, request: UpdateQuestionRequest): Observable<ApiResponse<null>> {
    return this.http.put<null>(`${this.base}/questions/${questionId}`, request, {showSuccess: true, showError: true});
  }

  deleteQuestion(questionId: string): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`${this.base}/questions/${questionId}`, {showSuccess: true, showError: true});
  }

  reorderQuestions(questionnaireId: string, request: ReorderQuestionsRequest): Observable<ApiResponse<null>> {
    return this.http.patch<null>(`${this.base}/${questionnaireId}/questions/reorder`, request, {showSuccess: true, showError: true});
  }

  getQuestionnaireRender(questionnaireId: string): Observable<ApiResponse<QuestionnaireRender>> {
    return this.http.get<QuestionnaireRender>(`${this.base}/${questionnaireId}/render`);
  }

  assignQuestionnaire(request: AssignQuestionnaireRequest): Observable<ApiResponse<{ assignmentId: string }>> {
    return this.http.post<{ assignmentId: string }>(`questionnaire-assignments`, request);
  }

  unassignQuestionnaire(assignmentId: string): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`questionnaire-assignments/${assignmentId}`);
  }

  getPatientAssignments(
    patientId: string,
    filter: PatientAssignmentFilterDto
  ): Observable<ApiResponse<PagedResponse<PatientAssignmentResponseDto>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber)
      .set('pageSize', filter.pageSize);
      if (filter.status) {
        params = params.set('status', filter.status);
      }
  
      if (filter.assignedBy) {
        params = params.set('assignedBy', filter.assignedBy);
      }
    return this.http.get<PagedResponse<PatientAssignmentResponseDto>>(
      `patients/${patientId}/questionnaire-assignments`, { params }
    );
  }

  getSubmissionDetail(submissionId: string): Observable<ApiResponse<SubmissionDetail>> {
    return this.http.get<SubmissionDetail>(`questionnaire-submissions/${submissionId}`);
  }
  getRender(assignmentId: string, patientId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/${assignmentId}/render?patientId=${patientId}`);
  }

  saveDraft(assignmentId: string, patientId: string, payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/${assignmentId}/draft?patientId=${patientId}`, payload, {showSuccess: true, showError: true});
  }

  submit(assignmentId: string, patientId: string, payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/${assignmentId}/submit?patientId=${patientId}`, payload, {showSuccess: true, showError: true});
  }
  getSubmissionVersions(assignmentId: string): Observable<ApiResponse<SubmissionVersion[]>> {
    return this.http.get<SubmissionVersion[]>(`${this.baseUrl}/${assignmentId}/versions`);
  }
}