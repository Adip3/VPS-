import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { useAuth } from '../../store/auth'
import toast from 'react-hot-toast'

export default function Register() {
  const [f, setF] = useState({ email: '', password: '', fullName: '', phone: '', address: '', nationalId: '' })
  const login = useAuth(s => s.login)
  const nav = useNavigate()

  const set = (k) => (e) => setF({ ...f, [k]: e.target.value })

  const submit = async (e) => {
    e.preventDefault()
    try {
      const { data } = await api.post('/auth/register', f)
      login(data)
      toast.success('Registered')
      nav('/customer')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Registration failed')
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-600 to-blue-700 p-6">
      <form onSubmit={submit} className="bg-white p-8 rounded-2xl shadow-xl w-full max-w-md">
        <h1 className="text-2xl font-bold mb-5">Customer Registration</h1>
        <input className="input mb-2" placeholder="Email" value={f.email} onChange={set('email')} required />
        <input className="input mb-2" type="password" placeholder="Password (min 6)" value={f.password} onChange={set('password')} required />
        <input className="input mb-2" placeholder="Full name" value={f.fullName} onChange={set('fullName')} required />
        <input className="input mb-2" placeholder="Phone" value={f.phone} onChange={set('phone')} />
        <input className="input mb-2" placeholder="Address" value={f.address} onChange={set('address')} />
        <input className="input mb-4" placeholder="National ID" value={f.nationalId} onChange={set('nationalId')} />
        <button className="btn-primary w-full">Register</button>
        <p className="text-sm text-center mt-4">Have account? <a href="/login" className="text-indigo-600">Login</a></p>
      </form>
    </div>
  )
}
