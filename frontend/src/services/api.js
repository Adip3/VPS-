import axios from 'axios'
import { useAuth } from '../store/auth'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5050/api'
})

api.interceptors.request.use(cfg => {
  const t = useAuth.getState().token
  if (t) cfg.headers.Authorization = `Bearer ${t}`
  return cfg
})

api.interceptors.response.use(
  r => r,
  err => {
    if (err.response?.status === 401) {
      useAuth.getState().logout()
    }
    return Promise.reject(err)
  }
)

export default api
