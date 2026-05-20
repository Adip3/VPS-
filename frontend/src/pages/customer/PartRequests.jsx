import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function PartRequests() {
  const [list, setList] = useState([])
  const [f, setF] = useState({ partName: '', note: '' })

  const load = () => api.get('/profile/part-requests').then(r => setList(r.data))
  useEffect(() => { load() }, [])

  const submit = async (e) => {
    e.preventDefault()
    try { await api.post('/part-requests', f); toast.success('Request submitted'); setF({ partName: '', note: '' }); load() }
    catch { toast.error('Failed') }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Part Requests</h1>
      <form onSubmit={submit} className="card mb-5">
        <h2 className="font-semibold mb-3">Request Unavailable Part</h2>
        <input className="input mb-2" placeholder="Part name" required value={f.partName} onChange={e => setF({ ...f, partName: e.target.value })} />
        <textarea className="input mb-3" placeholder="Notes / specifications" value={f.note} onChange={e => setF({ ...f, note: e.target.value })} />
        <button className="btn-primary">Submit Request</button>
      </form>

      <div className="card">
        <h2 className="font-semibold mb-3">My Requests</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Part</th><th>Note</th><th>Status</th><th>Date</th></tr></thead>
          <tbody>
            {list.map(r => <tr key={r.id} className="border-t">
              <td className="p-2">{r.partName}</td><td>{r.note}</td>
              <td><span className="badge bg-slate-100">{r.status}</span></td>
              <td>{new Date(r.requestedAt).toLocaleDateString()}</td>
            </tr>)}
          </tbody>
        </table>
      </div>
    </div>
  )
}
