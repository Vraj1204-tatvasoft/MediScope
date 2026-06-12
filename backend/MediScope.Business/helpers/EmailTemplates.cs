// File: MediScope.Business/Helpers/EmailTemplates.cs

namespace MediScope.Business.Helpers
{
  /// <summary>
  /// Centralized HTML email templates for all MediScope notifications.
  /// All templates share a consistent brand design.
  /// Usage: EmailTemplates.DoctorWelcome(name, email, password)
  /// </summary>
  public static class EmailTemplates
  {
    // ── Brand colors ──────────────────────────────────────────────
    private const string PRIMARY = "#2563eb";
    private const string DARK_BG = "#0d1b3e";
    private const string LIGHT_BG = "#f3f4f6";
    private const string TEXT_DARK = "#111827";
    private const string TEXT_GRAY = "#6b7280";
    private const string SUCCESS = "#16a34a";
    private const string WARNING = "#d97706";

    // ═════════════════════════════════════════════════════════════
    // 1. DOCTOR WELCOME — sent when admin creates a doctor account
    // ═════════════════════════════════════════════════════════════
    public static string DoctorWelcome(
        string doctorName,
        string email,
        string temporaryPassword)
    {
      return Wrap(
          preheader: "Your MediScope doctor account has been created.",
          content: $@"
                <div style='text-align:center; margin-bottom:32px;'>
                  <div style='display:inline-block; background:{PRIMARY}; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                    <span style='color:white; font-size:28px;'>👨‍⚕️</span>
                  </div>
                  <h1 style='color:{TEXT_DARK}; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                    Welcome to MediScope
                  </h1>
                  <p style='color:{TEXT_GRAY}; font-size:15px; margin:0;'>
                    Your doctor account has been created by the administrator.
                  </p>
                </div>

                <p style='color:{TEXT_DARK}; font-size:15px; margin:0 0 24px;'>
                  Hello <strong>Dr. {doctorName}</strong>,
                </p>

                <p style='color:{TEXT_GRAY}; font-size:14px; line-height:1.6; margin:0 0 24px;'>
                  You have been registered on the MediScope Health Management Platform.
                  Use the credentials below to log in and start managing your patients.
                </p>

                {CredentialBox(email, temporaryPassword)}

                {AlertBox(
              "⚠️ Security Notice",
              "This is a temporary password. You will be required to change it on your first login. Do not share these credentials with anyone.",
              WARNING)}

                {PrimaryButton("Log In to MediScope", "http://localhost:4200/login")}

                <div style='border-top:1px solid #e5e7eb; margin:32px 0 24px;'></div>

                <p style='color:{TEXT_GRAY}; font-size:13px; line-height:1.6; margin:0;'>
                  If you did not expect this email or believe this was sent in error,
                  please contact your hospital administrator immediately.
                </p>"
      );
    }

    // ═════════════════════════════════════════════════════════════
    // 2. PATIENT WELCOME — sent after patient self-registration
    // ═════════════════════════════════════════════════════════════
    public static string PatientWelcome(string patientName, string email)
    {
      return Wrap(
          preheader: "Welcome to MediScope — your health management platform.",
          content: $@"
                <div style='text-align:center; margin-bottom:32px;'>
                  <div style='display:inline-block; background:{SUCCESS}; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                    <span style='color:white; font-size:28px;'>🏥</span>
                  </div>
                  <h1 style='color:{TEXT_DARK}; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                    Account Created Successfully
                  </h1>
                  <p style='color:{TEXT_GRAY}; font-size:15px; margin:0;'>
                    Welcome to your personal health dashboard.
                  </p>
                </div>

                <p style='color:{TEXT_DARK}; font-size:15px; margin:0 0 24px;'>
                  Hello <strong>{patientName}</strong>,
                </p>

                <p style='color:{TEXT_GRAY}; font-size:14px; line-height:1.6; margin:0 0 24px;'>
                  Your MediScope account has been created. You can now:
                </p>

                {FeatureList(new[]
          {
                    "Track BP, Heart Rate, Glucose & other health metrics",
                    "Connect with your assigned doctors securely",
                    "Receive real-time alerts for abnormal readings",
                    "View your complete health history and trends",
          })}

                {InfoBox("Registered Email", email)}

                {PrimaryButton("Go to Your Dashboard", "http://localhost:4200/login")}

                <div style='border-top:1px solid #e5e7eb; margin:32px 0 24px;'></div>

                <p style='color:{TEXT_GRAY}; font-size:13px; line-height:1.6; margin:0;'>
                  Your health data is stored securely and shared only with doctors
                  you have explicitly consented to.
                </p>"
      );
    }

