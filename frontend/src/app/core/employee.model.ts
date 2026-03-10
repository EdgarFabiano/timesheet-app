export interface Employee {
  id: string;
  azureAdObjectId: string;
  fullName: string;
  email: string;
  department: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateEmployeeRequest {
  azureAdObjectId: string;
  fullName: string;
  email: string;
  department: string;
  isActive?: boolean;
}

export interface UpdateEmployeeRequest {
  azureAdObjectId: string;
  fullName: string;
  email: string;
  department: string;
  isActive: boolean;
}
