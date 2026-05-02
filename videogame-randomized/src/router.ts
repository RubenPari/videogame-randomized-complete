import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'
import MainLayout from '@/layouts/MainLayout.vue'
import HomeView from '@/views/HomeView.vue'
import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import ForgotPasswordView from '@/views/ForgotPasswordView.vue'
import ResetPasswordView from '@/views/ResetPasswordView.vue'
import ConfirmEmailView from '@/views/ConfirmEmailView.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: MainLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Home',
        component: HomeView,
      },
    ],
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { guest: true },
  },
  {
    path: '/register',
    name: 'Register',
    component: RegisterView,
    meta: { guest: true },
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: ForgotPasswordView,
    meta: { guest: true },
  },
  {
    path: '/reset-password',
    name: 'ResetPassword',
    component: ResetPasswordView,
    meta: { guest: true },
  },
  {
    path: '/confirm-email',
    name: 'ConfirmEmail',
    component: ConfirmEmailView,
    meta: { guest: true },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  authStore.loadFromStorage()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'Login' }
  }

  if (to.meta.guest && authStore.isAuthenticated) {
    return { name: 'Home' }
  }
})

export default router
