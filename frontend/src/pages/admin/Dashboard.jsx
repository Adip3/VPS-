import { useEffect, useState } from 'react'
import api from '../../services/api'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'

export default function AdminDashboard() {
  const [fin, setFin] = useState(null)
  const [low, setLow] = useState([])
  const [parts, setParts] = useState([])

  useEffect(() => {
    api.get('/reports/financial?period=monthly').then(r => setFin(r.data))
    api.get('/inventory/low-stock').then(r => setLow(r.data))
    api.get('/parts').then(r => setParts(r.data))
  }, [])

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Admin Dashboard</h1>
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Stat label="Total Sales (mo)" value={fin ? `NPR ${fin.totalSales}` : '-'} color="bg-green-100 text-green-700" />
        <Stat label="Total Purchases (mo)" value={fin ? `NPR ${fin.totalPurchases}` : '-'} color="bg-blue-100 text-blue-700" />
        <Stat label="Profit (mo)" value={fin ? `NPR ${fin.profit}` : '-'} color="bg-indigo-100 text-indigo-700" />
        <Stat label="Low Stock Items" value={low.length} color="bg-red-100 text-red-700" />
      </div>

      <div className="card mb-6">
        <h2 className="font-semibold mb-3">Stock by Part</h2>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={parts.slice(0, 10)}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <Tooltip />
            <Bar dataKey="stock" fill="#4f46e5" />
          </BarChart>
        </ResponsiveContainer>
      </div>

      <div className="card">
        <h2 className="font-semibold mb-3">Low Stock Alerts (&lt;10)</h2>
        {low.length === 0 ? <p className="text-slate-500 text-sm">All stocked above threshold.</p> :
          <ul className="divide-y">
            {low.map(p => (
              <li key={p.partId} className="py-2 flex justify-between text-sm">
                <span>{p.name} <span className="text-slate-400">({p.sku})</span></span>
                <span className="badge bg-red-100 text-red-700">Stock: {p.stock}</span>
              </li>
            ))}
          </ul>}
      </div>
    </div>
  )
}

function Stat({ label, value, color }) {
  return (
    <div className="card">
      <div className="text-sm text-slate-500">{label}</div>
      <div className={`text-xl font-bold mt-1 inline-block px-2 py-1 rounded ${color}`}>{value}</div>
    </div>
  )
}
