# Automation API Contract Reference

## Base URL
```
https://your-api.com/api/automation
```

## Authentication
All endpoints require JWT Bearer token in Authorization header:
```
Authorization: Bearer {jwt_token}
```

---

## Endpoints

### 1. Get Automation Settings

**Endpoint**: `GET /settings`

**Description**: Retrieve current automation preferences for authenticated user

**Request**:
```http
GET /api/automation/settings HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Response (200 OK)**:
```json
{
  "enabled": true,
  "dailyApplicationTarget": 5,
  "minimumMatchScore": 70.0,
  "autoTailorResume": true,
  "preferredResumeId": "guid-or-null"
}
```

**Response (404 Not Found)**:
```json
{
  "error": "Automation settings not found"
}
```

---

### 2. Update Automation Settings

**Endpoint**: `PUT /settings`

**Description**: Update automation preferences for authenticated user

**Request**:
```http
PUT /api/automation/settings HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "enabled": true,
  "dailyApplicationTarget": 10,
  "minimumMatchScore": 75.0,
  "autoTailorResume": true,
  "preferredResumeId": null
}
```

**Request Body**:
| Field | Type | Required | Default | Notes |
|-------|------|----------|---------|-------|
| enabled | bool | Yes | - | Enable/disable automation |
| dailyApplicationTarget | int | No | 5 | Jobs to apply to per day |
| minimumMatchScore | double | No | 70.0 | Min score to auto-apply |
| autoTailorResume | bool | No | true | Tailor resume per job |
| preferredResumeId | Guid? | No | null | Specific resume or auto-select |

**Response (200 OK)**:
```json
{
  "enabled": true,
  "dailyApplicationTarget": 10,
  "minimumMatchScore": 75.0,
  "autoTailorResume": true,
  "preferredResumeId": null
}
```

**Response (400 Bad Request)**:
```json
{
  "error": "Failed to update automation settings"
}
```

---

### 3. Enable Automation

**Endpoint**: `POST /enable`

**Description**: Enable automation for authenticated user (shortcut)

**Request**:
```http
POST /api/automation/enable HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Length: 0
```

**Response (200 OK)**:
```json
{
  "enabled": true,
  "dailyApplicationTarget": 5,
  "minimumMatchScore": 70.0,
  "autoTailorResume": true,
  "preferredResumeId": null
}
```

**Response (400 Bad Request)**:
```json
{
  "error": "Failed to enable automation"
}
```

---

### 4. Disable Automation

**Endpoint**: `POST /disable`

**Description**: Disable automation for authenticated user (shortcut)

**Request**:
```http
POST /api/automation/disable HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Length: 0
```

**Response (200 OK)**:
```json
{
  "enabled": false,
  "dailyApplicationTarget": 5,
  "minimumMatchScore": 70.0,
  "autoTailorResume": true,
  "preferredResumeId": null
}
```

**Response (400 Bad Request)**:
```json
{
  "error": "Failed to disable automation"
}
```

---

### 5. Manually Trigger Job Applications

**Endpoint**: `POST /apply`

**Description**: Manually trigger automated job applications (for testing)

**Request**:
```http
POST /api/automation/apply HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Length: 0
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Automation completed. Applied to 3 jobs.",
  "applicationsSubmitted": 3,
  "applications": [
	{
	  "jobListingId": "guid-1",
	  "jobTitle": "Senior DevOps Engineer",
	  "company": "Tech Corp",
	  "matchScore": 92.5,
	  "applied": true,
	  "reason": null,
	  "applicationId": "app-guid-1"
	},
	{
	  "jobListingId": "guid-2",
	  "jobTitle": "Cloud Architect",
	  "company": "Cloud Systems",
	  "matchScore": 88.3,
	  "applied": true,
	  "reason": null,
	  "applicationId": "app-guid-2"
	},
	{
	  "jobListingId": "guid-3",
	  "jobTitle": "Infrastructure Engineer",
	  "company": "InfraCorp",
	  "matchScore": 85.0,
	  "applied": true,
	  "reason": null,
	  "applicationId": "app-guid-3"
	}
  ],
  "executedAt": "2025-01-15T12:00:00Z",
  "error": null
}
```

**Response (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Automation is disabled for this user",
  "error": "Automation disabled"
}
```

