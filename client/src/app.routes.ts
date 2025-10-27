import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { Notfound } from '@/pages/notfound/notfound';
import { Documentation } from '@/pages/documentation/documentation';
import { AuthGuard } from '@/core/guards/auth.guard';

export const appRoutes: Routes = [
    {
        path: '',
        component: AppLayout,
        canActivate : [ AuthGuard],
        children: [
            { path: '', loadChildren: () => import('./app/features/tasks/task.routes') },
            { path: 'documentation', component: Documentation },
            { path: 'tasks', loadChildren: () => import('./app/features/tasks/task.routes') }
        ]
    },
    { path: 'notfound', component: Notfound },
    { path: 'auth', loadChildren: () => import('./app/features/auth/auth.routes') },
    { path: '**', redirectTo: '/notfound' }
];
