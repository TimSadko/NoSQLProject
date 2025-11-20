# Overview of the project (one for the whole team)
# - Name - Student number
# - In this project I individually developed …
# - And within the team, I was in charge of …

# .........................................................................................

# Project Overview
# This project is a ticket tracking system built with ASP.NET Core MVC (.NET 8, C# 12) 
# and Razor views. It supports employees and service desk staff. Employees can view 
# and submit tickets, while service desk staff can manage and update them.
# The system uses MongoDB and implements repositories and services for clean, modular architecture.

# Main Features:
# - EmployeesController: create, edit, delete, and list employees
# - TicketsServiceDeskController: service desk ticket operations
# - TicketsEmployeeController: employee ticket management
# - TicketRequestsController: sent, received, and all ticket requests
# - HomeController: login, password flow, navigation
# - API endpoints under Controllers/Api: AuthController, TicketsController (with Basic Auth)

# Architecture:
# - Controllers return Razor views in Views/*
# - Services: EmployeeService, ServiceDeskEmployeeService, TicketRequestService
# - Repositories: IEmployeeRepository, ITicketRepository, ITicketRequestRepository
# - MongoDB configuration: IMongoClient (singleton), IMongoDatabase (scoped)
# - Class maps registered for Employee and ServiceDeskEmployee

# Configuration and Tooling:
# - Loads .env for Mongo:ConnectionString
# - Reads Mongo:Database from appsettings.json
# - Swagger configured for API documentation and Basic Authentication
# - Sessions enabled (30 min timeout)
# - Password hashing with salt
# - Default route: {controller=Home}/{action=Index}/{id?}

# =====================================================================
# Abdullah Sen - 735987
# =====================================================================

# Individual Task:
# I individually developed the RESTful API module: 
# "API server: Build an API server module with GET/POST/PUT/DELETE".
# 
# Short Overview:
# The API handles authentication and ticket management with clear routes, 
# role-based access, validation, and consistent status codes.

# Authentication and Roles:
# - Uses BasicAuthenticationHandler
# - Reads Authorization: Basic <base64(email:password)>
# - Validates hashed credentials
# - Requires Active status
# - Assigns ServiceDesk or Employee roles

# AuthController:
# POST /api/auth/basic 
# - Validates credentials
# - Returns Authorization header
# - Status codes: 400, 401, 500

# TicketsController:
# /api/tickets (requires Basic Auth)
# - Service desk users: see all tickets or filter by employeeId
# - Employees: see only their own tickets
# - Supports: create, read, update, delete
# - Status codes: 201, 204, 403, 404

# Swagger:
# - Documents endpoints
# - Supports authentication tests

# API Usage:
# 1. Call POST /api/auth/basic to receive Authorization header
# 2. Use Authorization header in GET /api/tickets

# Security and Validation:
# - Hashed passwords
# - Validates required fields
# - Enforces role-based access
# - Predictable HTTP responses

# Team Task:
# I was in charge of the employee ticket pages.

# Overview:
# - Implemented in TicketsEmployeeController
# - Razor views in TicketsEmployeeViews
# - Requires authenticated employee (via Authenticate())
# - Redirects service desk or unauthenticated users to Login

# Ticket List (Index):
# - Shows creator name, title, status, created/updated dates, log count

# Create Ticket:
# - Collects title and description
# - Sets CreatedById, CreatedAt, UpdatedAt, Status=Open, initializes Logs

# Edit Ticket:
# - Employees can edit only their own tickets (title, description)
# - Updates UpdatedAt

# Delete Ticket:
# - Allowed only if user owns ticket and there are no logs


# =====================================================================
# Timofii Sadko - 732456
# =====================================================================

# Individual Task:
# Built the ticket request system allowing:
# - Managers to send requests to subordinates
# - Employees to redirect tickets they can't solve

# Example Workflow:
# - Employee can't log in -> sends complaint
# - Service desk tries but fails -> redirects to IT
# - IT tries but fails -> redirects to supervisor
# - Supervisor solves and closes ticket

# Individual tasks completed:
# - Added TicketRequest model, controller, service, repository
# - Implemented all methods and fields (including sorting)
# - Created CSHTML views for TicketRequests (except CSS)
# - Added sorting to TicketRequest pages

# Team tasks:
# - Created base project
# - Added login/logout, with Authorization and Hasher classes
# - Added Ticket model and main fields for it
# - Added Ticket repository and most of its methods
# - Added TicketsServiceDeskEmployee Controller, Service and views for it
# - Added all of the CSHTML for TicketServiceDeskEmployee (except CSS and sorting)


# =====================================================================
# Fernando Vazquez Juarez - 737242
# =====================================================================

# Team Task:
# CRUD for Employees using:
# - Employee model
# - EmployeesController
# - Views for all actions
# - EmployeeService for logic handling
#
# Created “managed-by” page:
# - Shows employees under a Service Desk Employee
# - A Service Desk Employee can have many employees
# - A regular employee cannot manage others
#
# Used Hasher to differentiate employee types.

# Individual Task:
# Implemented Forget Password functionality:
# - Multiple views for each step
# - Logic handled in HomeController
# - Uses GET/POST ForgotPassword and ResetPassword
# - Works with Hasher + ResetPasswordViewModel
#
# Results:
# - Employees have role-based rights
# - Passwords hashed
# - Password reset possible through simulated email flow
# - Database updates stored in MongoDB


# =====================================================================
# Sultan Al-Salemi - 725319
# =====================================================================

# Individual Feature: Priority Sorting System
# - Created PriorityHelper class
# - Implemented color-coded badges and emoji indicators
# - Added sorting (asc/desc) with date tiebreaker
# - Integrated sorting across controllers, repository, and views
# - Clickable column headers
# - Persistent sorting preferences

# Technical challenges solved:
# - MongoDB compound sorting (priority + date)
# - State persistence
# - Accessible color selection

# UI Design (with Tarek):
# - Gradient theme (purple-blue)
# - Priority color system
# - Card layouts with shadows and rounded corners
# - CSS variables, responsive, animations

# Impact:
# - Urgent tickets easy to identify
# - Consistent sorting behavior
# - Clear visual hierarchy
# - Maintainable, reusable structure


# =====================================================================
# Student: Tarek Abdalla - 720365
# =====================================================================

# Individual Contribution Overview:
# Developed features in Ticket Management and Employee Management:
# - Sorting Tickets
# - Escalate/Close Ticket actions
# - Employee update and synchronization improvements

# 1. Sorting Tickets:
# - Server-side sorting on: Title, Status, CreatedAt, UpdatedAt
# - No JavaScript needed
# - Helps prioritize urgent tickets

# 2. Escalate / Close Actions:
# - Secure POST actions
# - Escalate: sends ticket to higher support level
# - Close: marks ticket as resolved
# - Confirmation dialogs for safety
# - Real-time updates
# - Demo: http://localhost:5192/TicketsServiceDesk

# 3. Employee Update Improvements:
# - Ensured proper synchronization between DB and in-memory models
# - Improved maintainability

# Results:
# - Dynamic sorting for tickets & employees
# - Instant escalate/close workflow
# - Stable data synchronization
# - Backend-driven, clean implementation (C# MVC + MongoDB)