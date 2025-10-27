import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { InputIconModule } from 'primeng/inputicon';
import { IconFieldModule } from 'primeng/iconfield';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { TaskDto, TaskService, PagedResult } from '../../services/task.service';
import { TaskFormComponent } from '../form/task-form.component';
import { AppMenuitem } from '@/layout/component/app.menuitem';


interface Column {
  field: string;
  header: string;
}

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    RippleModule,
    ToastModule,
    ToolbarModule,
    InputTextModule,
    TagModule,
    InputIconModule,
    IconFieldModule,
    ConfirmDialogModule,
    TooltipModule,
    TaskFormComponent
  ],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css'],
  providers: [MessageService, ConfirmationService]
})
export class TaskListComponent implements OnInit {
  // ===========================
  // PROPRIÉTÉS
  // ===========================
  taskDialogVisible: boolean = false;
  tasks: TaskDto[] = [];
  selectedTask: TaskDto = {};
  selectedTasks: TaskDto[] = [];
  loading: boolean = false;
  totalRecords: number = 0;

  lastLazyLoadEvent: TableLazyLoadEvent = {};
  searchTerm: string = '';

  // Colonnes pour l'export
  cols: Column[] = [];
  exportColumns: any[] = [];

  @ViewChild('dt') dt!: Table;

