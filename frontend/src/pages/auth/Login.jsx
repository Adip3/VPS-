import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { useAuth } from '../../store/auth'
import toast from 'react-hot-toast'

export default function Login() {
  const [email, setEmail] = useState('admin@vps.com')
  const [password, setPassword] = useState('Admin@123')
  const login = useAuth(s => s.login)
  const nav = useNavigate()

  const submit = async (e) => {
    e.preventDefault()
    try {
      const { data } = await api.post('/auth/login', { email, password })
      login(data)
      toast.success('Welcome back')
      nav(data.role === 'Admin' ? '/admin' : data.role === 'Staff' ? '/staff' : '/customer')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Login failed')
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-600 to-blue-700 p-6">
      <form onSubmit={submit} className="bg-white p-8 rounded-2xl shadow-xl w-full max-w-md">
        <h1 className="text-2xl font-bold mb-1">Vehicle Parts System</h1>
        <p className="text-sm text-slate-500 mb-6">Sign in to your account</p>
        <label className="text-sm font-medium">Email</label>
        <input className="input mb-3 mt-1" value={email} onChange={e => setEmail(e.target.value)} />
        <label className="text-sm font-medium">Password</label>
        <input className="input mb-5 mt-1" type="password" value={password} onChange={e => setPassword(e.target.value)} />
        <button className="btn-primary w-full">Login</button>
        <p className="text-sm text-center mt-4">No account? <a href="/register" className="text-indigo-600">Register</a></p>
        <p className="text-xs text-slate-400 mt-4 text-center">
          Test: admin@vps.com / Admin@123 — staff@vps.com / Staff@123
        </p>
      </form>
    </div>
  )
}
