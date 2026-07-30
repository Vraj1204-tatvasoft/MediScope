import { UserRole } from "../models/auth.model";


export interface NavItem {
  label: string;
  route?: string;
  icon:  string; // SVG path string
  children?: NavItem[];
}

export interface NavConfig {
  portalLabel: string;
  accessLabel: string;
  items:        NavItem[];
}

// ── SVG icon paths ────────────────────────────────────────────
const ICONS = {
  dashboard: `<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>`,
  patients:  `<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>`,
  doctors:   `<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>`,
  metrics:   `<polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/>`,
  alerts:    `<path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/>`,
  profile:   `<path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>`,
  users:     `<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>`,
  analytics: `<line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/>`,
  settings:  `<circle cx="12" cy="12" r="3"/><path d="M19.07 4.93a10 10 0 0 1 0 14.14M4.93 4.93a10 10 0 0 0 0 14.14"/>`,
  records:   `<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>`,
};

// ── Nav configs per role ──────────────────────────────────────
export const NAV_CONFIG: Record<UserRole, NavConfig> = {

  Doctor: {
    portalLabel: 'Doctor Portal',
    accessLabel: 'Doctor Access',
    items: [
      { label: 'Dashboard',     route: '/doctor/dashboard',     icon: ICONS.dashboard },
      { label: 'My Patients',   route: '/doctor/my-patients',      icon: ICONS.patients  },
      { label: 'Notifications', route: '/doctor/doctor-notifications', icon: ICONS.alerts    },
      { label: 'Pending Requests', route: '/doctor/pending-requests', icon: ICONS.alerts    },
      { label: 'Appointments', route: '/doctor/appointments', icon: ICONS.metrics},
      { label: 'Invoices', route: '/doctor/doctor-invoices', icon: ICONS.metrics},
      { label: 'Profile',       route: '/doctor/profile',       icon: ICONS.profile   },
    ],
  },

  Patient: {
    portalLabel: 'Patient Portal',
    accessLabel: 'Patient Access',
    items: [
      { label: 'Dashboard',     route: '/patient/dashboard',    icon: ICONS.dashboard },
      { label: 'Add Health Data',    route: '/patient/add-health-data',      icon: ICONS.metrics   },
      { label: 'Health History',    route: '/patient/health-history',      icon: ICONS.doctors   },
      { label: 'My Doctors',route: '/patient/my-doctors',      icon: ICONS.records   },
      { label: 'Notifications', route: '/patient/patient-notifications',icon: ICONS.alerts    },
      { label: 'Appointments', route: '/patient/appointments', icon: ICONS.metrics},
      {label:'Invoices', route:'/patient/invoices', icon: ICONS.analytics},
      {label:'Questionnaire', route:'/patient/patient-questionnaire-list', icon: ICONS.metrics},
      { label: 'Profile',       route: '/patient/profile',      icon: ICONS.profile   },
    ],
  },

  Admin: {
    portalLabel: 'Admin Portal',
    accessLabel: 'Admin Access',
    items: [
      { label: 'Dashboard',     route: '/admin/dashboard',      icon: ICONS.dashboard },
      { label: 'Doctors',         route: '/admin/manage-doctors',          icon: ICONS.users     },
      { label: 'Patients',       route: '/admin/manage-patients',        icon: ICONS.users   },
      { label: 'Metrics',     route: '/admin/manage-metrics',    icon: ICONS.metrics},
      { label: 'Doctor-Patient',     route: '/admin/doctor-patient-links',      icon: ICONS.analytics },
      {
        label: 'Hospitalization',
        icon: ICONS.records,
        children: [
          {
            label: 'Hospital Dashboard',
            route: '/admin/hospital-dashboard',
            icon: ICONS.dashboard
          },
          {
            label: 'Rooms',
            route: '/admin/manage-rooms',
            icon: ICONS.records
          },
          {
            label: 'Admissions',
            route: '/admin/admissions',
            icon: ICONS.records
          }
        ]
      },
      {label: 'Questionnaire', route: '/admin/admin-questionnaire', icon: ICONS.analytics}
    ]
  },
  

};