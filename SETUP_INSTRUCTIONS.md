# SmartClinic System Setup and Verification Instructions

## Overview
This document provides step-by-step instructions to set up, run, and verify the SmartClinic modular monolith system with Keycloak authentication, Docker services, and Transactional Outbox pattern.

## Prerequisites
- Docker Desktop installed and running
- .NET 10.0 SDK installed
- Postman or similar API testing tool (optional, Swagger UI is available)

---

## Step 1: Start Docker Services

### 1.1 Navigate to Project Root
```powershell
cd C:\Users\ms\OneDrive\Documents\Clinic-System
```

### 1.2 Start Docker Compose Services
```powershell
docker-compose up -d
```

This will start:
- **PostgreSQL** on port `5432`
- **RabbitMQ** on ports `5672` (AMQP) and `15672` (Management UI)
- **Keycloak** on port `8080`

### 1.3 Verify Services are Running
```powershell
docker-compose ps
```

All services should show status "Up". Wait 30-60 seconds for Keycloak to fully initialize.

### 1.4 Check Keycloak Logs (Optional)
```powershell
docker logs clinic_keycloak -f
```

Look for messages indicating:
- Realm import successful
- Server started
- Admin console available

---

## Step 2: Access Keycloak Admin Console

### 2.1 Open Keycloak Admin Console
1. Open your browser
2. Navigate to: **http://localhost:8080/admin/**
3. Login with:
   - **Username:** `admin`
   - **Password:** `admin`

### 2.2 Verify Realm Import
1. In the Keycloak admin console, you should see the **"clinic-realm"** in the realm dropdown (top-left)
2. Select **"clinic-realm"** from the dropdown
3. Navigate to **Users** → **View all users**
4. Verify the following users exist:
   - `admin` (email: admin@clinic.com)
   - `doctor` (email: doctor@clinic.com)
   - `patient` (email: patient@clinic.com)

### 2.3 Verify Client Configuration
1. In **clinic-realm**, navigate to **Clients**
2. Verify **"clinic-client"** exists and is enabled
3. Check that:
   - **Access Type:** Public
   - **Direct Access Grants Enabled:** ON
   - **Standard Flow Enabled:** ON

---

## Step 3: Build and Run .NET Applications

### 3.1 Build the Solution
```powershell
dotnet build SmartClinic.sln
```

Expected: Build succeeded with warnings (no errors)

### 3.2 Run API Services

Open **three separate terminal windows** and run each API:

#### Terminal 1 - Patient Management API (Public, No Auth)
```powershell
cd src\Modules\PatientManagement\SmartClinic.PatientManagement.Api
dotnet run
```
**Expected:** API running on `http://localhost:5000` or `https://localhost:5001`
**Swagger UI:** Available at `/swagger`

#### Terminal 2 - Appointment Scheduling API (Requires JWT)
```powershell
cd src\Modules\AppointmentScheduling\SmartClinic.AppointmentScheduling.Api
dotnet run
```
**Expected:** API running on `http://localhost:5002` or `https://localhost:5003`
**Swagger UI:** Available at `/swagger`

#### Terminal 3 - Prescription Management API (Requires JWT)
```powershell
cd src\Modules\PrescriptionManagement\SmartClinic.PrescriptionManagement.Api
dotnet run
```
**Expected:** API running on `http://localhost:5004` or `https://localhost:5005`
**Swagger UI:** Available at `/swagger`

**Note:** The actual ports may vary. Check the console output for the exact URLs.

### 3.3 Run Background Job Service (Outbox Publisher)

Open a **fourth terminal window**:

```powershell
cd src\Infrastructure\SmartClinic.Infrastructure.Host
dotnet run
```

**Expected:** 
- Service starts and logs "Starting outbox publishing job" every 5 seconds
- If there are no unprocessed messages, it will log "Found 0 unprocessed messages"

---

## Step 4: Test API Endpoints

### 4.1 Get JWT Token from Keycloak

#### Option A: Using Keycloak UI
1. Navigate to: **http://localhost:8080/realms/clinic-realm/account**
2. Login with any user (e.g., `doctor` / `doctor`)
3. Go to **Account Console** → **Security** → **Sessions** to view tokens

#### Option B: Using Postman/curl (Recommended)

