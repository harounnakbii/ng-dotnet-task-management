import { Routes } from '@angular/router';
import { Login } from './components/login/login.component';
import { Register } from './components/register/register.component';
import { NoneAuthGuard } from '@/core/guards/none.auth.guard';

export default [
    { path: 'login', component: Login, canActivate : [NoneAuthGuard] },
    { path: 'register', component: Register, canActivate : [NoneAuthGuard]  }
] as Routes;
