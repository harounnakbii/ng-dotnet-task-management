import { Routes } from '@angular/router';
import { AuthGuard } from '@/core/guards/auth.guard';
import { TaskListComponent } from './components/list/task-list.component';

export default [
    { path: '', component: TaskListComponent, canActivate : [AuthGuard] },
] as Routes;
