import { useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Customers() {
  const [q, setQ] = useState('')
  const [results, setResults] = useState([])
  const [show, setShow] = useState(false)
  const [form, setForm] = useState({ email: '', password: '', fullName: '', phone: '', address: '', nationalId: '', vehicles: [{ plateNo: '', make: '', model: '', year: '', mileageKm: 0 }] })

  const search = async () => {
    try { const { data } = await api.get(`/customers/search?q=${encodeURIComponent(q)}`); setResults(data) }
    catch { toast.error('Search failed') }
  }

  const submit = async (e) => {
    e.preventDefault()
    try {
      const payload = {
        ...form,
        vehicles: form.vehicles.filter(v => v.plateNo).map(v => ({ ...v, year: v.year ? +v.year : null, mileageKm: +v.mileageKm }))
      }
      await api.post('/customers', payload)
      toast.success('Customer registered')
      setShow(false)
      setForm({ email: '', password: '', fullName: '', phone: '', address: '', nationalId: '', vehicles: [{ plateNo: '', make: '', model: '', year: '', mileageKm: 0 }] })
    } catch (err) { toast.error(err.response?.data?.error || 'Failed') }
  }

  const addV = () => setForm({ ...form, vehicles: [...form.vehicles, { plateNo: '', make: '', model: '', year: '', mileageKm: 0 }] })
  const updV = (i, k, v) => setForm({ ...form, vehicles: form.vehicles.map((x, idx) => idx === i ? { ...x, [k]: v } : x) })

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Customers</h1>
      <div className="card mb-5">
        <div className="flex gap-2">
          <input className="input" placeholder="Search by name / phone / national id / plate no" value={q} onChange={e => setQ(e.target.value)} />
          <button className="btn-primary" onClick={search}>Search</button>
          <button className="btn-ghost" onClick={() => setShow(!show)}>{show ? 'Cancel' : '+ New Customer'}</button>
        </div>
      </div>

      {show && <form onSubmit={submit} className="card mb-5">
        <h2 className="font-semibold mb-3">Register New Customer</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <input className="input" placeholder="Email" required value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} />
          <input className="input" placeholder="Password" type="password" required value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} />
          <input className="input" placeholder="Full name" required value={form.fullName} onChange={e => setForm({ ...form, fullName: e.target.value })} />
          <input className="input" placeholder="Phone" required value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
          <input className="input" placeholder="Address" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
          <input className="input" placeholder="National Id" value={form.nationalId} onChange={e => setForm({ ...form, nationalId: e.target.value })} />
        </div>
        <h3 className="font-semibold mt-4 mb-2">Vehicles</h3>
        {form.vehicles.map((v, i) => (
          <div key={i} className="grid grid-cols-1 md:grid-cols-5 gap-2 mb-2">
            <input className="input" placeholder="Plate" value={v.plateNo} onChange={e => updV(i, 'plateNo', e.target.value)} />
            <input className="input" placeholder="Make" value={v.make} onChange={e => updV(i, 'make', e.target.value)} />
            <input className="input" placeholder="Model" value={v.model} onChange={e => updV(i, 'model', e.target.value)} />
            <input className="input" placeholder="Year" type="number" value={v.year} onChange={e => updV(i, 'year', e.target.value)} />
            <input className="input" placeholder="Mileage km" type="number" value={v.mileageKm} onChange={e => updV(i, 'mileageKm', e.target.value)} />
          </div>
        ))}
        <button type="button" className="btn-ghost mb-3" onClick={addV}>+ Vehicle</button>
        <div><button className="btn-primary">Register</button></div>
      </form>}

      <div className="card">
        <h2 className="font-semibold mb-3">Results</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Name</th><th>Email</th><th>Phone</th><th>Vehicles</th></tr></thead>
          <tbody>
            {results.map(c => <tr key={c.id} className="border-t">
              <td className="p-2">{c.fullName}</td><td>{c.email}</td><td>{c.phone}</td>
              <td>{c.vehicles.map(v => v.plateNo).join(', ')}</td>
            </tr>)}
            {results.length === 0 && <tr><td colSpan={4} className="p-4 text-center text-slate-500">No results</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}
