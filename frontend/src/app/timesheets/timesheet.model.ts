export interface Timesheet {
  id: string;
  employeeId: string;
  employeeName: string;
  projectId: string;
  projectName: string;
  date: string;
  hoursWorked: number;
  notes: string | null;
  createdAt: string;
}

export interface CreateTimesheetRequest {
  employeeId: string;
  projectId: string;
  date: string;
  hoursWorked: number;
  notes: string | null;
}

export interface UpdateTimesheetRequest {
  hoursWorked: number;
  notes: string | null;
}
