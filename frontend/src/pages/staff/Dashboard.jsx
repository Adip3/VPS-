import { useEffect, useState } from 'react'
import api from '../../services/api'

export default function StaffDashboard() {
  const [sales, setSales] = useState([])
  useEffect(() => { api.get('/sales-invoices').then(r => setSales(r.data.slice(0, 10))) }, [])

  const total = sales.reduce((s, i) => s + i.total, 0)
  const credit = sales.filter(s => s.paymentStatus === 'Credit').length

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Staff Dashboard</h1>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="card"><div className="text-sm text-slate-500">Recent Sales</div><div className="text-xl font-bold">{sales.length}</div></div>
        <div className="card"><div className="text-sm text-slate-500">Recent Total</div><div className="text-xl font-bold text-green-600">NPR {total}</div></div>
        <div className="card"><div className="text-sm text-slate-500">Credit Sales</div><div className="text-xl font-bold text-orange-600">{credit}</div></div>
      </div>
      <div className="card">
        <h2 className="font-semibold mb-3">Recent Invoices</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Id</th><th>Customer</th><th>Date</th><th>Total</th><th>Status</th></tr></thead>
          <tbody>
            {sales.map(s => <tr key={s.id} className="border-t">
              <td className="p-2">#{s.id}</td><td>{s.customerName}</td>
              <td>{new Date(s.date).toLocaleDateString()}</td>
              <td>NPR {s.total}</td>
              <td><span className={`badge ${s.paymentStatus === 'Paid' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>{s.paymentStatus}</span></td>
            </tr>)}
          </tbody>
        </table>
      </div>
    </div>
  )
}
