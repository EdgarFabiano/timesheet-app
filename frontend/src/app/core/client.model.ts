export interface Client {
  id: string;
  name: string;
  contactEmail: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateClientRequest {
  name: string;
  contactEmail: string;
  isActive?: boolean;
}

export interface UpdateClientRequest {
  name: string;
  contactEmail: string;
  isActive: boolean;
}
