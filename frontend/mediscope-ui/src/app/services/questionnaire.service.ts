import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { PagedResponse } from '../models/paged-response.model';
import { QuestionnaireListFilter, QuestionnaireListItem, QuestionnaireDetail, ActiveQuestionnaire, CreateQuestionnaireRequest, UpdateQuestionnaireRequest, QuestionItem, CreateQuestionRequest, UpdateQuestionRequest, ReorderQuestionsRequest, QuestionnaireRender, SubmitQuestionnaireRequest, SubmissionHistoryItem, SubmissionDetail } from '../models/questionnaire.model';
import { BaseHttpService } from './base-http.service';

@Injectable({ providedIn: 'root' })
export class QuestionnaireService {
  private readonly http = inject(BaseHttpService);
  private readonly base = `questionnaires`; 

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

//   submitQuestionnaire(patientId: string, request: SubmitQuestionnaireRequest): Observable<ApiResponse<{ submissionId: string }>> {
//     return this.http.post<{ submissionId: string }>(
//       `${this.base}/submit/${patientId}`, request
//     );
//   }

//   getPatientSubmissions(patientId: string, pageNumber = 1, pageSize = 10): Observable<ApiResponse<PagedResponse<SubmissionHistoryItem>>> {
//     const params = new HttpParams()
//       .set('pageNumber', pageNumber)
//       .set('pageSize', pageSize);
      
//     return this.http.get<PagedResponse<SubmissionHistoryItem>>(
//       `${this.base}/submissions/patient/${patientId}`, { params }
//     );
//   }

//   getSubmissionDetail(submissionId: string): Observable<ApiResponse<SubmissionDetail>> {
//     return this.http.get<SubmissionDetail>(
//       `${this.base}/submissions/${submissionId}`
//     );
//   }
}