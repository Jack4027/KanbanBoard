import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login')
      .then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register')
      .then(m => m.RegisterComponent)
  },
  {
    path: 'boards',
    loadComponent: () => import('./features/boards/boards')
      .then(m => m.BoardsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'boards/:id',
    loadComponent: () => import('./features/board-detail/board-detail')
      .then(m => m.BoardDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];