    // ═════════════════════════════════════════════════════════════
    // 3. PASSWORD CHANGED — sent after successful password change
    // ═════════════════════════════════════════════════════════════
    public static string PasswordChanged(string fullName, DateTime changedAt)
    {
      return Wrap(
          preheader: "Your MediScope password was changed successfully.",
          content: $@"
                <div style='text-align:center; margin-bottom:32px;'>
                  <div style='display:inline-block; background:{SUCCESS}; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                    <span style='color:white; font-size:28px;'>🔒</span>
                  </div>
                  <h1 style='color:{TEXT_DARK}; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                    Password Changed
                  </h1>
                  <p style='color:{TEXT_GRAY}; font-size:15px; margin:0;'>
                    Your account password was updated successfully.
                  </p>
                </div>

                <p style='color:{TEXT_DARK}; font-size:15px; margin:0 0 24px;'>
                  Hello <strong>{fullName}</strong>,
                </p>

                <p style='color:{TEXT_GRAY}; font-size:14px; line-height:1.6; margin:0 0 24px;'>
                  Your MediScope account password was changed on
                  <strong>{changedAt:dddd, MMMM dd, yyyy}</strong> at
                  <strong>{changedAt:hh:mm tt} UTC</strong>.
                </p>

                {AlertBox(
              "⚠️ Wasn't you?",
              "If you did not make this change, your account may be compromised. Please reset your password immediately and contact support.",
              WARNING)}

                {PrimaryButton("Log In Again", "http://localhost:4200/login")}"
      );
    }

