import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    title: 'Noor — Modest Atelier',
    loadComponent: () => import('./features/home/home-page').then((m) => m.HomePageComponent)
  },
  {
    path: 'shop',
    title: 'Shop the atelier — Noor',
    loadComponent: () => import('./features/shop/shop-page').then((m) => m.ShopPageComponent)
  },
  {
    path: 'product/:slug',
    title: 'The piece — Noor',
    loadComponent: () =>
      import('./features/product/product-page').then((m) => m.ProductPageComponent)
  },
  {
    path: 'cart',
    title: 'Your bag — Noor',
    loadComponent: () => import('./features/cart/cart-page').then((m) => m.CartPageComponent)
  },
  {
    path: 'checkout',
    title: 'Checkout — Noor',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/checkout/checkout-page').then((m) => m.CheckoutPageComponent)
  },
  {
    path: 'login',
    title: 'Sign in — Noor',
    canActivate: [guestGuard],
    data: { mode: 'login' },
    loadComponent: () => import('./features/auth/auth-page').then((m) => m.AuthPageComponent)
  },
  {
    path: 'register',
    title: 'Create an account — Noor',
    canActivate: [guestGuard],
    data: { mode: 'register' },
    loadComponent: () => import('./features/auth/auth-page').then((m) => m.AuthPageComponent)
  },
  {
    path: 'account',
    title: 'My account — Noor',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/account/account-page').then((m) => m.AccountPageComponent)
  },
  {
    path: 'account/orders/:id',
    title: 'Order details — Noor',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/account/order-detail-page').then((m) => m.OrderDetailPageComponent)
  },
  {
    path: 'account/:section',
    title: 'My account — Noor',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/account/account-page').then((m) => m.AccountPageComponent)
  },
  {
    path: 'our-story',
    title: 'Our story — Noor',
    data: { page: 'story' },
    loadComponent: () => import('./features/info/info-page').then((m) => m.InfoPageComponent)
  },
  {
    path: 'contact',
    title: 'Contact — Noor',
    data: { page: 'contact' },
    loadComponent: () => import('./features/info/info-page').then((m) => m.InfoPageComponent)
  },
  {
    path: 'shipping-returns',
    title: 'Shipping & returns — Noor',
    data: { page: 'shipping' },
    loadComponent: () => import('./features/info/info-page').then((m) => m.InfoPageComponent)
  },
  {
    path: 'size-guide',
    title: 'Size guide — Noor',
    data: { page: 'sizes' },
    loadComponent: () => import('./features/info/info-page').then((m) => m.InfoPageComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/admin/admin-layout').then((m) => m.AdminLayoutComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      },
      {
        path: 'dashboard',
        title: 'Dashboard — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/dashboard-page').then((m) => m.AdminDashboardPageComponent)
      },
      {
        path: 'products',
        title: 'Products — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/products-page').then((m) => m.AdminProductsPageComponent)
      },
      {
        path: 'products/new',
        title: 'New product — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/product-form-page').then(
            (m) => m.AdminProductFormPageComponent
          )
      },
      {
        path: 'products/:id/edit',
        title: 'Edit product — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/product-form-page').then(
            (m) => m.AdminProductFormPageComponent
          )
      },
      {
        path: 'categories',
        title: 'Collections — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/categories-page').then((m) => m.AdminCategoriesPageComponent)
      },
      {
        path: 'orders',
        title: 'Orders — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/orders-page').then((m) => m.AdminOrdersPageComponent)
      },
      {
        path: 'orders/:id',
        title: 'Order — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/order-detail-page').then(
            (m) => m.AdminOrderDetailPageComponent
          )
      },
      {
        path: 'customers',
        title: 'Clients — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/customers-page').then((m) => m.AdminCustomersPageComponent)
      },
      {
        path: 'coupons',
        title: 'Coupons — Noor Atelier',
        loadComponent: () =>
          import('./features/admin/coupons-page').then((m) => m.AdminCouponsPageComponent)
      }
    ]
  },
  {
    path: '**',
    title: 'Not found — Noor',
    loadComponent: () =>
      import('./features/not-found/not-found-page').then((m) => m.NotFoundPageComponent)
  }
];
