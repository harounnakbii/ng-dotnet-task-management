import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { TaskDto } from '../../services/task.service';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    DatePickerModule
  ],
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.css']
})
export class TaskFormComponent implements OnChanges {
  // ===========================
  // INPUTS & OUTPUTS
  // ===========================
  @Input() visible: boolean = false;
  @Input() task: TaskDto = {};
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() onSave = new EventEmitter<TaskDto>();
  @Output() onCancel = new EventEmitter<void>();

  // ===========================
  // PROPRIÉTÉS
  // ===========================
  submitted: boolean = false;
  localTask: TaskDto = {};

  // Date minimale = maintenant
  minDate: Date = new Date();

  statuses = [
    { label: 'Pending', value: false },
    { label: 'Completed', value: true }
  ];

  // ===========================
  // LIFECYCLE HOOKS
  // ===========================
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['task'] && changes['task'].currentValue) {
      this.localTask = { ...this.task };
      this.submitted = false;
    }

    if (changes['visible'] && !changes['visible'].currentValue) {
      this.resetForm();
    }
  }

  // ===========================
  // DIALOG MANAGEMENT
  // ===========================
  onDialogHide(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.resetForm();
    this.onCancel.emit();
  }

  // ===========================
  // FORM ACTIONS
  // ===========================
  save(): void {
    this.submitted = true;

    if (this.isFormValid()) {
      this.onSave.emit(this.localTask);
      this.resetForm();
    }
  }

  cancel(): void {
    this.onDialogHide();
  }

  // ===========================
  // VALIDATION
  // ===========================
  isFormValid(): boolean {
    const titleValid = !!this.localTask.title?.trim();
    const dueDateValid = this.isDueDateValid();

    return titleValid && dueDateValid;
  }

  isDueDateValid(): boolean {
    // Si pas de due date, c'est valide (optionnel)
    if (!this.localTask.dueDate) {
      return true;
    }

    // Vérifier que la date est >= maintenant
    const selectedDate = new Date(this.localTask.dueDate);
    const now = new Date();

    // Réinitialiser l'heure pour comparer uniquement les dates
    now.setHours(0, 0, 0, 0);
    selectedDate.setHours(0, 0, 0, 0);

    return selectedDate >= now;
  }

  get dueDateErrorMessage(): string {
    if (this.submitted && this.localTask.dueDate && !this.isDueDateValid()) {
      return 'Due date cannot be in the past';
    }
    return '';
  }

  // ===========================
  // HELPERS
  // ===========================
  private resetForm(): void {
    this.submitted = false;
    this.localTask = {};
  }

  get dialogHeader(): string {
    return this.localTask.id ? 'Edit Task' : 'New Task';
  }

  get isEditMode(): boolean {
    return !!this.localTask.id;
  }
}