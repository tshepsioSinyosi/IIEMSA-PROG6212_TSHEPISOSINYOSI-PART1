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

...Login Credentials (Demo Accounts)...
Role	Email	Password	Redirect Page
Coordinator	coord1@claims.com
	Coord@123	CoordinatorDashboard
Lecturer	lecturer1@claims.com
	Lect@123	LecturerDashboard
Manager	manager@claims.com
	Manager@123	ManagerDashboard
HR (NEW)	hruser@claims.com
	Hr@123	HRDashboard

7. PART 3 – Automation Enhancements (POE Requirements)

These improvements elevate the system with automation, validation, and streamlined processing.

✨ Lecturer View Automation
“The auto-calculation feature and validation checks are implemented exceptionally well, ensuring accurate and comprehensive claim submissions.”

✔ Automated total calculation:
Total Amount = Hours × Rate (calculated instantly)
✔ Validation checks before submission:

Required fields

Valid hours

Valid rate

Prevent duplicate submissions for the same month
✔ Real-time visual feedback
✔ Auto-display of claim totals on the dashboard

✨ Coordinator & Manager Automation
“Automated verification and approval processes streamline and ensure error-free claim processing.”

✔ Automatic verification checks:

Ensures claim fields are valid

Ensures documents are attached where required

Highlights missing data or inconsistencies

✔ Coordinators/Managers can approve/reject in one click
✔ Automatic status updates
✔ Automated logging of approval actions
✔ Dashboards update instantly after decisions

✨ HR Automation (NEW FEATURE)
“Automation of claim processing and lecturer data management significantly streamlines administrative processes.”

✔ Automatic retrieval of all approved claims
✔ Auto-calculation:

Total hours per lecturer

Total payout per lecturer

Grand totals for payroll

✔ HR Summary Reports automatically generated
✔ Auto-sync lecturer details (email, phone, etc.)
✔ Error-handling ensures missing lecturers don’t break the system
✔ HR can update lecturer details through automated forms
✔ Eliminates manual payroll calculations

8. Visual Representation
   
<img width="1912" height="1002" alt="image" src="https://github.com/user-attachments/assets/b2a2d944-d789-45c4-b4bf-28a75524f030" />

10. Video Demonstration

YouTube Link:

🔗 

10. Conclusion

The Contract Monthly Claim System provides:

A fully automated workflow

Accurate financial calculations

Streamlined approval pipeline

Better communication between lecturers and administration

Reduced manual work for HR

A secure, modern, and user-friendly digital claims platform

The latest automation upgrades further enhance system performance and efficiency, aligning with the POE requirements by demonstrating automation across Lecturer, Coordinator/Manager, and HR views.
