# Contract Monthly Claim System (CMCS)

## Project Overview
CMCS is a web-based application designed to manage lecturers' monthly claims. The system allows lecturers to submit claims, track their status, and upload supporting documents. Managers and coordinators can review, approve, or reject claims efficiently.

## Key Features
- **Lecturer Dashboard:** Submit new claims, upload documents, and view submitted claims.
- **Claim History:** Track status of past claims.
- **Manager Dashboard:** Review, approve, or reject submitted claims.
- **Coordinator Dashboard:** Manage claims workflow and monitor approvals.

## Technology Stack
- ASP.NET Core MVC
- Razor Views (.cshtml)
- Bootstrap 5 for styling
- C# for backend logic

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/tshepsioSinyosi/IIEMSA-PROG6212_TSHEPISOSINYOSI-PART1.git
here is a visual representation of the lecturer dashboard <img width="900" height="381" alt="image" src="https://github.com/user-attachments/assets/b033edb3-0f57-4669-aaa1-bc31c4228a40" />
PART2


CTREDENTIALS FOR LOGIN 


Role	Email	Password	Expected Redirect
Coordinator	coord1@claims.com	Coord@123	CoordinatorDashboard
Lecturer	lecturer1@claims.com	Lect@123	LecturerDashboard
Manager	manager@claims.com	Manager@123	ManagerDashboard

PART2 "YOUTUBE VIDEO LINK
	YOUTUBE VIDEO :https://youtu.be/g6UTeBxDR-s


PART2 Additions – Contract Monthly Claims System



1️⃣ Claim Submission & Management

Lecturers can submit monthly claims for work done, including hours worked, hourly rate, and optional supporting documents.

Claims are tracked with statuses: Pending, Approved, or Rejected.

Lecturers can view, edit (if pending), and delete their submitted claims.

Supporting documents (PDFs, images, etc.) are uploaded and linked to each claim.

2️⃣ Dashboard Views

Lecturer Dashboard: View all personal claims with status and total amounts.

Coordinator Dashboard: View all pending claims, with ability to approve or reject claims.

Manager Dashboard: View all claims, with summary statistics and status filtering.

3️⃣ Claim Approval Workflow

Coordinators and managers can approve or reject claims.

Claim statuses update in real-time and are visible to the lecturer.

Notifications are displayed on successful actions using TempData messages.

4️⃣ Authentication & Authorization

Only authenticated users can access the system.

Role-based access ensures:

Lecturers can submit and manage personal claims.

Coordinators and managers can review and process claims.

5️⃣ File Handling

Supporting documents are stored using a file storage service (local or cloud).

Lecturers can attach multiple files per claim.

Coordinators and managers can view uploaded files when reviewing claims.


