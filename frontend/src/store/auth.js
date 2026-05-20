import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const useAuth = create(persist(
  (set) => ({
    token: null, role: null, email: null, userId: null,
    login: (data) => set({ token: data.token, role: data.role, email: data.email, userId: data.userId }),
    logout: () => set({ token: null, role: null, email: null, userId: null })
  }),
  { name: 'vps-auth' }
))
