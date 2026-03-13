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

export interface BulkTimesheetEntry {
  projectId: string;
  date: string;
  hoursWorked: number;
  notes: string | null;
}

export interface BulkSaveRequest {
  employeeId: string;
  entries: BulkTimesheetEntry[];
}

export interface BulkSaveResponse {
  saved: Timesheet[];
  errors: string[];
}

export interface WeekDay {
  date: Date;
  dateStr: string;
  dayName: string;
  dayNumber: number;
}