    // ═════════════════════════════════════════════════════════════
    // 4. DOCTOR ASSIGNED TO PATIENT — sent to patient
    // ═════════════════════════════════════════════════════════════
    public static string DoctorAssigned(
        string patientName,
        string doctorName,
        string specialization)
    {
      return Wrap(
          preheader: $"Dr. {doctorName} has been assigned to your care.",
          content: $@"
                <div style='text-align:center; margin-bottom:32px;'>
                  <div style='display:inline-block; background:{PRIMARY}; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                    <span style='color:white; font-size:28px;'>🩺</span>
                  </div>
                  <h1 style='color:{TEXT_DARK}; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                    New Doctor Assigned
                  </h1>
                  <p style='color:{TEXT_GRAY}; font-size:15px; margin:0;'>
                    A doctor has been assigned to your care team.
                  </p>
                </div>

                <p style='color:{TEXT_DARK}; font-size:15px; margin:0 0 24px;'>
                  Hello <strong>{patientName}</strong>,
                </p>

                <p style='color:{TEXT_GRAY}; font-size:14px; line-height:1.6; margin:0 0 24px;'>
                  The following doctor has been assigned to monitor your health records:
                </p>

                {InfoBox($"Dr. {doctorName}", specialization)}

                {AlertBox(
              "📋 What this means",
              "This doctor will now have access to your health metrics and records based on your consent settings. You can manage what they can see from your Profile → Data Privacy settings.",
              PRIMARY)}

                {PrimaryButton("Manage Data Privacy", "http://localhost:4200/patient/profile")}"
      );
    }
    public static string PendingRequestReminder(
    string doctorName,
    string patientName,
    string frontendUrl)
    {
      return Wrap(
          preheader: $"Daily Reminder: Pending patient assignment for {patientName}.",
          content: $@"
                <div style='text-align:center; margin-bottom:32px;'>
                  <div style='display:inline-block; background:{WARNING}; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                    <span style='color:white; font-size:28px;'>⏳</span>
                  </div>
                  <h1 style='color:{TEXT_DARK}; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                    Action Required — Pending Assignment
                  </h1>
                  <p style='color:{TEXT_GRAY}; font-size:15px; margin:0;'>
                    A patient care request is awaiting your review.
                  </p>
                </div>

                <p style='color:{TEXT_DARK}; font-size:15px; margin:0 0 24px;'>
                  Hello <strong>Dr. {doctorName}</strong>,
                </p>

                <p style='color:{TEXT_GRAY}; font-size:14px; line-height:1.6; margin:0 0 24px;'>
                  This is an automated reminder that the care request for patient <strong>{patientName}</strong> has been pending your approval for over 24 hours.
                </p>

                {AlertBox(
                  "📋 Next Steps",
                  "Please sign in to your dashboard to review this patient's intake profiles and officially accept or decline the assignment.",
                  PRIMARY)}

                {PrimaryButton("Go to Dashboard", $"{frontendUrl}/login")}"
      );
    }
    // ═════════════════════════════════════════════════════════════
    // 5. HEALTH ALERT — sent when a metric is out of range
    // ═════════════════════════════════════════════════════════════
    public static string HealthAlert(
        string patientName,
        string metricType,
        string value,
        string unit,
        string severity,
        string normalRange)
    {
      var severityColor = severity.ToLower() switch
      {
        "high" => "#dc2626",
        "medium" => WARNING,
        _ => SUCCESS
      };

      return Wrap(
          preheader: $"Health alert: Your {metricType} reading requires attention.",
          content: $@"
                <div style='text-align:center; margin-bottom:32px;'>
                  <div style='display:inline-block; background:{severityColor}; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                    <span style='color:white; font-size:28px;'>⚠️</span>
                  </div>
                  <h1 style='color:{TEXT_DARK}; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                    Health Alert — {severity.ToUpper()} Severity
                  </h1>
                  <p style='color:{TEXT_GRAY}; font-size:15px; margin:0;'>
                    An abnormal reading has been detected.
                  </p>
                </div>

                <p style='color:{TEXT_DARK}; font-size:15px; margin:0 0 24px;'>
                  Hello <strong>{patientName}</strong>,
                </p>

                <div style='background:{LIGHT_BG}; border-radius:12px; padding:24px; margin:0 0 24px; text-align:center;'>
                  <p style='color:{TEXT_GRAY}; font-size:13px; text-transform:uppercase; letter-spacing:0.08em; margin:0 0 8px;'>{metricType}</p>
                  <p style='color:{severityColor}; font-size:36px; font-weight:800; margin:0 0 4px;'>{value} <span style='font-size:18px;'>{unit}</span></p>
                  <p style='color:{TEXT_GRAY}; font-size:13px; margin:0;'>Normal range: {normalRange} {unit}</p>
                </div>

                {AlertBox(
              "What should you do?",
              "Please consult with your assigned doctor as soon as possible. If you are experiencing symptoms, seek medical attention immediately.",
              severityColor)}

                {PrimaryButton("View Health Dashboard", "http://localhost:4200/patient/dashboard")}"
      );
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE LAYOUT HELPERS
    // ═══════════════════════════════════════════════════════════════
    // FILE: MediScope.Business/Helpers/EmailTemplates.cs
    // ADD this method to the existing EmailTemplates static class

    public static string PasswordReset(
        string fullName,
        string resetLink,
        int expiryMinutes = 30)
    {
      return Wrap(
          preheader: "Reset your MediScope password.",
          content: $@"
            <div style='text-align:center; margin-bottom:32px;'>
              <div style='display:inline-block; background:#2563eb; border-radius:50%; width:64px; height:64px; line-height:64px; text-align:center;'>
                <span style='color:white; font-size:28px;'>🔑</span>
              </div>
              <h1 style='color:#111827; font-size:24px; font-weight:700; margin:16px 0 8px;'>
                Reset Your Password
              </h1>
              <p style='color:#6b7280; font-size:15px; margin:0;'>
                We received a request to reset your password.
              </p>
            </div>
    
            <p style='color:#111827; font-size:15px; margin:0 0 24px;'>
              Hello <strong>{fullName}</strong>,
            </p>
    
            <p style='color:#6b7280; font-size:14px; line-height:1.6; margin:0 0 24px;'>
              Click the button below to set a new password for your MediScope account.
              This link will expire in <strong>{expiryMinutes} minutes</strong>.
            </p>
    
            {PrimaryButton("Reset My Password", resetLink)}
    
            {AlertBox(
              "⚠️ Didn't request this?",
              "If you did not request a password reset, you can safely ignore this email. Your password will remain unchanged.",
              WARNING)}
    
            <div style='border-top:1px solid #e5e7eb; margin:32px 0 24px;'></div>
    
            <p style='color:#6b7280; font-size:12px; line-height:1.6; margin:0;'>
              If the button above does not work, copy and paste this link into your browser:<br/>
              <a href='{resetLink}' style='color:#2563eb; word-break:break-all;'>{resetLink}</a>
            </p>"
      );
    }
    /// <summary>Master email layout wrapper — all templates use this</summary>
    private static string Wrap(string preheader, string content)
    {
      return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
  <title>MediScope</title>
</head>
<body style='margin:0; padding:0; background:{LIGHT_BG}; font-family:""Segoe UI"", Roboto, Arial, sans-serif;'>

  <!-- Preheader (hidden preview text) -->
  <div style='display:none; max-height:0; overflow:hidden; color:{LIGHT_BG};'>
    {preheader}
  </div>

  <!-- Email wrapper -->
  <table width='100%' cellpadding='0' cellspacing='0' border='0'
         style='background:{LIGHT_BG}; padding:40px 16px;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' border='0'
               style='max-width:600px; width:100%;'>

          <!-- HEADER -->
          <tr>
            <td style='background:{DARK_BG}; border-radius:12px 12px 0 0; padding:28px 40px;'>
              <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr>
                  <td>
                    <table cellpadding='0' cellspacing='0' border='0'>
                      <tr>
                        <td style='background:{PRIMARY}; border-radius:8px; width:36px; height:36px; text-align:center; vertical-align:middle;'>
                          <span style='color:white; font-size:18px; line-height:36px;'>⚡</span>
                        </td>
                        <td style='padding-left:10px;'>
                          <span style='color:white; font-size:18px; font-weight:700; vertical-align:middle;'>MediScope</span>
                        </td>
                      </tr>
                    </table>
                  </td>
                  <td align='right'>
                    <span style='color:rgba(255,255,255,0.45); font-size:12px;'>Health Management Platform</span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- BODY -->
          <tr>
            <td style='background:white; padding:40px; border-left:1px solid #e5e7eb; border-right:1px solid #e5e7eb;'>
              {content}
            </td>
          </tr>

          <!-- FOOTER -->
          <tr>
            <td style='background:{LIGHT_BG}; border:1px solid #e5e7eb; border-top:none; border-radius:0 0 12px 12px; padding:24px 40px; text-align:center;'>
              <p style='color:{TEXT_GRAY}; font-size:12px; margin:0 0 8px;'>
                © {DateTime.UtcNow.Year} MediScope. All rights reserved.
              </p>
              <p style='color:{TEXT_GRAY}; font-size:12px; margin:0;'>
                This is an automated message. Please do not reply to this email.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>

</body>
</html>";
    }

    /// <summary>Blue credential box — email + temp password</summary>
    private static string CredentialBox(string email, string password)
    {
      return $@"
            <div style='background:{LIGHT_BG}; border:1px solid #e5e7eb; border-radius:10px; padding:24px; margin:0 0 24px;'>
              <p style='color:{TEXT_GRAY}; font-size:12px; text-transform:uppercase; letter-spacing:0.08em; margin:0 0 16px; font-weight:600;'>
                YOUR LOGIN CREDENTIALS
              </p>
              <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                <tr>
                  <td style='padding:8px 0; border-bottom:1px solid #e5e7eb;'>
                    <span style='color:{TEXT_GRAY}; font-size:13px;'>Email</span>
                  </td>
                  <td align='right' style='padding:8px 0; border-bottom:1px solid #e5e7eb;'>
                    <span style='color:{TEXT_DARK}; font-size:13px; font-weight:600;'>{email}</span>
                  </td>
                </tr>
                <tr>
                  <td style='padding:8px 0;'>
                    <span style='color:{TEXT_GRAY}; font-size:13px;'>Temporary Password</span>
                  </td>
                  <td align='right' style='padding:8px 0;'>
                    <span style='background:{DARK_BG}; color:white; font-family:monospace; font-size:14px; font-weight:700; padding:4px 12px; border-radius:6px; letter-spacing:0.05em;'>
                      {password}
                    </span>
                  </td>
                </tr>
              </table>
            </div>";
    }

    /// <summary>Colored alert/notice box</summary>
    private static string AlertBox(string title, string message, string color)
    {
      var bg = HexToRgba(color, 0.08);
      var border = HexToRgba(color, 0.3);

      return $@"
            <div style='background:{bg}; border:1px solid {border}; border-left:4px solid {color}; border-radius:8px; padding:16px 20px; margin:0 0 24px;'>
              <p style='color:{color}; font-size:13px; font-weight:700; margin:0 0 6px;'>{title}</p>
              <p style='color:{TEXT_DARK}; font-size:13px; line-height:1.6; margin:0;'>{message}</p>
            </div>";
    }

    /// <summary>Single info key-value row</summary>
    private static string InfoBox(string label, string value)
    {
      return $@"
            <div style='background:{LIGHT_BG}; border:1px solid #e5e7eb; border-radius:8px; padding:16px 20px; margin:0 0 24px; display:flex; justify-content:space-between;'>
              <span style='color:{TEXT_GRAY}; font-size:13px;'>{label}</span>
              <span style='color:{TEXT_DARK}; font-size:13px; font-weight:600;'>{value}</span>
            </div>";
    }

    /// <summary>Feature bullet list</summary>
    private static string FeatureList(string[] items)
    {
      var rows = string.Join("", items.Select(item => $@"
            <tr>
              <td style='padding:6px 0;'>
                <table cellpadding='0' cellspacing='0' border='0'>
                  <tr>
                    <td style='width:20px; vertical-align:top;'>
                      <span style='color:{PRIMARY}; font-size:14px;'>•</span>
                    </td>
                    <td style='padding-left:8px;'>
                      <span style='color:{TEXT_GRAY}; font-size:14px; line-height:1.5;'>{item}</span>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>"));

      return $@"
            <table width='100%' cellpadding='0' cellspacing='0' border='0'
                   style='background:{LIGHT_BG}; border:1px solid #e5e7eb; border-radius:8px; padding:16px 20px; margin:0 0 24px;'>
              {rows}
            </table>";
    }

    /// <summary>Full-width primary CTA button</summary>
    private static string PrimaryButton(string text, string url)
    {
      return $@"
            <div style='text-align:center; margin:28px 0;'>
              <a href='{url}'
                 style='display:inline-block; background:{PRIMARY}; color:white; text-decoration:none;
                        font-size:15px; font-weight:600; padding:14px 40px; border-radius:8px;
                        letter-spacing:0.01em;'>
                {text}
              </a>
            </div>";
    }

    /// <summary>Convert hex color to rgba string for backgrounds/borders</summary>
    private static string HexToRgba(string hex, double alpha)
    {
      hex = hex.TrimStart('#');
      if (hex.Length == 6)
      {
        int r = Convert.ToInt32(hex[..2], 16);
        int g = Convert.ToInt32(hex[2..4], 16);
        int b = Convert.ToInt32(hex[4..6], 16);
        return $"rgba({r},{g},{b},{alpha})";
      }
      return hex;
    }
  }
}