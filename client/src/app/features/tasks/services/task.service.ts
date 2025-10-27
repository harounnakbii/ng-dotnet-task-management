import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';


export interface CreateTaskDto {
    title: string;
    description: string;
    dueDate: Date;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

export interface TaskDto {
    id?: string | null;
    title?: string | null;
    description?: string | null;
    dueDate?: Date | null;
    isCompleted?: boolean | null;
    userId?: string | null;
    createdAt?: Date | null;
    updatedAt?: Date | null;
}

export interface TaskFilterParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
  isCompleted?: boolean;
  dueDateFrom?: Date;
  dueDateTo?: Date;
  searchTerm?: string;
}

@Injectable({
    providedIn: 'root'
})
export class TaskService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/tasks`;


    getTasksPaged(filters: TaskFilterParams): Observable<PagedResult<TaskDto>> {
        let params = new HttpParams()
            .set('pageNumber', filters.pageNumber.toString())
            .set('pageSize', filters.pageSize.toString());

        if (filters.sortBy) {
            params = params.set('sortBy', filters.sortBy);
        }

        if (filters.sortDescending !== undefined) {
            params = params.set('sortDescending', filters.sortDescending.toString());
        }

        if (filters.isCompleted !== undefined && filters.isCompleted !== null) {
            params = params.set('isCompleted', filters.isCompleted.toString());
        }

        if (filters.dueDateFrom) {
            params = params.set('dueDateFrom', filters.dueDateFrom.toISOString());
        }

        if (filters.dueDateTo) {
            params = params.set('dueDateTo', filters.dueDateTo.toISOString());
        }

        if (filters.searchTerm) {
            params = params.set('searchTerm', filters.searchTerm);
        }

        return this.http.get<PagedResult<TaskDto>>(`${this.apiUrl}/paged`, { params });
    }

    /**
     * Crée une tâche (userId automatique via JWT)
     */
    createTask(task: TaskDto): Observable<TaskDto> {
        return this.http.post<TaskDto>(this.apiUrl, task);
        // Le token JWT est automatiquement ajouté par l'interceptor
        // Le backend extrait le userId du token
    }

    /**
     * Récupère toutes les tâches de l'utilisateur connecté
     */
    getTasks(): Observable<TaskDto[]> {
        return this.http.get<TaskDto[]>(this.apiUrl);
        // Le backend filtre automatiquement par userId du token
    }

    /**
     * Récupère une tâche spécifique
     */
    getTaskById(id: string): Observable<TaskDto> {
        return this.http.get<TaskDto>(`${this.apiUrl}/${id}`);
    }

    /**
     * Met à jour une tâche
     */
    updateTask(id: string, task: TaskDto): Observable<TaskDto> {
        return this.http.put<TaskDto>(`${this.apiUrl}/${id}`, task);
    }

    /**
     * Supprime une tâche
     */
    deleteTask(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    /**
     * Toggle le statut de complétion
     */
    toggleCompletion(id: string): Observable<TaskDto> {
        return this.http.patch<TaskDto>(`${this.apiUrl}/${id}/toggle`, {});
    }
}