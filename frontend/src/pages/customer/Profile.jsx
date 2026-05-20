import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Profile() {
  const [me, setMe] = useState(null)
  const [edit, setEdit] = useState(false)
  const [form, setForm] = useState({ fullName: '', address: '', phone: '' })

  const load = () => api.get('/profile').then(r => { setMe(r.data); setForm({ fullName: r.data.fullName, address: r.data.address || '', phone: r.data.phone || '' }) })
 useEffect(() => { load() }, [])
  const save = async () => {
    try { await api.put('/profile', form); toast.success('Saved'); setEdit(false); load() }
    catch { toast.error('Save failed') }
  }

  if (!me) return <div>Loading...</div>

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">My Profile</h1>
      <div className="card max-w-xl">
        {edit ? <>
          <label className="text-xs">Full Name</label>
          <input className="input mb-2" value={form.fullName} onChange={e => setForm({ ...form, fullName: e.target.value })} />
          <label className="text-xs">Phone</label>
          <input className="input mb-2" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
          <label className="text-xs">Address</label>
          <input className="input mb-3" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
          <div className="flex gap-2">
            <button className="btn-primary" onClick={save}>Save</button>
            <button className="btn-ghost" onClick={() => setEdit(false)}>Cancel</button>
          </div>
        </> : <>
          <div className="grid grid-cols-2 gap-3 text-sm">
            <Field label="Full Name" value={me.fullName} />
            <Field label="Email" value={me.email} />
            <Field label="Phone" value={me.phone} />
            <Field label="Address" value={me.address} />
            <Field label="National ID" value={me.nationalId} />
          </div>
          <button className="btn-primary mt-4" onClick={() => setEdit(true)}>Edit</button>
        </>}
      </div>
    </div>
  )
}

function Field({ label, value }) {
  return (
    <div>
      <div className="text-xs text-slate-500">{label}</div>
      <div className="font-medium">{value || '—'}</div>
    </div>
  )
}