**Response with warnings**:
```json
{
  "success": true,
  "message": "Automation completed. Applied to 1 job.",
  "applicationsSubmitted": 1,
  "applications": [
	{
	  "jobListingId": "guid-1",
	  "jobTitle": "Senior DevOps Engineer",
	  "company": "Tech Corp",
	  "matchScore": 92.5,
	  "applied": true,
	  "applicationId": "app-guid-1"
	},
	{
	  "jobListingId": "guid-2",
	  "jobTitle": "Platform Engineer",
	  "company": "Platform Inc",
	  "matchScore": 55.0,
	  "applied": false,
	  "reason": "Match score 55.0 below threshold 70.0"
	},
	{
	  "jobListingId": "guid-3",
	  "jobTitle": "DevOps Manager",
	  "company": "Manager Co",
	  "matchScore": 0.0,
	  "applied": false,
	  "reason": "No match score found"
	}
  ],
  "executedAt": "2025-01-15T12:00:00Z",
  "error": null
}
```

---

### 6. Manually Trigger Resume Upload

**Endpoint**: `POST /upload-resume`

**Description**: Manually trigger automated resume upload (for testing)

**Request**:
```http
POST /api/automation/upload-resume HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Length: 0
```

**Response (200 OK)**:
```json
{
  "message": "Resume upload automation executed successfully"
}
```

**Response (400 Bad Request)**:
```json
{
  "message": "Failed to execute resume upload automation"
}
```

---

## Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "traceId": "..."
}
```
**Cause**: Missing or invalid JWT token

### 404 Not Found
```json
{
  "error": "Automation settings not found"
}
```
**Cause**: User not found in database

### 429 Too Many Requests
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.9",
  "title": "Too Many Requests",
  "status": 429
}
```
**Cause**: Rate limit exceeded (100 requests/minute per user)

---

## Data Models

### AutomationSettingsResponse
```typescript
{
  enabled: boolean;
  dailyApplicationTarget: number;
  minimumMatchScore: number;
  autoTailorResume: boolean;
  preferredResumeId: string | null;  // GUID format
}
```

### UpdateAutomationSettingsRequest
```typescript
{
  enabled: boolean;
  dailyApplicationTarget?: number;
  minimumMatchScore?: double;
  autoTailorResume?: boolean;
  preferredResumeId?: string | null;  // GUID format
}
```

### AutomationRunResponse
```typescript
{
  success: boolean;
  message: string;
  applicationsSubmitted: number;
  applications: ApplicationResultDto[];
  executedAt: string;  // ISO 8601 format
  error?: string;
}
```

### ApplicationResultDto
```typescript
{
  jobListingId: string;      // GUID format
  jobTitle: string;
  company: string;
  matchScore: number;        // 0-100
  applied: boolean;
  reason?: string;           // why not applied, if applicable
  applicationId?: string;    // GUID format, if applied
}
```

### MessageResponse
```typescript
{
  message: string;
}
```

---

## Example Workflows

### Workflow 1: Get Current Settings
```bash
# Get settings
curl -X GET https://api.example.com/api/automation/settings \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Workflow 2: Enable and Test
```bash
# Enable automation
curl -X POST https://api.example.com/api/automation/enable \
  -H "Authorization: Bearer YOUR_TOKEN"

# Wait a moment...

# Manually test applications
curl -X POST https://api.example.com/api/automation/apply \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Workflow 3: Custom Configuration
```bash
# Update settings
curl -X PUT https://api.example.com/api/automation/settings \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
	"enabled": true,
	"dailyApplicationTarget": 10,
	"minimumMatchScore": 75.0,
	"autoTailorResume": true,
	"preferredResumeId": null
  }'

# Verify changes
curl -X GET https://api.example.com/api/automation/settings \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Rate Limits

- **Global Limit**: 100 requests/minute per user
- **Automation Endpoint Limit**: Included in global limit
- **Response Header**: `X-RateLimit-Remaining: 95`

---

## Pagination & Filtering

Not currently supported on automation endpoints. All settings are user-specific and singular.

---

## Versioning

- **Current Version**: v1
- **Deprecation Policy**: 6-month notice before removal
- **URL Format**: `/api/automation` (future: `/api/v1/automation`)

---

## Implementation Notes

1. **User ID Extraction**: User ID derived from JWT token claims
2. **Time Zone**: All scheduled times are UTC (6:00 AM and 12:00 PM)
3. **Idempotency**: Most endpoints are idempotent
4. **Async Operations**: Background tasks run asynchronously
5. **Consistency**: Settings applied immediately; scheduler applies next run

---

## Support

For issues or questions about this API:
1. Check `AUTOMATION_FEATURE.md` for detailed documentation
2. Review `AUTOMATION_QUICK_START.md` for examples
3. See troubleshooting section in implementation summary

---

**Last Updated**: January 2025  
**API Version**: 1.0  
**Status**: Production Ready
