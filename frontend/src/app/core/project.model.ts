export interface Project {
  id: string;
  name: string;
  description: string | null;
  startDate: Date;
  endDate: Date | null;
  isActive: boolean;
  createdAt: Date;
  clientId: string;
  clientName: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string | null;
  startDate: Date;
  endDate: Date | null;
  isActive: boolean;
  clientId: string;
}

export interface UpdateProjectRequest {
  name: string;
  description: string | null;
  startDate: Date;
  endDate: Date | null;
  isActive: boolean;
  clientId: string;
}
