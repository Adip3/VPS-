import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Appointments() {
  const [list, setList] = useState([])
  const [vehicles, setVehicles] = useState([])
  const [f, setF] = useState({ vehicleId: '', scheduledAt: '', notes: '' })

  const load = () => {
    api.get('/profile/appointments').then(r => setList(r.data))
    api.get('/profile/vehicles').then(r => setVehicles(r.data))
  }
  useEffect(() => { load() }, [])

  const submit = async (e) => {
    e.preventDefault()
    try {
      await api.post('/appointments', { vehicleId: +f.vehicleId, scheduledAt: f.scheduledAt, notes: f.notes })
      toast.success('Appointment booked'); setF({ vehicleId: '', scheduledAt: '', notes: '' }); load()
    } catch (err) { toast.error(err.response?.data?.error || 'Failed') }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Appointments</h1>
      <form onSubmit={submit} className="card mb-5">
        <h2 className="font-semibold mb-3">Book Appointment</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <select className="input" required value={f.vehicleId} onChange={e => setF({ ...f, vehicleId: e.target.value })}>
            <option value="">-- vehicle --</option>
            {vehicles.map(v => <option key={v.id} value={v.id}>{v.plateNo}</option>)}
          </select>
          <input className="input" type="datetime-local" required value={f.scheduledAt} onChange={e => setF({ ...f, scheduledAt: e.target.value })} />
          <input className="input" placeholder="Notes" value={f.notes} onChange={e => setF({ ...f, notes: e.target.value })} />
        </div>
        <button className="btn-primary mt-3">Book</button>
      </form>

      <div className="card">
        <h2 className="font-semibold mb-3">My Appointments</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Plate</th><th>Date</th><th>Status</th><th>Notes</th></tr></thead>
          <tbody>
            {list.map(a => <tr key={a.id} className="border-t">
              <td className="p-2 font-mono">{a.plateNo}</td>
              <td>{new Date(a.scheduledAt).toLocaleString()}</td>
              <td><span className="badge bg-slate-100">{a.status}</span></td>
              <td>{a.notes}</td>
            </tr>)}
            {list.length === 0 && <tr><td colSpan={4} className="p-4 text-center text-slate-500">No appointments</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}