  // ===========================
  // CONSTRUCTEUR
  // ===========================
  constructor(
    private taskService: TaskService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  // ===========================
  // LIFECYCLE HOOKS
  // ===========================
  ngOnInit(): void {
    this.loading = true;
    this.initializeColumns();
  }

  // ===========================
  // INITIALISATION
  // ===========================
  initializeColumns(): void {
    this.cols = [
      { field: 'title', header: 'Title' },
      { field: 'description', header: 'Description' },
      { field: 'dueDate', header: 'Due Date' },
      { field: 'isCompleted', header: 'Status' },
      { field: 'createdAt', header: 'Created At' }
    ];

    this.exportColumns = this.cols.map(col => ({
      title: col.header,
      dataKey: col.field
    }));
  }

  // ===========================
  // CHARGEMENT DES DONNÉES
  // ===========================
  loadTasks(event: TableLazyLoadEvent): void {
    this.loading = true;
    this.lastLazyLoadEvent = event;

    const filters = {
      pageNumber: (event.first || 0) / (event.rows || 10) + 1,
      pageSize: event.rows || 10,
      sortBy: event.sortField as string || 'dueDate',
      sortDescending: event.sortOrder === -1,
      searchTerm: this.searchTerm || undefined
    };

    this.taskService.getTasksPaged(filters).subscribe({
      next: (result: PagedResult<TaskDto>) => {
        this.tasks = result.items;
        this.totalRecords = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.showError('Failed to load tasks');
        this.loading = false;
      }
    });
  }

  // ===========================
  // FILTRAGE
  // ===========================
  onGlobalFilter(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value;
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
    this.searchDebounceTimer = setTimeout(() => {
      this.loadTasks(this.lastLazyLoadEvent);
    }, 500);
  }

  private searchDebounceTimer: any;

  // ===========================
  // CRUD OPERATIONS - DIALOG
  // ===========================
  openNew(): void {
    this.selectedTask = {};
    this.taskDialogVisible = true;
  }

  editTask(task: TaskDto): void {
    this.selectedTask = { ...task };
    this.taskDialogVisible = true;
  }

  onTaskSave(task: TaskDto): void {
    if (task.id) {
      this.taskService.updateTask(task.id, task).subscribe({
        next: () => {
          this.showSuccess('Task updated successfully');
          this.taskDialogVisible = false;
          this.loadTasks(this.lastLazyLoadEvent);
        },
        error: (error) => {
          console.error('Error updating task:', error);
          this.showError('Failed to update task');
        }
      });
    } else {
      this.taskService.createTask(task).subscribe({
        next: () => {
          this.showSuccess('Task created successfully');
          this.taskDialogVisible = false;
          this.loadTasks(this.lastLazyLoadEvent);
        },
        error: (error) => {
          console.error('Error creating task:', error);
          this.showError('Failed to create task');
        }
      });
    }
  }

  onTaskCancel(): void {
    this.taskDialogVisible = false;
    this.selectedTask = {};
  }

  // ===========================
  // DELETE OPERATIONS
  // ===========================
  deleteTask(task: TaskDto): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${task.title}"?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        if (task.id) {
          this.taskService.deleteTask(task.id).subscribe({
            next: () => {
              this.showSuccess('Task deleted successfully');
              this.loadTasks(this.lastLazyLoadEvent);
            },
            error: (error) => {
              console.error('Error deleting task:', error);
              this.showError('Failed to delete task');
            }
          });
        }
      }
    });
  }

  deleteSelectedTasks(): void {
    if (!this.selectedTasks || this.selectedTasks.length === 0) {
      return;
    }

    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${this.selectedTasks.length} selected task(s)?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        const deletePromises = this.selectedTasks
          .filter(task => task.id)
          .map(task => this.taskService.deleteTask(task.id!).toPromise());

        Promise.all(deletePromises)
          .then(() => {
            this.showSuccess(`${this.selectedTasks.length} task(s) deleted successfully`);
            this.selectedTasks = [];
            this.loadTasks(this.lastLazyLoadEvent);
          })
          .catch((error) => {
            console.error('Error deleting tasks:', error);
            this.showError('Failed to delete some tasks');
          });
      }
    });
  }

  // ===========================
  // TOGGLE COMPLETION
  // ===========================
  toggleTaskCompletion(task: TaskDto): void {
    if (!task.id) return;

    const updatedTask = { ...task, isCompleted: !task.isCompleted };

    this.taskService.updateTask(task.id, updatedTask).subscribe({
      next: () => {
        this.showSuccess(
          `Task marked as ${updatedTask.isCompleted ? 'completed' : 'pending'}`
        );
        this.loadTasks(this.lastLazyLoadEvent);
      },
      error: (error) => {
        console.error('Error toggling task completion:', error);
        this.showError('Failed to update task status');
      }
    });
  }

  // ===========================
  // EXPORT
  // ===========================

  // Ajouter cette méthode pour exporter toutes les données
  exportAllData(): void {
    this.loading = true;

    const filters = {
      pageNumber: 1,
      pageSize: this.totalRecords || 10000,
      sortBy: 'dueDate',
      sortDescending: false
    };

    this.taskService.getTasksPaged(filters).subscribe({
      next: (result: PagedResult<TaskDto>) => {
        this.generateCSV(result.items);
        this.loading = false;
        this.showSuccess('Export completed successfully');
      },
      error: (error) => {
        console.error('Error exporting tasks:', error);
        this.showError('Failed to export tasks');
        this.loading = false;
      }
    });
  }

  private generateCSV(tasks: TaskDto[]): void {
    // Headers
    const headers = ['Title', 'Description', 'Due Date', 'Status', 'Created At'];

    // Data rows
    const rows = tasks.map(task => [
      this.escapeCSV(task.title || ''),
      this.escapeCSV(task.description || ''),
      task.dueDate ? new Date(task.dueDate).toLocaleString('en-US') : '',
      task.isCompleted ? 'Completed' : 'Pending',
      task.createdAt ? new Date(task.createdAt).toLocaleString('en-US') : ''
    ]);

    // Combine headers and rows
    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.join(','))
    ].join('\n');

    // Create blob and download
    const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', `tasks_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }

  private escapeCSV(value: string): string {
    if (!value) return '';

    // Échapper les guillemets doubles
    value = value.replace(/"/g, '""');

    // Entourer de guillemets si contient virgule, guillemet ou retour à la ligne
    if (value.includes(',') || value.includes('"') || value.includes('\n')) {
      return `"${value}"`;
    }

    return value;
  }

  // ===========================
  // HELPERS
  // ===========================
  getStatusSeverity(isCompleted: boolean | undefined): 'success' | 'warn' | 'danger' | 'info' {
    return isCompleted ? 'success' : 'warn';
  }

  getStatusLabel(isCompleted: boolean | undefined): string {
    return isCompleted ? 'Completed' : 'Pending';
  }

  getStatusIcon(isCompleted: boolean | undefined): string {
    return isCompleted ? 'pi pi-check-circle' : 'pi pi-clock';
  }

  formatDate(date: Date | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isOverdue(task: TaskDto): boolean {
    if (!task.dueDate || task.isCompleted) return false;
    return new Date(task.dueDate) < new Date();
  }

  getDaysUntilDue(task: TaskDto): number {
    if (!task.dueDate) return 0;
    const now = new Date();
    const due = new Date(task.dueDate);
    const diffTime = due.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  // ===========================
  // MESSAGES
  // ===========================
  private showSuccess(detail: string): void {
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: detail,
      life: 3000
    });
  }

  private showError(detail: string): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Error',
      detail: detail,
      life: 3000
    });
  }

  private showInfo(detail: string): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Info',
      detail: detail,
      life: 3000
    });
  }

  // ===========================
  // CLEANUP
  // ===========================
  ngOnDestroy(): void {
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
  }
}