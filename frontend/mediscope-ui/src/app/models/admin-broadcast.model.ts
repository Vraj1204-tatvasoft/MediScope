// ── String enums — match what the C# JSON serializer actually returns ─────────

export type BroadcastChannel  = 'Email' | 'Sms' | 'PushNotification';
export type BroadcastAudience = 'Patients' | 'Doctors' | 'Admins' | 'All';
export type BroadcastStatus   = 'Draft' | 'Pending' | 'Processing' | 'Completed' | 'Failed';

// ── Display label maps (string key → display string) ─────────────────────────

export const CHANNEL_ICONS: Record<BroadcastChannel, string> = {
  Email:            'email',
  Sms:              'sms',
  PushNotification: 'notifications',
};

export const CHANNEL_LABELS: Record<BroadcastChannel, string> = {
  Email:            'Email',
  Sms:              'SMS',
  PushNotification: 'Push Notification',
};

export const AUDIENCE_LABELS: Record<BroadcastAudience, string> = {
  Patients: 'All Patients',
  Doctors:  'All Doctors',
  Admins:   'All Admins',
  All:      'Everyone',
};

// ── Integer values sent TO the backend on create/update ───────────────────────
// The GET returns strings, but POST/PUT accept integers.

export const CHANNEL_INT: Record<BroadcastChannel, number> = {
  Email:            0,
  Sms:              1,
  PushNotification: 2,
};

export const AUDIENCE_INT: Record<BroadcastAudience, number> = {
  Patients: 0,
  Doctors:  1,
  Admins:   2,
  All:      3,
};

// ── List item (GET paged) ─────────────────────────────────────────────────────

export interface BroadcastListItem {
  id:              string;
  name:            string;
  channel:         BroadcastChannel;
  channelDisplay:  string;
  subject:         string | null;
  audience:        BroadcastAudience;
  audienceDisplay: string;
  status:          BroadcastStatus;
  statusDisplay:   string;
  totalRecipients: number;
  sentCount:       number;
  failedCount:     number;
  scheduledAt:     string | null;
  completedAt:     string | null;
  createdAt:       string;
  progressPercent: number;
}

// ── Full detail (GET by id) ───────────────────────────────────────────────────

export interface BroadcastDetail {
  id:              string;
  name:            string;
  channel:         BroadcastChannel;
  channelDisplay:  string;
  subject:         string | null;
  message:         string;
  audience:        BroadcastAudience;
  audienceDisplay: string;
  status:          BroadcastStatus;
  statusDisplay:   string;
  totalRecipients: number;
  sentCount:       number;
  failedCount:     number;
  hangfireJobId:   string | null;
  batchSize:       number;
  scheduledAt:     string | null;
  startedAt:       string | null;
  completedAt:     string | null;
  failureReason:   string | null;
  createdBy:       string;
  createdAt:       string;
  updatedAt:       string;
  progressPercent: number;
}

// ── Paged response ────────────────────────────────────────────────────────────

export interface BroadcastPagedResult {
  items:      BroadcastListItem[];
  totalCount: number;
  page:       number;
  pageSize:   number;
  totalPages: number;
}

// ── Request DTOs — integers sent to backend ───────────────────────────────────

export interface CreateBroadcastRequestDto {
  name:      string;
  channel:   number;   // 0=Email 1=Sms 2=PushNotification
  subject:   string | null;
  message:   string;
  audience:  number;   // 0=Patients 1=Doctors 2=Admins 3=All
  sendNow:   boolean;
  batchSize: number;
}

export interface UpdateBroadcastRequestDto {
  name:     string;
  channel:  number;
  subject:  string | null;
  message:  string;
  audience: number;
}

export interface GetBroadcastsRequestDto {
  search?:    string;
  pageNumber: number;
  pageSize:   number;
}

// ── Action responses ──────────────────────────────────────────────────────────

export interface BroadcastSendResponse {
  message:         string;
  totalRecipients: number;
}

export interface BroadcastRetryResponse {
  message:     string;
  failedCount: number;
}

export interface AudienceCountResponse {
  audience:        BroadcastAudience;
  audienceDisplay: string;
  totalRecipients: number;
}