**Get Token for "doctor" user:**
```powershell
$body = @{
    grant_type = "password"
    client_id = "clinic-client"
    username = "doctor"
    password = "doctor"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:8080/realms/clinic-realm/protocol/openid-connect/token" -Method Post -Body $body -ContentType "application/json"
$token = $response.access_token
Write-Host "Token: $token"
```

**Get Token for "patient" user:**
```powershell
$body = @{
    grant_type = "password"
    client_id = "clinic-client"
    username = "patient"
    password = "patient"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:8080/realms/clinic-realm/protocol/openid-connect/token" -Method Post -Body $body -ContentType "application/json"
$token = $response.access_token
Write-Host "Token: $token"
```

### 4.2 Test Patient Management API (Public Endpoint)

**Endpoint:** `POST /patients`

**Using PowerShell:**
```powershell
$body = @{
    FullName = "John Doe"
    Email = "john.doe@example.com"
    DateOfBirth = "1990-01-15T00:00:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/patients" -Method Post -Body $body -ContentType "application/json"
```

**Expected Response:**
```json
{
  "id": "guid-here"
}
```

**Note:** This endpoint is **public** and does not require authentication.

### 4.3 Test Appointment Scheduling API (Protected Endpoint)

**Endpoint:** `POST /appointments`

**Using PowerShell (with JWT token):**
```powershell
$token = "YOUR_JWT_TOKEN_HERE"  # Replace with actual token from Step 4.1

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    PatientId = "123e4567-e89b-12d3-a456-426614174000"
    AppointmentDate = "2024-12-20T10:00:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5002/appointments" -Method Post -Body $body -Headers $headers
```

**Expected Response:**
```json
{
  "id": "guid-here"
}
```

**Note:** This endpoint **requires JWT authentication**. Without a valid token, you'll get `401 Unauthorized`.

### 4.4 Test Prescription Management API (Protected Endpoint)

**Endpoint:** `POST /prescriptions`

**Using PowerShell (with JWT token):**
```powershell
$token = "YOUR_JWT_TOKEN_HERE"  # Replace with actual token from Step 4.1

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    AppointmentId = "123e4567-e89b-12d3-a456-426614174000"
    Medications = @(
        @{
            MedicationName = "Aspirin"
            Dosage = "100mg"
            Quantity = 30
        }
    )
    Notes = "Take with food"
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5004/prescriptions" -Method Post -Body $body -Headers $headers
```

**Expected Response:**
```json
{
  "id": "guid-here"
}
```

**Note:** This endpoint **requires JWT authentication**.

### 4.5 Test Using Swagger UI

1. Open Swagger UI for any API (e.g., `http://localhost:5000/swagger`)
2. For protected endpoints:
   - Click **"Authorize"** button
   - Enter: `Bearer YOUR_JWT_TOKEN_HERE`
   - Click **"Authorize"**
3. Test endpoints using the Swagger UI interface

---

## Step 5: Verify Transactional Outbox Pattern

### 5.1 Understanding the Outbox Pattern

The system uses the Transactional Outbox pattern:
- When domain events occur (e.g., patient registered, appointment booked), they are written to an `OutboxMessages` table in the same database transaction
- A background job (Quartz) runs every 5 seconds to:
  - Read unprocessed messages from all three module outboxes
  - Publish them to RabbitMQ
  - Mark them as processed

### 5.2 Verify Outbox Messages are Created

**Note:** Since the APIs use InMemory databases, outbox messages are stored in memory and will be lost when the API stops. To verify the pattern:

1. **Create a patient** (Step 4.2)
2. **Check the background job logs** (Terminal 4) - you should see:
   ```
   Starting outbox publishing job.
   Found X unprocessed messages.
   Published and marked processed message {MessageId}.
   ```

### 5.3 Verify RabbitMQ Messages

1. Open RabbitMQ Management UI: **http://localhost:15672**
2. Login with:
   - **Username:** `guest`
   - **Password:** `guest`
3. Navigate to **Exchanges**
4. Look for exchange: **`smartclinic.events`**
5. Check message counts and routing

### 5.4 Verify Background Job is Running

In the background job service terminal (Terminal 4), you should see logs every 5 seconds:
```
Starting outbox publishing job.
Found 0 unprocessed messages.
Completed outbox publishing job.
```

