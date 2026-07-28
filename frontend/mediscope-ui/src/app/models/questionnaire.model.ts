
export type QuestionnaireStatus = 'Active' | 'Inactive';

export type FieldType =
  | 'TextBox'
  | 'TextArea'
  | 'Number'
  | 'Date'
  | 'Dropdown'
  | 'RadioButton'
  | 'Checkbox';

export const FIELD_TYPES: { value: FieldType; label: string }[] = [
  { value: 'TextBox',     label: 'Text Box' },
  { value: 'TextArea',    label: 'Text Area' },
  { value: 'Number',      label: 'Number' },
  { value: 'Date',        label: 'Date' },
  { value: 'Dropdown',    label: 'Dropdown' },
  { value: 'RadioButton', label: 'Radio Button' },
  { value: 'Checkbox',    label: 'Checkbox' },
];

export const CHOICE_FIELD_TYPES: FieldType[] = ['Dropdown', 'RadioButton', 'Checkbox'];


export interface QuestionnaireListItem {
  id: string;
  name: string;
  description: string | null;
  department: string | null;
  status: QuestionnaireStatus;
  createdAt: string;
  updatedAt: string;
  questionCount: number;
}

export interface QuestionnaireDetail {
  id: string;
  name: string;
  description: string | null;
  department: string | null;
  status: QuestionnaireStatus;
  createdAt: string;
  updatedAt: string;
  questions: QuestionItem[];
}

export interface ActiveQuestionnaire {
  id: string;
  name: string;
  description: string | null;
  department: string | null;
}


export interface QuestionItem {
  id: string;
  label: string;
  fieldType: FieldType;
  placeholder: string | null;
  isRequired: boolean;
  displayOrder: number;
  defaultValue: string | null;
  options: QuestionOption[];
  minValue?: number | null;
  maxValue?: number | null;
  minLength?: number | null;
  maxLength?: number | null;
  regexPattern?: string | null;
}

export interface QuestionOption {
  id: string;
  label: string;
  value: string;
  displayOrder: number;
}


export interface CreateQuestionnaireRequest {
  name: string;
  description: string | null;
  department: string | null;
  status: QuestionnaireStatus;
}

export interface UpdateQuestionnaireRequest extends CreateQuestionnaireRequest {}

export interface CreateQuestionRequest {
  label: string;
  fieldType: FieldType;
  placeholder: string | null;
  isRequired: boolean;
  displayOrder: number;
  defaultValue: string | null;
  options: QuestionOptionRequest[] | null;
  minValue?: number | null;
maxValue?: number | null;
minLength?: number | null;
maxLength?: number | null;
regexPattern?: string | null;
}

export interface UpdateQuestionRequest extends CreateQuestionRequest {}

export interface QuestionOptionRequest {
  label: string;
  value: string;
  order: number;
}

export interface QuestionnaireListFilter {
  search?: string;
  status?: QuestionnaireStatus | '';
  pageNumber: number;
  pageSize: number;
}

export interface ReorderItem {
  id: string;
  order: number;
}

export interface ReorderQuestionsRequest {
  orderMap: ReorderItem[];
}


export interface QuestionnaireRender {
  questionnaireId: string;
  questionnaireName: string;
  description: string | null;
  department: string | null;
  questions: QuestionItem[];
}

export interface SubmitQuestionnaireRequest {
  questionnaireId: string;
  notes: string | null;
  responses: ResponseItem[];
}

export interface ResponseItem {
  questionId: string;
  responseValue: string | null;
  responseValues: string[] | null;
}

export interface SubmissionHistoryItem {
  submissionId: string;
  questionnaireId: string;
  questionnaireName: string;
  department: string | null;
  submittedAt: string;
  submittedByName: string;
  notes: string | null;
}

export interface SubmissionDetail {
  submissionId: string;
  questionnaireId: string;
  questionnaireName: string;
  department: string | null;
  submittedAt: string;
  submittedByName: string;
  notes: string | null;
  responses: SubmissionResponseItem[];
}

export interface SubmissionResponseItem {
  questionId: string;
  label: string;
  fieldType: FieldType;
  displayOrder: number;
  responseValue: string | null;
  responseValues: string[] | null;
}