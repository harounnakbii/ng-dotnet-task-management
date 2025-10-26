export interface UserDto {
  id: string;
  name: string;
  email: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface RegisterDto {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface RegisterResponse {
  id: string;
  name: string;
  email: string;
  createdAt: Date;
}

export interface ErrorResponse {
  message: string;
  errors?: { [key: string]: string[] };
}