If messages exist, you'll see:
```
Found 2 unprocessed messages.
Published and marked processed message {guid}.
Published and marked processed message {guid}.
```

---

## Step 6: System Verification Checklist

### ✅ Docker Services
- [ ] PostgreSQL is running on port 5432
- [ ] RabbitMQ is running on ports 5672 and 15672
- [ ] Keycloak is running on port 8080
- [ ] All services show "Up" status in `docker-compose ps`

### ✅ Keycloak
- [ ] Admin console accessible at http://localhost:8080/admin/
- [ ] Can login with admin/admin
- [ ] "clinic-realm" is imported and visible
- [ ] Users exist: admin, doctor, patient
- [ ] "clinic-client" is configured correctly

### ✅ API Endpoints
- [ ] Patient Management API is running (public endpoint works)
- [ ] Appointment Scheduling API is running (protected endpoint requires JWT)
- [ ] Prescription Management API is running (protected endpoint requires JWT)
- [ ] Swagger UI is accessible for all APIs

### ✅ Authentication
- [ ] Can get JWT token from Keycloak
- [ ] Public endpoint (POST /patients) works without token
- [ ] Protected endpoints (POST /appointments, POST /prescriptions) require JWT token
- [ ] Protected endpoints return 401 without token
- [ ] Protected endpoints work with valid JWT token

### ✅ Transactional Outbox
- [ ] Background job service is running
- [ ] Job executes every 5 seconds
- [ ] Outbox messages are created when domain events occur
- [ ] Messages are published to RabbitMQ
- [ ] Messages are marked as processed

---

## Troubleshooting

### Keycloak 404 Error
**Problem:** Cannot access Keycloak admin console

**Solutions:**
1. Wait 30-60 seconds after starting Docker services
2. Check Keycloak logs: `docker logs clinic_keycloak`
3. Verify port 8080 is not in use by another application
4. Try accessing: `http://localhost:8080` (without /admin/)

### Realm Not Imported
**Problem:** "clinic-realm" not visible in Keycloak

**Solutions:**
1. Check Keycloak logs for import errors
2. Verify `keycloak/import/realm-export.json` exists
3. Restart Keycloak: `docker-compose restart keycloak`
4. Manually import realm via Keycloak admin console if needed

### API Authentication Fails
**Problem:** Getting 401 Unauthorized with valid token

**Solutions:**
1. Verify token is not expired
2. Check API appsettings.json has correct Keycloak authority
3. Verify Keycloak is accessible from the API
4. Check token issuer matches: `http://localhost:8080/realms/clinic-realm`

### Background Job Not Running
**Problem:** No outbox messages being processed

**Solutions:**
1. Verify background job service is running
2. Check logs for errors
3. Verify RabbitMQ is accessible (connection string uses "localhost")
4. Check that DbContexts are properly registered

### Build Errors
**Problem:** Solution does not build

**Solutions:**
1. Run `dotnet restore` first
2. Check .NET SDK version (should be 10.0)
3. Review build output for specific errors
4. Ensure all NuGet packages are restored

---

## API Endpoints Summary

| Module | Endpoint | Method | Authentication | Port (Example) |
|--------|----------|--------|----------------|----------------|
| PatientManagement | `/patients` | POST | None (Public) | 5000 |
| AppointmentScheduling | `/appointments` | POST | JWT Required | 5002 |
| PrescriptionManagement | `/prescriptions` | POST | JWT Required | 5004 |

---

## Keycloak Users

| Username | Password | Email | Roles |
|----------|----------|-------|-------|
| admin | admin | admin@clinic.com | admin |
| doctor | doctor | doctor@clinic.com | doctor |
| patient | patient | patient@clinic.com | patient |

---

## Next Steps

1. **Integration Testing:** Create integration tests for all endpoints
2. **Database Migration:** Replace InMemory databases with PostgreSQL
3. **Production Configuration:** Update connection strings and security settings
4. **Monitoring:** Add logging and monitoring for production use
5. **Documentation:** Update API documentation with OpenAPI/Swagger

---

## Support

For issues or questions:
1. Check Docker logs: `docker logs <container_name>`
2. Check .NET application logs in console output
3. Verify all prerequisites are met
4. Review this document for troubleshooting steps

---

**Last Updated:** 2024-12-19
**System Version:** SmartClinic Modular Monolith v1.0
