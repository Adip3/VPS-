import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Vehicles() {
  const [list, setList] = useState([])
  const [f, setF] = useState({ plateNo: '', make: '', model: '', year: '', mileageKm: 0 })
  const [predict, setPredict] = useState(null)
  const [partType, setPartType] = useState('Brake')

  const load = () => api.get('/profile/vehicles').then(r => setList(r.data))
  useEffect(() => { load() }, [])

  const add = async (e) => {
    e.preventDefault()
    try {
      await api.post('/profile/vehicles', { ...f, year: f.year ? +f.year : null, mileageKm: +f.mileageKm })
      toast.success('Vehicle added'); setF({ plateNo: '', make: '', model: '', year: '', mileageKm: 0 }); load()
    } catch { toast.error('Failed') }
  }

  const runPredict = async (vehicleId) => {
    try {
      const { data } = await api.get(`/profile/predict-failure/${vehicleId}?partType=${partType}&monthsInstalled=12&avgLifespanKm=50000`)
      setPredict({ vehicleId, ...data })
    } catch { toast.error('Prediction failed') }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">My Vehicles</h1>
      <form onSubmit={add} className="card mb-5">
        <h2 className="font-semibold mb-3">Add Vehicle</h2>
        <div className="grid grid-cols-1 md:grid-cols-5 gap-2">
          <input className="input" placeholder="Plate" required value={f.plateNo} onChange={e => setF({ ...f, plateNo: e.target.value })} />
          <input className="input" placeholder="Make" value={f.make} onChange={e => setF({ ...f, make: e.target.value })} />
          <input className="input" placeholder="Model" value={f.model} onChange={e => setF({ ...f, model: e.target.value })} />
          <input className="input" placeholder="Year" type="number" value={f.year} onChange={e => setF({ ...f, year: e.target.value })} />
          <input className="input" placeholder="Mileage km" type="number" value={f.mileageKm} onChange={e => setF({ ...f, mileageKm: e.target.value })} />
        </div>
        <button className="btn-primary mt-3">Add</button>
      </form>

      <div className="card">
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-semibold">My Garage</h2>
          <select className="input w-40" value={partType} onChange={e => setPartType(e.target.value)}>
            <option>Brake</option><option>Engine</option><option>Filter</option><option>Battery</option><option>Tyre</option>
          </select>
        </div>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Plate</th><th>Make/Model</th><th>Year</th><th>Mileage</th><th>AI Failure Predict</th></tr></thead>
          <tbody>
            {list.map(v => <tr key={v.id} className="border-t">
              <td className="p-2 font-mono">{v.plateNo}</td><td>{v.make} {v.model}</td>
              <td>{v.year}</td><td>{v.mileageKm} km</td>
              <td><button className="btn-ghost text-xs" onClick={() => runPredict(v.id)}>Predict {partType}</button></td>
            </tr>)}
          </tbody>
        </table>
        {predict && <div className="mt-4 p-3 bg-indigo-50 rounded-lg">
          <div className="text-sm">Vehicle #{predict.vehicleId} — Failure probability: <span className="font-bold">{(predict.failureProbability * 100).toFixed(1)}%</span></div>
          <div className="text-sm text-indigo-700 mt-1">{predict.recommendation}</div>
        </div>}
      </div>
    </div>
  )
}
