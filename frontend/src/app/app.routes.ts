import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login.component';
import { DashboardComponent } from './pages/dashboard.component';
import { ProductsComponent } from './pages/products.component';
import { StockComponent } from './pages/stock.component';
import { CategoriesComponent } from './pages/categories.component';
import { WarehousesComponent } from './pages/warehouses.component';
import { MovementsComponent } from './pages/movements.component';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'products', component: ProductsComponent, canActivate: [authGuard] },
  { path: 'stock', component: StockComponent, canActivate: [authGuard] },
  { path: 'categories', component: CategoriesComponent, canActivate: [authGuard] },
  { path: 'warehouses', component: WarehousesComponent, canActivate: [authGuard] },
  { path: 'movements', component: MovementsComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
